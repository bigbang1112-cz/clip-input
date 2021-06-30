using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Control;
using GBX.NET.IO;
using GBX.NET.Engines.MwFoundations;

namespace ClipInput
{
    public class ClipInputTool
    {
        public CMwNod Input { get; }

        public Game GameVersion { get; private set; }

        public Vec2 AspectRatio { get; set; } = (16, 9);
        public Vec2 Scale { get; set; } = (1, 1);
        public Vec2 Space { get; set; } = (1, 1);
        public Vec2 Position { get; set; } = (0, 0);
        public bool ShowAfterInteraction { get; set; }
        public bool CutoffOutside { get; set; } // WIP
        public Vec2 PadOffset { get; set; }
        public Vec4 PadColor { get; set; } = (1, 1, 1, 1);
        public Vec4 PadBrakeColor { get; set; } = (1, 0, 0, 1);
        public Vec3 PadStartPosition { get; set; }
        public Vec3 PadEndPosition { get; set; }
        public Theme Theme { get; set; }
        public TimeSpan StartOffset { get; set; }
        public KeyboardKey[] Keys { get; set; }
        public bool AdjustToFPS { get; set; }
        public float FPS { get; set; } = 30;

        public ClipInputTool(CMwNod node)
        {
            Input = node;

            AspectRatio = (16, 9);
            Scale = (0.5f, 0.5f);
            Space = (1.05f, 1.05f);
            Position = (0, 0.5f);
            ShowAfterInteraction = false;
            PadOffset = (0.385f, 0);
            PadColor = (0.11f, 0.44f, 0.69f, 1);
            PadBrakeColor = (0.69f, 0.18f, 0.11f, 1);
            PadStartPosition = (0.16f, -0.45f, 0);
            PadEndPosition = (0.6f, 0, 0);
            Theme = Theme.Black;

            Keys = new KeyboardKey[]
            {
                new KeyboardKey()
                {
                    EntryName = "Accelerate",
                    TrackName = "Key Up",
                    Position = (0, 0.25f),
                    ImageOff = "{0}_Up.png",
                    ImageOn = "{0}_Up_On.png"
                },
                new KeyboardKey()
                {
                    EntryName = "Brake",
                    TrackName = "Key Down",
                    Position = (0, -0.25f),
                    ImageOff = "{0}_Down.png",
                    ImageOn = "{0}_Down_On.png"
                },
                new KeyboardKey()
                {
                    EntryNames = new string[] { "SteerLeft", "Steer left" },
                    TrackName = "Key Left",
                    Position = (0.28f, -0.25f),
                    ImageOff = "{0}_Left.png",
                    ImageOn = "{0}_Left_On.png",
                    IsSteerInput = true
                },
                new KeyboardKey()
                {
                    EntryNames = new string[] { "SteerRight", "Steer right" },
                    TrackName = "Key Right",
                    Position = (-0.28f, -0.25f),
                    ImageOff = "{0}_Right.png",
                    ImageOn = "{0}_Right_On.png",
                    IsSteerInput = true
                }
            };
        }

        /// <summary>
        /// Processes the program.
        /// </summary>
        /// <exception cref="TMTurboNotSupportedException"/>
        /// <returns>Output result.</returns>
        public CGameCtnMediaClip Process()
        {
            if (Input is CGameCtnReplayRecord replay)
            {
                Console.WriteLine("CGameCtnReplayRecord (Replay.Gbx) detected...");

                if (replay.ControlEntries is null)
                {
                    Console.WriteLine("Control entries not found in the root node...");

                    if (replay.Ghosts is not null && replay.Ghosts.Length > 0)
                    {
                        Console.WriteLine("Ghost found...");

                        return ProcessGhost(replay.Ghosts.FirstOrDefault());
                    }

                    Console.WriteLine("Checking ghosts in Clip...");

                    var ghosts = ExtractGhosts(replay.Clip);

                    return ProcessGhost(ghosts.FirstOrDefault());
                }

                GameVersion = Game.TMUF;

                return ProcessControlEntries(replay.ControlEntries, TimeSpan.FromMilliseconds(replay.EventsDuration));
            }

            if (Input is CGameCtnGhost ghost)
            {
                Console.WriteLine("CGameCtnGhost (Ghost.Gbx) detected...");

                return ProcessGhost(ghost);
            }

            if (Input is CGameCtnMediaClip clip)
            {
                Console.WriteLine("CGameCtnMediaClip (Clip.Gbx) detected...");

                var ghosts = ExtractGhosts(clip);

                return ProcessGhost(ghosts.FirstOrDefault());
            }

            throw new Exception();
        }

