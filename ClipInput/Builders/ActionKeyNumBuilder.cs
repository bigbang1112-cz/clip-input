using GBX.NET;
using GBX.NET.Engines.Control;
using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class ActionKeyNumBuilder : BlockBuilder
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;
    private readonly int index;
    private readonly bool newActionKeyLayout;

    public ActionKeyNumBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config, int index, bool newActionKeyLayout) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
        this.index = index;
        this.newActionKeyLayout = newActionKeyLayout;
    }

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? blockEndTime, TimeInt32? inputEndTime)
    {
        var earliestInputTime = GetFirstInputTime();

        var block = config.Design.InitiateActionSlotNum(earliestInputTime, blockEndTime ?? inputs.Last().Time, index);

        if (block is not null)
        {
            yield return block;
        }
    }
}
