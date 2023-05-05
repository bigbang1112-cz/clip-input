using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class WalkBackwardBuilder : WalkBuilder
{
    private readonly IDesign design;

    public WalkBackwardBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config, EWalk.Backward)
    {
        design = config.Design;
    }

    protected override CGameCtnMediaBlock Initiate(TimeSingle time, bool pressed)
    {
        return design.InitiateDigitalBrake(time, pressed);
    }
}
