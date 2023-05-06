using ClipInput.Builders;
using ClipInput.Skins;
using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using GbxToolAPI;
using TmEssentials;

namespace ClipInput;

[ToolName("Clip Input 2")]
[ToolDescription("Transfers input data available inside a replay or ghost into a MediaTracker clip.")]
[ToolGitHub("bigbang1112-cz/clip-input")]
[ToolAssets("ClipInput2")]
[ToolAssetsIgnoreIngame("Skins")]
public class ClipInputTool : ITool, IHasOutput<NodeFile<CGameCtnMediaClip>>, IHasAssets, IConfigurable<ClipInputConfig>
{
    private readonly CGameCtnReplayRecord? replay;
    private readonly IReadOnlyCollection<Ghost> ghosts;
    private readonly TimeInt32 endTime;

    public ClipInputConfig Config { get; set; } = new();

    public ClipInputTool(CGameCtnReplayRecord replay)
    {
        this.replay = replay;
        (ghosts, endTime) = GetGhostsAndEndTime(replay.GetGhosts(), replay);
    }

    public ClipInputTool(CGameCtnGhost ghost) : this(Enumerable.Repeat(ghost, 1))
    {

    }

    public ClipInputTool(IEnumerable<CGameCtnGhost> ghosts)
    {
        (this.ghosts, endTime) = GetGhostsAndEndTime(ghosts);
    }

    public ClipInputTool(CGameCtnMediaClip clip) : this(clip.Tracks.SelectMany(x => x.Blocks).OfType<CGameCtnMediaBlockGhost>().Select(x => x.GhostModel))
    {
        
    }

    public ClipInputTool(TextFile txtFile)
    {
        var inputs = DonadigoInputFile.Parse(txtFile.Text)
            .OrderBy(x => x.Time)
            .ToList();
        
        ghosts = new List<Ghost> { new(inputs) };
        endTime = inputs.LastOrDefault()?.Time ?? TimeInt32.Zero;
    }

    public async ValueTask LoadAssetsAsync()
    {
        var usedSkinId = string.IsNullOrWhiteSpace(Config.SkinId) ? "Default" : Config.SkinId;

        Config.Skin = Config.DesignId.ToLowerInvariant() switch
        {
            "basic" => await AssetsManager<ClipInputTool>.GetFromYmlAsync<BasicDesignSkin>(Path.Combine("Skins", "Basic", usedSkinId + ".yml")),
            "compact" => await AssetsManager<ClipInputTool>.GetFromYmlAsync<BasicDesignSkin>(Path.Combine("Skins", "Basic", usedSkinId + ".yml")),
            "image" => await AssetsManager<ClipInputTool>.GetFromYmlAsync<ImageDesignSkin>(Path.Combine("Skins", "Image", usedSkinId + ".yml")),
            "text" => null,
            _ => throw new NotImplementedException($"{Config.DesignId} is not a valid design ID to be used with skins.")
        };
    }

    public static string RemapAssetRoute(string route, bool isManiaPlanet)
    {
        if (route.StartsWith("Images"))
        {
            return Path.Combine("Media" + (isManiaPlanet ? "" : "Tracker"), "Images", "ClipInput2", route[("Images".Length + 1)..]);
        }

        return "";
    }

    private static (IReadOnlyCollection<Ghost>, TimeInt32 endTime) GetGhostsAndEndTime(IEnumerable<CGameCtnGhost> ghosts, CGameCtnReplayRecord? replay = null)
    {
        var ghostWraps = new List<Ghost>();

        var longestTime = default(TimeInt32?);

        foreach (var ghost in ghosts)
        {
            var time = GetBlockEndTime(ghost);

            if (longestTime is null || time > longestTime)
            {
                longestTime = time;
            }

            if (ghost.PlayerInputs?.Length > 1)
            {
                // note
            }

            var inputs = ghost.PlayerInputs?.FirstOrDefault()?.Inputs ?? ghost.Inputs ?? replay?.Inputs;

            if (inputs is null || inputs.Count == 0)
            {
                continue;
            }

            longestTime ??= inputs.Last().Time + TimeInt32.FromSeconds(1);

            ghostWraps.Add(new(inputs, ghost));
        }

        if (longestTime is null)
        {
            throw new Exception("No ghosts found.");
        }

        return (ghostWraps, longestTime.Value);
    }

    public NodeFile<CGameCtnMediaClip> Produce()
    {
        var tracks = new List<CGameCtnMediaTrack>();

        Config.UpdateDesign();
        
        var forManiaPlanet = IsManiaPlanet();

        if (forManiaPlanet)
        {
            Config.Design.SetupForManiaPlanet();
        }

        foreach (var ghost in ghosts)
        {
            GenerateGhostInputs(tracks, ghost);
        }

        var clip = CGameCtnMediaClip.Create()
            .WithTracks(tracks)
            .ForTMUF()
            .Build();

        var firstGhost = ghosts.FirstOrDefault()?.Object;

        var mapName = replay?.Challenge?.MapName ?? replay?.MapInfo?.Id ?? firstGhost?.Validate_ChallengeUid ?? "unknownmap";
        var time = firstGhost?.RaceTime.ToTmString(useApostrophe: true) ?? "unfinished";
        var author = firstGhost?.GhostNickname ?? firstGhost?.GhostLogin ?? "unnamed";

        var pureFileName = $"ClipInput2_{TextFormatter.Deformat(mapName)}_{time}_{TextFormatter.Deformat(author)}.Clip.Gbx";
        var validFileName = string.Join("_", pureFileName.Split(Path.GetInvalidFileNameChars()));

        var dir = forManiaPlanet ? "Replays/Clips/ClipInput2" : "Tracks/ClipInput2";

        return new(clip, $"{dir}/{validFileName}", forManiaPlanet);
    }

