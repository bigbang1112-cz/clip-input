using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class BrakeBackBuilder : GasBackBuilder<Brake, BrakeReal>
{
    private readonly IDesign design;

    public BrakeBackBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        design = config.Design;
    }

    protected override CGameCtnMediaBlock? InitiateAnalog(TimeSingle time)
    {
        return design.InitiateAnalogBrakeBack(time);
    }
}