        private IEnumerable<CGameCtnGhost> ExtractGhosts(CGameCtnMediaClip clip)
        {
            foreach (var track in clip.Tracks)
                foreach (var block in track.Blocks)
                    if (block is CGameCtnMediaBlockGhost ghostBlock)
                        yield return ghostBlock.GhostModel;
        }

        private CGameCtnMediaClip ProcessGhost(CGameCtnGhost ghost)
        {
            if (ghost == null)
                throw new ArgumentNullException(nameof(ghost));

            Console.WriteLine("Processing CGameCtnGhost...");

            if (ghost.TryGetChunk(out CGameCtnGhost.Chunk03092025 _))
            {
                Console.WriteLine("Chunk 0x025 found.");

                if (ghost.Validate_ExeVersion?.StartsWith("TrackmaniaTurbo") == true)
                {
                    Console.WriteLine("TrackmaniaTurbo in Validate_ExeVersion found.");

                    throw new TMTurboNotSupportedException();
                }
                else
                {
                    Console.WriteLine("The replay comes from ManiaPlanet.");

                    GameVersion = Game.ManiaPlanet;
                }
            }
            else
            {
                Console.WriteLine("The replay comes from TMUF or older game.");

                GameVersion = Game.TMUF;
            }

            return ProcessControlEntries(ghost.ControlEntries, TimeSpan.FromMilliseconds(ghost.EventsDuration));
        }

        private CGameCtnMediaClip ProcessControlEntries(IEnumerable<ControlEntry> entries, TimeSpan eventsDuration)
        {
            if (entries == null)
                throw new NoInputsException();

            Console.WriteLine("Processing the inputs...");

            Console.WriteLine("Creating CGameCtnMediaClip...");

            var clip = new CGameCtnMediaClip()
            {
                LocalPlayerClipEntIndex = -1
            };

            switch (GameVersion)
            {
                case Game.TMUF:
                    Console.WriteLine("Creating CGameCtnMediaClip chunks 0x004, 0x005, 0x007...");
                    clip.CreateChunk<CGameCtnMediaClip.Chunk03079004>();
                    clip.CreateChunk<CGameCtnMediaClip.Chunk03079005>();
                    clip.CreateChunk<CGameCtnMediaClip.Chunk03079007>();
                    break;
                case Game.ManiaPlanet:
                    Console.WriteLine("Creating CGameCtnMediaClip chunk 0x00D...");
                    clip.CreateChunk<CGameCtnMediaClip.Chunk0307900D>();
                    break;
            }

            var tracks = new List<CGameCtnMediaTrack>();

            var hasAccelReal = false;
            var hasBrakeReal = false;
            var hasAnalogSteering = false;

            foreach (var entry in entries)
            {
                switch (entry.Name)
                {
                    case "Accelerate":
                    case "Brake":
                    case "SteerLeft":
                    case "SteerRight":
                    case "Steer left":
                    case "Steer right":
                        break;
                    case "Gas":
                        hasAccelReal = true;
                        hasBrakeReal = true;
                        break;
                    case "AccelerateReal":
                        if ((entry as ControlEntryAnalog).Value >= 0)
                            hasAccelReal = true;
                        break;
                    case "BrakeReal":
                        if ((entry as ControlEntryAnalog).Value >= 0)
                            hasBrakeReal = true;
                        break;
                    case "Steer (analog)":
                    case "Steer":
                        hasAnalogSteering = true;
                        break;
                }
            }

            Console.WriteLine("Processing acceleration input...");
            ProcessDigitalInput(entries, eventsDuration, tracks,
                onlyAcceleration: hasAnalogSteering,
                usesAnalogAccel: hasAccelReal,
                usesAnalogBrake: hasBrakeReal);

            if (hasAnalogSteering)
            {
                Console.WriteLine("Processing analog steering input...");
                ProcessAnalogSteering(entries, eventsDuration, tracks);
            }

            if (hasAccelReal || hasBrakeReal)
            {
                Console.WriteLine("Processing analog acceleration/brake...");

                ProcessAnalogAccel(entries, eventsDuration, tracks,
                    hasAccelReal ? Keys.FirstOrDefault(x => x.EntryName == "Accelerate") : null,
                    hasBrakeReal ? Keys.FirstOrDefault(x => x.EntryName == "Brake") : null);
            }

            // Defines the start of the first event
            var eventStartTime = new TimeSpan();

            if (!ShowAfterInteraction)
                eventStartTime = TimeSpan.FromMilliseconds(Math.Min(0, entries.Min(x => x.Time.TotalMilliseconds)));

            clip.Tracks = tracks;

            if (StartOffset != TimeSpan.Zero)
            {
                Console.WriteLine($"Offsetting the blocks by {StartOffset.TotalSeconds} seconds.");

                foreach (var track in tracks)
                {
                    foreach (var block in track.Blocks)
                    {
                        switch (block)
                        {
                            case CGameCtnMediaBlockImage blockImage:
                                foreach (var key in blockImage.Effect.Keys)
                                    key.Time += StartOffset;
                                break;
                            case CGameCtnMediaBlockTriangles blockTriangles:
                                foreach (var key in blockTriangles.Keys)
                                    key.Time += StartOffset;
                                break;
                            case CGameCtnMediaBlockText blockText:
                                foreach (var key in blockText.Effect.Keys)
                                    key.Time += StartOffset;
                                break;
                        }
                    }
                }
            }

            Console.WriteLine("Building the final GBX file...");

            return clip;
        }

