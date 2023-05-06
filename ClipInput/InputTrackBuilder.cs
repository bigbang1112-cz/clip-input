using ClipInput.Builders;
using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using System.Diagnostics.CodeAnalysis;
using TmEssentials;

namespace ClipInput;

internal class InputTrackBuilder
{
    private readonly ClipInputConfig config;
    private readonly IList<CGameCtnMediaTrack> tracks;
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly TimeInt32? blockEndTime;
    private readonly TimeInt32? inputEndTime;

    public InputTrackBuilder(ClipInputConfig config, IList<CGameCtnMediaTrack> tracks, IReadOnlyCollection<IInput> inputs, TimeInt32? blockEndTime, TimeInt32? inputEndTime)
    {
        this.config = config;
        this.tracks = tracks;
        this.inputs = inputs;
        this.blockEndTime = blockEndTime;
        this.inputEndTime = inputEndTime;
    }

    public void Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string trackName) where T : BlockBuilder
    {
        var builder = (T)Activator.CreateInstance(typeof(T), inputs, config)!;

        var blocks = builder.BuildBlocks(blockEndTime, inputEndTime).ToList();

        if (blocks.Count > 0)
        {
            tracks.Add(CreateTrack(trackName, blocks));
        }
    }

    private IEnumerable<CGameCtnMediaTrack> BuildActionKeyTracks(CGameCtnGhost.PlayerInputData.EVersion? inputVersion)
    {
        var hasActionKeys = false;
        var hasOldActionKeys = false;

        foreach (var actionKey in inputs.OfType<ActionSlot>())
        {
            hasActionKeys = true;

            if (actionKey.Slot is 1 or 3 or 5 or 7 or 9)
            {
                hasOldActionKeys = true;
                break;
            }
        }

        if (!hasActionKeys)
        {
            yield break;
        }

        var isShootMania = false;
        var count = hasOldActionKeys ? 10 : 5;

        if (inputVersion <= CGameCtnGhost.PlayerInputData.EVersion._2017_09_12)
        {
            isShootMania = true;
            hasOldActionKeys = true;
            count = 4;
        }

        for (var i = 0; i < count; i++)
        {
            var num = (i + 1) % 10;

            var blocksNum = new ActionKeyNumBuilder(inputs, config, i, newActionKeyLayout: !hasOldActionKeys)
                .BuildBlocks(blockEndTime, inputEndTime)
                .ToList();

            yield return CreateTrack(string.Format(config.Dictionary.MediaTrackerTrackActionKeyOverlay, num), blocksNum);

            var blocks = new ActionKeyBuilder(inputs, config, i, isShootMania, newActionKeyLayout: !hasOldActionKeys)
                .BuildBlocks(blockEndTime, inputEndTime)
                .ToList();

            yield return CreateTrack(string.Format(config.Dictionary.MediaTrackerTrackActionKey, num), blocks);
        }
    }

    public void AddActionKeys(CGameCtnGhost.PlayerInputData.EVersion? inputVersion)
    {
        if (!config.EnableActionKeys)
        {
            return;
        }

        foreach (var track in BuildActionKeyTracks(inputVersion))
        {
            tracks.Add(track);
        }
    }

    private static CGameCtnMediaTrack CreateTrack(string name, IList<CGameCtnMediaBlock> blocks)
    {
        return CGameCtnMediaTrack.Create()
            .WithName(name)
            .WithBlocks(blocks)
            .ForTMUF()
            .Build();
    }
}
