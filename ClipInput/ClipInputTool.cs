using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Control;
using GBX.NET.IO;

namespace ClipInput
{
    public class ClipInputTool : GameBoxIO<CGameCtnMediaClip>
    {
        public Game GameVersion { get; private set; }

        public Vec2 AspectRatio { get; set; } = (16, 9);
        public Vec2 Scale { get; set; } = (1, 1);
        public Vec2 Space { get; set; } = (1, 1);
        public Vec2 Position { get; set; } = (0, 0);
        public bool ShowAfterInteraction { get; set; }
        public bool CutoffOutside { get; set; } // WIP
        public Vec2 PadOffset { get; set; }
        public Vec4 PadColor { get; set; } = (1, 1, 1, 0);
        public Vec4 PadBackgroundColor { get; set; } = (0, 0, 0, 1);
        public Vec3 PadStartPosition { get; set; }
        public Vec3 PadEndPosition { get; set; }
        public Theme Theme { get; set; }
        public KeyboardKey[] Keys { get; set; }

        public ClipInputTool(GameBox gbx) : base(gbx)
        {
            AspectRatio = (16, 9);
            Scale = (0.5f, 0.5f);
            Space = (1.05f, 1.05f);
            Position = (0, 0.5f);
            ShowAfterInteraction = false;
            PadOffset = (0.385f, 0);
            PadColor = (0.11f, 0.44f, 0.69f, 1);
            PadBackgroundColor = (0.3f, 0.3f, 0.3f, 0.3f);
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
        public override GameBox<CGameCtnMediaClip> Process()
        {
            if (InputGBX.TryNode(out CGameCtnReplayRecord replay))
            {
                if (replay.ControlEntries is null)
                {
                    if (replay.Ghosts is not null && replay.Ghosts.Length > 0)
                    {
                        return ProcessGhost(replay.Ghosts.FirstOrDefault());
                    }

                    var ghosts = ExtractGhosts(replay.Clip);
                    return ProcessGhost(ghosts.FirstOrDefault());
                }

                return ProcessControlEntries(replay.ControlEntries, TimeSpan.FromMilliseconds(replay.EventsDuration));
            }

            if (InputGBX.TryNode(out CGameCtnGhost ghost))
            {
                return ProcessGhost(ghost);
            }

            if (InputGBX.TryNode(out CGameCtnMediaClip clip))
            {
                var ghosts = ExtractGhosts(clip);

                return ProcessGhost(ghosts.FirstOrDefault());
            }

            throw new Exception();
        }

        private GameBox<CGameCtnMediaClip> ProcessGhost(CGameCtnGhost ghost)
        {
            if (ghost == null)
                throw new ArgumentNullException(nameof(ghost));

            if (ghost.TryGetChunk(out CGameCtnGhost.Chunk03092025 chunk025))
            {
                if (ghost.Validate_ExeVersion.StartsWith("TrackmaniaTurbo"))
                {
                    throw new TMTurboNotSupportedException();
                }
                else
                {
                    GameVersion = Game.ManiaPlanet;
                }
            }
            else
            {
                GameVersion = Game.TMUF;
            }

            return ProcessControlEntries(ghost.ControlEntries, TimeSpan.FromMilliseconds(ghost.EventsDuration));
        }

        private GameBox<CGameCtnMediaClip> ProcessControlEntries(IEnumerable<ControlEntry> entries, TimeSpan eventsDuration)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries),
                "No inputs are available in this GBX.");

            var clip = new CGameCtnMediaClip()
            {
                LocalPlayerClipEntIndex = -1
            };

            switch (GameVersion)
            {
                case Game.TMUF:
                    clip.CreateChunk<CGameCtnMediaClip.Chunk03079004>();
                    clip.CreateChunk<CGameCtnMediaClip.Chunk03079005>();
                    clip.CreateChunk<CGameCtnMediaClip.Chunk03079007>();
                    break;
                case Game.ManiaPlanet:
                    clip.CreateChunk<CGameCtnMediaClip.Chunk0307900D>();
                    break;
            }

            var tracks = new List<CGameCtnMediaTrack>();