        private void ProcessDigitalInput(IEnumerable<ControlEntry> entries, TimeSpan eventsDuration, IList<CGameCtnMediaTrack> tracks,
            bool onlyAcceleration, bool usesAnalogAccel, bool usesAnalogBrake)
        {
            var trackDictionary = new Dictionary<KeyboardKey, CGameCtnMediaTrack>();
            var currentImageDictionary = new Dictionary<KeyboardKey, CGameCtnMediaBlockImage>();
            var pressedKeyDictionary = new Dictionary<KeyboardKey, bool>();

            // Defines the start of the first event
            var eventStartTime = new TimeSpan();

            if (!ShowAfterInteraction)
                eventStartTime = TimeSpan.FromMilliseconds(Math.Min(0, entries.Min(x => x.Time.TotalMilliseconds)));

            (string imageOff, string imageOn) DetermineImages(KeyboardKey key, bool isSteerInput)
            {
                var imageOff = key.ImageOff;
                var imageOn = key.ImageOn;

                if (!isSteerInput)
                {
                    if (usesAnalogAccel && key.EntryName == "Accelerate")
                    {
                        imageOff = "{0}_Analog.png";
                        imageOn = "{0}_Analog_Accel.png";
                    }

                    if (usesAnalogBrake && key.EntryName == "Brake")
                    {
                        imageOff = "{0}_Analog.png";
                        imageOn = "{0}_Analog_Brake.png";
                    }
                }

                return (
                    string.Format(imageOff, (int)Theme),
                    string.Format(imageOn, (int)Theme)
                );
            }

            foreach (var key in Keys)
            {
                var (imageOff, imageOn) = DetermineImages(key, key.IsSteerInput);

                if (!onlyAcceleration || !key.IsSteerInput)
                {
                    var track = CreateMediaTrack(key.TrackName);
                    trackDictionary[key] = track;

                    currentImageDictionary[key] = null;
                    pressedKeyDictionary[key] = false;

                    if (!ShowAfterInteraction)
                    {
                        var blockImage = CreateImageBlock(imageOff, eventStartTime, key.Position);

                        trackDictionary[key].Blocks.Add(blockImage);
                        currentImageDictionary[key] = blockImage;
                    }
                }
            }

            foreach (var entry in entries)
            {
                foreach (var key in Keys)
                {
                    var (imageOff, imageOn) = DetermineImages(key, key.IsSteerInput);

                    if (!onlyAcceleration || !key.IsSteerInput)
                    {
                        if (key.EntryNames.Contains(entry.Name))
                        {
                            if (entry.IsEnabled)
                            {
                                if (!pressedKeyDictionary[key])
                                {
                                    var time = entry.Time;

                                    if (currentImageDictionary[key] != null)
                                    {
                                        currentImageDictionary[key].Effect.Keys[1] = CreateSimiKey(time, key.Position);
                                    }

                                    var blockImage = CreateImageBlock(imageOn, time, key.Position);

                                    trackDictionary[key].Blocks.Add(blockImage);
                                    currentImageDictionary[key] = blockImage;
                                    pressedKeyDictionary[key] = true;
                                }
                            }
                            else
                            {
                                var prevTime = currentImageDictionary[key].Effect.Keys[0].Time;
                                var time = entry.Time;

                                if (AdjustToFPS)
                                {
                                    var blockLength = time - prevTime;

                                    if (blockLength < TimeSpan.FromSeconds(1 / FPS))
                                        time = prevTime + TimeSpan.FromSeconds(1 / FPS);
                                }

                                currentImageDictionary[key].Effect.Keys[1] = CreateSimiKey(time, key.Position);

                                var blockImage = CreateImageBlock(imageOff, time, key.Position);

                                trackDictionary[key].Blocks.Add(blockImage);
                                currentImageDictionary[key] = blockImage;
                                pressedKeyDictionary[key] = false;
                            }
                        }
                    }
                }
            }

            foreach (var key in Keys)
            {
                if (!onlyAcceleration || !key.IsSteerInput)
                {
                    currentImageDictionary[key].Effect.Keys[1] = CreateSimiKey(eventsDuration, key.Position);

                    tracks.Add(trackDictionary[key]);
                }
            }
        }

