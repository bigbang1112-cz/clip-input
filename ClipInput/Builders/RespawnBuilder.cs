using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class RespawnBuilder : BlockBuilder
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;

    public RespawnBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
    }

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? endTime)
    {
        var isOneTickHorn = inputs.OfType<RespawnTM2020>().Any();

        BlockBuilder builder = isOneTickHorn ? new RespawnTapBuilder(inputs, config) : new RespawnPressBuilder(inputs, config);

        foreach (var block in builder.BuildBlocks(endTime))
        {
            yield return block;
        }
    }
}