            var hasDigitalInput = false;
            var hasAnalogInput = false;

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
                        hasDigitalInput = true;
                        break;
                    case "Steer (analog)":
                    case "Steer":
                        hasAnalogInput = true;
                        break;
                }
            }
            
            if (hasDigitalInput)
                ProcessDigitalInput(entries, eventsDuration, tracks, onlyAcceleration: hasAnalogInput);

            if (hasAnalogInput)
                ProcessAnalogInput(entries, eventsDuration, tracks);

            clip.Tracks = tracks;

            return new GameBox<CGameCtnMediaClip>(clip);
        }

        private void ProcessDigitalInput(IEnumerable<ControlEntry> entries, TimeSpan eventsDuration, IList<CGameCtnMediaTrack> tracks, bool onlyAcceleration)
        {
            var trackDictionary = new Dictionary<KeyboardKey, CGameCtnMediaTrack>();
            var currentImageDictionary = new Dictionary<KeyboardKey, CGameCtnMediaBlockImage>();
            var pressedKeyDictionary = new Dictionary<KeyboardKey, bool>();

            foreach (var key in Keys)
            {
                if (!onlyAcceleration || !key.IsSteerInput)
                {
                    var track = CreateMediaTrack(key.TrackName);
                    track.Blocks = new List<CGameCtnMediaBlock>();
                    trackDictionary[key] = track;

                    currentImageDictionary[key] = null;
                    pressedKeyDictionary[key] = false;

                    if (!ShowAfterInteraction)
                    {
                        // Defines the start of the first event
                        TimeSpan eventStartTime;

                        if (CutoffOutside)
                            eventStartTime = new TimeSpan();
                        else
                            eventStartTime = TimeSpan.FromMilliseconds(Math.Min(0, entries.Min(x => x.Time.TotalMilliseconds)));

                        var blockImage = CreateImageBlock(string.Format(key.ImageOff, (int)Theme), eventStartTime, key.Position);

                        trackDictionary[key].Blocks.Add(blockImage);
                        currentImageDictionary[key] = blockImage;
                    }
                }
            }

            foreach (var entry in entries)
            {
                foreach (var key in Keys)
                {
                    if (!onlyAcceleration || !key.IsSteerInput)
                    {
                        if (key.EntryNames.Contains(entry.Name))
                        {
                            if (entry.IsEnabled)
                            {
                                if (!pressedKeyDictionary[key])
                                {
                                    if (currentImageDictionary[key] != null)
                                    {
                                        currentImageDictionary[key].Effect.Keys[1] = CreateSimiKey(entry.Time, key.Position);
                                    }

                                    var blockImage = CreateImageBlock(string.Format(key.ImageOn, (int)Theme), entry.Time, key.Position);

                                    trackDictionary[key].Blocks.Add(blockImage);
                                    currentImageDictionary[key] = blockImage;
                                    pressedKeyDictionary[key] = true;
                                }
                            }
                            else
                            {
                                currentImageDictionary[key].Effect.Keys[1] = CreateSimiKey(entry.Time, key.Position);

                                var blockImage = CreateImageBlock(string.Format(key.ImageOff, (int)Theme), entry.Time, key.Position);

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

        private void ProcessAnalogInput(IEnumerable<ControlEntry> entries, TimeSpan eventsDuration, IList<CGameCtnMediaTrack> tracks)
        {
            // Defines the start of the first event
            var eventStartTime = new TimeSpan();

            if (!ShowAfterInteraction)
                eventStartTime = TimeSpan.FromMilliseconds(Math.Min(0, entries.Min(x => x.Time.TotalMilliseconds)));

            var leftPad = CreatePad(Side.Left, $"{(int)Theme}_PadLeft.png", eventStartTime, eventsDuration, tracks);
            var rightPad = CreatePad(Side.Right, $"{(int)Theme}_PadRight.png", eventStartTime, eventsDuration, tracks);

            var trackPad = CreateMediaTrack("Pad");
            trackPad.Blocks = new List<CGameCtnMediaBlock>();
            tracks.Add(trackPad);

            var pads = new CGameCtnMediaBlockTriangles[] { leftPad, rightPad };

            var padQuad = default(CGameCtnMediaBlockTriangles);

            var inverse = -1;

            foreach (var entry in entries)
            {
                if (entry.Name == "_FakeDontInverseAxis" && entry.IsEnabled)
                    inverse = 1;

                if (entry.Name == "Steer" || entry.Name == "Steer (analog)")
                {
                    var analog = (ControlEntryAnalog)entry;

                    if (padQuad != null)
                    {
                        var key = new CGameCtnMediaBlockTriangles.Key(padQuad)
                        {
                            Time = (float)entry.Time.TotalSeconds,
                            Positions = padQuad.Keys[0].Positions
                        };

                        padQuad.Keys.Add(key);
                    }

                    if (analog.Value > 0 || analog.Value < 0)
                    {
                        padQuad = CreatePad(analog.Value * inverse, entry.Time);
                        trackPad.Blocks.Add(padQuad);
                    }
                    else
                    {
                        padQuad = null;
                    }
                }
            }
        }

        private CGameCtnMediaBlockTriangles CreatePad(Side side, string image, TimeSpan eventStartTime, TimeSpan eventsDuration, IList<CGameCtnMediaTrack> tracks)
        {
            var trackBase = CreateMediaTrack($"Pad {side} Base");
            trackBase.Blocks = new List<CGameCtnMediaBlock>();
            tracks.Add(trackBase);

            var sideMultiplier = 1;
            if (side == Side.Right)
                sideMultiplier = -1;

            var imageBase = CreateImageBlock(image, eventStartTime, PadOffset * (sideMultiplier, 1), (2, 2));
            imageBase.Effect.Keys[1] = CreateSimiKey(eventsDuration, PadOffset * (sideMultiplier, 1), (2, 2));
            trackBase.Blocks.Add(imageBase);

            var trackBackground = CreateMediaTrack($"Pad {side} Bg");
            trackBackground.Blocks = new List<CGameCtnMediaBlock>();
            tracks.Add(trackBackground);

            var triangleBackground = CreatePadBackground(side, eventStartTime, eventsDuration);
            trackBackground.Blocks.Add(triangleBackground);

            return triangleBackground;
        }

        private CGameCtnMediaBlockTriangles CreatePadBackground(Side side, TimeSpan eventStartTime, TimeSpan eventsDuration)
        {
            var sideMultiplier = 1;
            if (side == Side.Right)
                sideMultiplier = -1;

            var trianglePad = new CGameCtnMediaBlockTriangles2D()
            {
                Vertices = new Vec4[] { PadBackgroundColor, PadBackgroundColor, PadBackgroundColor, PadBackgroundColor },
            };
            trianglePad.CreateChunk<CGameCtnMediaBlockTriangles.Chunk03029001>();

            trianglePad.Triangles = new Int3[] { (0, 1, 2), (0, 2, 3) };

            var positions = new Vec3[]
            {
                PadStartPosition * (sideMultiplier, 1, 1) * Scale,
                PadEndPosition * (sideMultiplier, 1, 1) * Scale,
                PadEndPosition * (sideMultiplier, 1, 1) * Scale,
                PadStartPosition * (1, -1, 1) * (sideMultiplier, 1, 1) * Scale
            };

            TransformTriangles(ref positions);

            var key1 = new CGameCtnMediaBlockTriangles.Key(trianglePad)
            {
                Time = (float)eventStartTime.TotalSeconds,
                Positions = positions
            };
            trianglePad.Keys.Add(key1);

            var key2 = new CGameCtnMediaBlockTriangles.Key(trianglePad)
            {
                Time = (float)eventsDuration.TotalSeconds,
                Positions = positions
            };
            trianglePad.Keys.Add(key2);

            return trianglePad;
        }

        private void TransformTriangles(ref Vec3[] positions)
        {
            var avgPos = new Vec3();
            foreach (var pos in positions)
                avgPos += pos;
            avgPos = (avgPos.X / positions.Length, avgPos.Y / positions.Length, avgPos.Z / positions.Length);
            var newAvgPos = avgPos * Space;
            var offset = newAvgPos - avgPos;

            for (var i = 0; i < positions.Length; i++)
                positions[i] += offset + Position;
        }

        private CGameCtnMediaBlockTriangles CreatePad(float value, TimeSpan time)
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

            trianglePad.CreateChunk<CGameCtnMediaBlockTriangles.Chunk03029001>();

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
                Time = (float)time.TotalSeconds,
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
            var track = new CGameCtnMediaTrack
            {
                Name = name,
                IsKeepPlaying = false
            };

            var chunk001 = track.CreateChunk<CGameCtnMediaTrack.Chunk03078001>();
            chunk001.U01 = 10;

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

        private CGameCtnMediaBlockImage CreateImageBlock(string imageUrl, TimeSpan time, Vec2 position, Vec2 scale)
        {
            var blockImage = new CGameCtnMediaBlockImage();

            switch (GameVersion)
            {
                case Game.TMUF:
                    blockImage.Image = new FileRef(2, null, Path.Combine(@"MediaTracker\Images\Inputs", Path.GetFileName(imageUrl)),
                    new Uri("https://bigbang1112.eu/projects/clipinput/" + imageUrl));
                    break;
                case Game.ManiaPlanet:
                    blockImage.Image = new FileRef(3, FileRef.DefaultChecksum, Path.Combine(@"Media\Images\Inputs", Path.GetFileName(imageUrl)),
                    new Uri("https://bigbang1112.eu/projects/clipinput/" + imageUrl));
                    break;
            }

            blockImage.CreateChunk<CGameCtnMediaBlockImage.Chunk030A5000>();

            var effect = new CControlEffectSimi
            {
                Keys = new List<CControlEffectSimi.Key>
                {
                    CreateSimiKey(time, position, scale), null
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

        private CControlEffectSimi.Key CreateSimiKey(TimeSpan time, Vec2 position, Vec2 scale)
        {
            return new CControlEffectSimi.Key
            {
                Time = (float)time.TotalSeconds,
                ScaleX = scale.X * Scale.X / (AspectRatio.X / AspectRatio.Y),
                ScaleY = scale.Y * Scale.Y,
                Opacity = 1,
                Depth = 0.5f,
                X = position.X * Space.X * Scale.X + Position.X,
                Y = position.Y * Space.Y * Scale.Y + Position.Y,
                Unknown = new float[] { 0, 0, 0 }
            };
        }

        private CControlEffectSimi.Key CreateSimiKey(TimeSpan time, Vec2 position)
        {
            return CreateSimiKey(time, position, (1, 1));
        }
    }
}