        private void ProcessAnalogAccel(IEnumerable<ControlEntry> entries, TimeSpan eventsDuration, IList<CGameCtnMediaTrack> tracks, KeyboardKey accelKey, KeyboardKey brakeKey)
        {
            // Defines the start of the first event
            var eventStartTime = new TimeSpan();

            if (!ShowAfterInteraction)
                eventStartTime = TimeSpan.FromMilliseconds(Math.Min(0, entries.Min(x => x.Time.TotalMilliseconds)));

            var trackAccelPad = CreateMediaTrack("Pad Acceleration");
            tracks.Add(trackAccelPad);

            var trackBrakePad = CreateMediaTrack("Pad Brake");
            tracks.Add(trackBrakePad);

            var accelPadQuad = default(CGameCtnMediaBlockTriangles);
            var brakePadQuad = default(CGameCtnMediaBlockTriangles);

            var lastEntry = entries.Last();

            foreach (var entry in entries)
            {
                if (entry.Equals(lastEntry))
                {
                    CompleteTheTriangle(accelPadQuad, eventsDuration);
                    CompleteTheTriangle(brakePadQuad, eventsDuration);
                }

                if (entry.Name == "Gas" || entry.Name == "AccelerateReal")
                {
                    var analog = (ControlEntryAnalog)entry;

                    CompleteTheTriangle(accelPadQuad, entry.Time);

                    if (analog.Value > 0)
                    {
                        accelPadQuad = CreateKeyPadMoment(accelKey, analog.Value, entry.Time);
                        trackAccelPad.Blocks.Add(accelPadQuad);
                    }
                    else
                    {
                        accelPadQuad = null;
                    }
                }

                if (entry.Name == "Gas" || entry.Name == "BrakeReal")
                {
                    var analog = (ControlEntryAnalog)entry;

                    CompleteTheTriangle(brakePadQuad, entry.Time);

                    if ((analog.Value > 0 && entry.Name != "Gas") || (entry.Name == "Gas" && analog.Value < 0))
                    {
                        brakePadQuad = CreateKeyPadMoment(brakeKey, analog.Value, entry.Time);
                        trackBrakePad.Blocks.Add(brakePadQuad);
                    }
                    else
                    {
                        brakePadQuad = null;
                    }
                }
            }
        }

