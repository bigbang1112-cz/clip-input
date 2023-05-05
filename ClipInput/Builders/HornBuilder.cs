using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class HornBuilder : BlockBuilder
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;

    public HornBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
    }

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? endTime)
    {
        var isOneTickHorn = true;

        var prevTime = default(TimeInt32?);

        foreach (var input in inputs.OfType<Horn>())
        {
            if (prevTime.HasValue && input.Time - prevTime.Value > new TimeInt32(10))
            {
                isOneTickHorn = false;
                break;
            }

            prevTime = input.Time;
        }

        BlockBuilder builder = isOneTickHorn ? new HornTapBuilder(inputs, config) : new HornPressBuilder(inputs, config);

        foreach (var block in builder.BuildBlocks(endTime))
        {
            yield return block;
        }
    }
}
