using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class WalkForwardBuilder : WalkBuilder
{
    private readonly IDesign design;

    public WalkForwardBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config, EWalk.Forward)
    {
        design = config.Design;
    }

    protected override CGameCtnMediaBlock Initiate(TimeSingle time, bool pressed)
    {
        return design.InitiateDigitalAccel(time, pressed);
    }
}