        private static void CompleteTheTriangle(CGameCtnMediaBlockTriangles triangles, TimeSpan time)
        {
            if (triangles is not null)
            {
                var key = new CGameCtnMediaBlockTriangles.Key(triangles)
                {
                    Time = time,
                    Positions = triangles.Keys[0].Positions
                };

                triangles.Keys.Add(key);
            }
        }

        private void ProcessAnalogSteering(IEnumerable<ControlEntry> entries, TimeSpan eventsDuration, IList<CGameCtnMediaTrack> tracks)
        {
            // Defines the start of the first event
            var eventStartTime = new TimeSpan();

            if (!ShowAfterInteraction)
                eventStartTime = TimeSpan.FromMilliseconds(Math.Min(0, entries.Min(x => x.Time.TotalMilliseconds)));

            CreatePad(Side.Left, $"{(int)Theme}_PadLeft_2.png", $"{(int)Theme}_PadLeft_2_On.png", eventStartTime, eventsDuration, tracks, entries);
            CreatePad(Side.Right, $"{(int)Theme}_PadRight_2.png", $"{(int)Theme}_PadRight_2_On.png", eventStartTime, eventsDuration, tracks, entries);

            var trackPad = CreateMediaTrack("Pad");
            tracks.Add(trackPad);

            var padQuad = default(CGameCtnMediaBlockTriangles);

            var inverse = -1;
            var lastEntry = entries.Last();

            foreach (var entry in entries)
            {
                if (entry.Name == "_FakeDontInverseAxis" && entry.IsEnabled)
                {
                    inverse = 1;
                }

                if (entry.Equals(lastEntry))
                {
                    CompleteTheTriangle(padQuad, eventsDuration);
                }

                switch (entry.Name)
                {
                    case "Steer":
                    case "Steer (analog)":
                        {
                            var analog = (ControlEntryAnalog)entry;

                            CompleteTheTriangle(padQuad, entry.Time);

                            if (analog.Value > 0 || analog.Value < 0)
                            {
                                padQuad = CreatePadMoment(analog.Value * inverse, entry.Time);
                                trackPad.Blocks.Add(padQuad);
                            }
                            else
                            {
                                padQuad = null;
                            }

                            break;
                        }
                }
            }
        }

        private CGameCtnMediaBlockTriangles CreateKeyPadMoment(KeyboardKey keyboardKey, float value, TimeSpan time)
        {
            if (value == 0) return null;

            var color = PadColor;
            if (keyboardKey.EntryName == "Brake")
                color = PadBrakeColor;

            var padColorShine = (MathF.Min(1, color.X + 0.1f), MathF.Min(1, color.Y + 0.1f), MathF.Min(1, color.Z + 0.1f), color.W);

            var trianglePad = new CGameCtnMediaBlockTriangles2D()
            {
                Vertices = new Vec4[] { padColorShine, color, color, padColorShine },
                Triangles = new Int3[] { (0, 1, 2), (0, 2, 3) }
            };

            trianglePad.CreateChunk<CGameCtnMediaBlockTriangles.Chunk03029001>();

            var positions = new Vec3[]
            {
                (new Vec3(-0.2f, -0.2f, 0) + keyboardKey.Position) * Scale,
                (new Vec3(0.2f, -0.2f, 0) + keyboardKey.Position) * Scale,
                (new Vec3(0.2f, 0.2f, 0) + keyboardKey.Position) * Scale,
                (new Vec3(-0.2f, 0.2f, 0) + keyboardKey.Position) * Scale
            };

            TransformTriangles(ref positions);

            var pos1Start = positions[0];
            var pos2Start = positions[1];
            var pos2End = positions[2];
            var pos1End = positions[3];

            if (keyboardKey.EntryName == "Brake")
            {
                pos1End = positions[0];
                pos2End = positions[1];
                pos2Start = positions[2];
                pos1Start = positions[3];
            }

            var pos1Value = AdditionalMath.Lerp(pos1Start, pos1End, MathF.Abs(value));
            var pos2Value = AdditionalMath.Lerp(pos2Start, pos2End, MathF.Abs(value));

            var key = new CGameCtnMediaBlockTriangles.Key(trianglePad)
            {
                Time = time,
                Positions = new Vec3[]
                {
                    pos1Start,
                    pos1Value,
                    pos2Value,
                    pos2Start
                }
            };

            trianglePad.Keys.Add(key);

            return trianglePad;
        }