    private void GenerateGhostInputs(IList<CGameCtnMediaTrack> tracks, Ghost ghost)
    {
        var inputs = replay?.Inputs ?? ghost.Inputs;
        var inputEndTime = replay?.EventsDuration ?? ghost.Object?.EventsDuration;

        var fakeIsRaceRunning = inputs.OfType<FakeIsRaceRunning>().FirstOrDefault();

        if (fakeIsRaceRunning.Time == new TimeInt32(ushort.MaxValue))
        {
            foreach (var input in inputs)
            {
                input.GetType().GetProperty(nameof(IInput.Time))!.SetValue(input, input.Time - fakeIsRaceRunning.Time);
            }
        }

        var inputTrackBuilder = new InputTrackBuilder(Config, tracks, inputs, endTime, inputEndTime > TimeInt32.Zero ? inputEndTime.Value : null);

        for (var i = 0; i < 2; i++)
        {
            var left = Convert.ToBoolean(i); // according to mediatracker 2d coord system

            Config.Design.SetSteer(left);

            if (Config.EnableAnalogSteerValue)
            {
                var steerValueTrackName = string.Format(Config.Dictionary.MediaTrackerTrackSteerValue, left ? Config.Dictionary.Left : Config.Dictionary.Right);

                inputTrackBuilder.Add<SteerValueBuilder>(steerValueTrackName);
            }

            var steerTrackName = string.Format(Config.Dictionary.MediaTrackerTrackSteer, left ? Config.Dictionary.Left : Config.Dictionary.Right);

            inputTrackBuilder.Add<SteerBuilder>(steerTrackName);

            var steerBackTrackName = string.Format(Config.Dictionary.MediaTrackerTrackSteerBack, left ? Config.Dictionary.Left : Config.Dictionary.Right);

            inputTrackBuilder.Add<SteerBackBuilder>(steerBackTrackName);

            var strafeTrackName = string.Format(Config.Dictionary.MediaTrackerTrackStrafe, left ? Config.Dictionary.Left : Config.Dictionary.Right);

            inputTrackBuilder.Add<StrafeBuilder>(strafeTrackName);
        }

        Config.Design.SetSteer(null);

        if (Config.EnableAnalogAccelValue)
        {
            inputTrackBuilder.Add<AccelValueBuilder>(Config.Dictionary.MediaTrackerTrackAccelValue);
        }

        inputTrackBuilder.Add<AccelBuilder>(Config.Dictionary.MediaTrackerTrackAccel);
        inputTrackBuilder.Add<AccelBackBuilder>(Config.Dictionary.MediaTrackerTrackAccelBack);

        if (Config.EnableAnalogBrakeValue)
        {
            inputTrackBuilder.Add<BrakeValueBuilder>(Config.Dictionary.MediaTrackerTrackBrakeValue);
        }

        inputTrackBuilder.Add<BrakeBuilder>(Config.Dictionary.MediaTrackerTrackBrake);
        inputTrackBuilder.Add<BrakeBackBuilder>(Config.Dictionary.MediaTrackerTrackBrakeBack);

        inputTrackBuilder.Add<WalkForwardBuilder>(Config.Dictionary.MediaTrackerTrackWalkForward);
        inputTrackBuilder.Add<WalkBackwardBuilder>(Config.Dictionary.MediaTrackerTrackWalkBackward);

        inputTrackBuilder.Add<MouseBuilder>(Config.Dictionary.MediaTrackerTrackMouse);

        inputTrackBuilder.Add<JumpBuilder>(Config.Dictionary.MediaTrackerTrackJump);
        inputTrackBuilder.Add<HornBuilder>(Config.Dictionary.MediaTrackerTrackHorn);
        inputTrackBuilder.Add<RespawnBuilder>(Config.Dictionary.MediaTrackerTrackRespawn);
        inputTrackBuilder.Add<SecondaryRespawnBuilder>(Config.Dictionary.MediaTrackerTrackSecondaryRespawn);

        inputTrackBuilder.AddActionKeys(ghost.Object?.PlayerInputs?.FirstOrDefault()?.Version);

        if (fakeIsRaceRunning.Time == new TimeInt32(ushort.MaxValue))
        {
            foreach (var input in inputs)
            {
                // "Fix" for the mutation
                input.GetType().GetProperty(nameof(IInput.Time))!.SetValue(input, input.Time + fakeIsRaceRunning.Time);
            }
        }
    }

    private bool IsManiaPlanet()
    {
        var forManiaPlanet = ghosts.Select(x => x.Object)
            .OfType<CGameCtnGhost>()
            .All(GameVersion.IsManiaPlanet);

        if (!forManiaPlanet)
        {
            var someAreManiaPlanet = ghosts.Select(x => x.Object)
                .OfType<CGameCtnGhost>()
                .Any(GameVersion.IsManiaPlanet);

            if (someAreManiaPlanet)
            {
                throw new NotSupportedException("Some ghosts are from ManiaPlanet, but not all. This is not supported.");
            }

            if (!ghosts.Select(x => x.Object).OfType<CGameCtnGhost>().Any())
            {
                forManiaPlanet = Config.PreferManiaPlanet;
            }
        }

        return forManiaPlanet;
    }

    private static TimeInt32? GetBlockEndTime(CGameCtnGhost ghost)
    {
        var smEndTime = ghost.RecordData?.End;

        if (smEndTime.HasValue)
        {
            return smEndTime;
        }

        var tmEndTime = ghost.SampleData?.Samples.LastOrDefault()?.Time;

        if (tmEndTime.HasValue)
        {
            return tmEndTime.Value + ghost.SampleData?.SamplePeriod;
        }
        
        return ghost.RaceTime; // TMO samples should be fixed instead
    }
}