        private void CreatePad(Side side, string image, string imageOn, TimeSpan eventStartTime, TimeSpan eventsDuration,
            IList<CGameCtnMediaTrack> tracks, IEnumerable<ControlEntry> entries)
        {
            var trackBase = CreateMediaTrack($"Pad {side} Base");
            tracks.Add(trackBase);

            var sideMultiplier = 1;
            if (side == Side.Right)
                sideMultiplier = -1;

            var imageBase = CreateImageBlock(image, eventStartTime, PadOffset * (sideMultiplier, 1), (2, 2));

            var isPressed = false;
            var position = PadOffset * (sideMultiplier, 1);

            foreach (var entry in entries)
            {
                if ((side == Side.Left && entry.Name == "SteerLeft")
                  || side == Side.Right && entry.Name == "SteerRight")
                {
                    if (entry.IsEnabled)
                    {
                        if (!isPressed)
                        {
                            var time = entry.Time;

                            if (imageBase != null)
                            {
                                imageBase.Effect.Keys[1] = CreateSimiKey(time, position, (2, 2));
                            }

                            trackBase.Blocks.Add(imageBase);

                            imageBase = CreateImageBlock(imageOn, time, position, (2, 2));

                            isPressed = true;
                        }
                    }
                    else
                    {
                        var prevTime = imageBase.Effect.Keys[0].Time;
                        var time = entry.Time;

                        if (AdjustToFPS)
                        {
                            var blockLength = time - prevTime;

                            if (blockLength < TimeSpan.FromSeconds(1 / FPS))
                                time = prevTime + TimeSpan.FromSeconds(1 / FPS);
                        }

                        imageBase.Effect.Keys[1] = CreateSimiKey(time, position, (2, 2));
                        trackBase.Blocks.Add(imageBase);

                        imageBase = CreateImageBlock(image, time, position, (2, 2));

                        isPressed = false;
                    }
                }
            }

            imageBase.Effect.Keys[1] = CreateSimiKey(eventsDuration, position, (2, 2));
            trackBase.Blocks.Add(imageBase);
        }

        private void TransformTriangles(ref Vec3[] positions)
        {
            for (var i = 0; i < positions.Length; i++)
                positions[i] *= (AspectRatio.Y / AspectRatio.X, 1, 1);

            var avgPos = new Vec3();
            foreach (var pos in positions)
                avgPos += pos;
            avgPos = (avgPos.X / positions.Length, avgPos.Y / positions.Length, avgPos.Z / positions.Length);
            var newAvgPos = avgPos * Space;
            var offset = newAvgPos - avgPos;

            for (var i = 0; i < positions.Length; i++)
                positions[i] += offset + Position;
        }

        private CGameCtnMediaBlockTriangles CreatePadMoment(float value, TimeSpan time)
        {
            int sideMultiplier;
            if (value == 0) return null;
            else if (value > 0)
                sideMultiplier = -1;
            else
                sideMultiplier = 1;

            var padColorShine = (MathF.Min(1, PadColor.X + 0.1f), MathF.Min(1, PadColor.Y + 0.1f), MathF.Min(1, PadColor.Z + 0.1f), PadColor.W);

            var trianglePad = new CGameCtnMediaBlockTriangles2D()
            {
                Vertices = new Vec4[] { padColorShine, PadColor, PadColor, padColorShine },
                Triangles = new Int3[] { (0, 1, 2), (0, 2, 3) }
            };

            var chunk001 = trianglePad.CreateChunk<CGameCtnMediaBlockTriangles.Chunk03029001>();

            var positions = new Vec3[]
            {
                PadStartPosition * (sideMultiplier, 1, 1) * Scale,
                PadEndPosition * (sideMultiplier, 1, 1) * Scale,
                PadEndPosition * (1, -1, 1) * (sideMultiplier, 1, 1) * Scale,
                PadStartPosition * (1, -1, 1) * (sideMultiplier, 1, 1) * Scale,
            };

            TransformTriangles(ref positions);

            var pos1Start = positions[0];
            var pos1End = positions[1];
            var pos2End = positions[2];
            var pos2Start = positions[3];

            var pos1Value = AdditionalMath.Lerp(pos1Start, pos1End, MathF.Abs(value));
            var pos2Value = AdditionalMath.Lerp(pos2Start, pos2End, MathF.Abs(value));

            var key = new CGameCtnMediaBlockTriangles.Key(trianglePad)
            {
                Time = time,
                Positions = new Vec3[]
                {
                    pos1Start,
                    pos1Value,
                    pos2Value,
                    pos2Start
                }
            };

            trianglePad.Keys.Add(key);

            return trianglePad;
        }

        private CGameCtnMediaTrack CreateMediaTrack(string name)
        {
            Console.WriteLine($"Creating media track {name}...");

            var track = new CGameCtnMediaTrack
            {
                Name = name,
                IsKeepPlaying = false,
                Blocks = new List<CGameCtnMediaBlock>()
            };

            var chunk001 = track.CreateChunk<CGameCtnMediaTrack.Chunk03078001>();

            switch (GameVersion)
            {
                case Game.TMUF:
                    track.CreateChunk<CGameCtnMediaTrack.Chunk03078004>();
                    chunk001.U02 = 2;
                    break;
                case Game.ManiaPlanet:
                    track.CreateChunk<CGameCtnMediaTrack.Chunk03078005>();
                    chunk001.U02 = -1;
                    break;
            }

            return track;
        }

        private CGameCtnMediaBlockImage CreateImageBlock(string image, TimeSpan time, Vec2 position, Vec2 scale, float depth = 0.5f)
        {
            var blockImage = new CGameCtnMediaBlockImage();

            switch (GameVersion)
            {
                case Game.TMUF:
                    blockImage.Image = new FileRef(2, null, Path.Combine(@"MediaTracker\Images", Path.GetFileName(image)),
                    new Uri("https://bigbang1112.eu/projects/clipinput/" + image));
                    break;
                case Game.ManiaPlanet:
                    blockImage.Image = new FileRef(3, FileRef.DefaultChecksum, Path.Combine(@"Media\Images\Inputs", Path.GetFileName(image)),
                    new Uri("https://bigbang1112.eu/projects/clipinput/" + image));
                    break;
            }

            blockImage.CreateChunk<CGameCtnMediaBlockImage.Chunk030A5000>();

            var effect = new CControlEffectSimi
            {
                Keys = new List<CControlEffectSimi.Key>
                {
                    CreateSimiKey(time, position, scale, depth), null
                }
            };

            effect.CreateChunk<CControlEffectSimi.Chunk07010005>();
            effect.Centered = true;
            effect.IsInterpolated = true;

            blockImage.Effect = effect;

            return blockImage;
        }

        private CGameCtnMediaBlockImage CreateImageBlock(string image, TimeSpan time, Vec2 position)
        {
            return CreateImageBlock(image, time, position, (1, 1));
        }

        private CControlEffectSimi.Key CreateSimiKey(TimeSpan time, Vec2 position, Vec2 scale, float depth = 0.5f)
        {
            return new CControlEffectSimi.Key
            {
                Time = time,
                ScaleX = scale.X * Scale.X / (AspectRatio.X / AspectRatio.Y),
                ScaleY = scale.Y * Scale.Y,
                Opacity = 1,
                Depth = depth,
                X = position.X * Space.X * Scale.X + Position.X,
                Y = position.Y * Space.Y * Scale.Y + Position.Y
            };
        }

        private CControlEffectSimi.Key CreateSimiKey(TimeSpan time, Vec2 position)
        {
            return CreateSimiKey(time, position, (1, 1));
        }
    }
}
