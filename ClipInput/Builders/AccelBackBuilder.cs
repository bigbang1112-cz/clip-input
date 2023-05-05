using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class AccelBackBuilder : GasBackBuilder<Accelerate, AccelerateReal>
{
    private readonly IDesign design;

    public AccelBackBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        design = config.Design;
    }

    protected override CGameCtnMediaBlock? InitiateAnalog(TimeSingle time)
    {
        return design.InitiateAnalogAccelBack(time);
    }
}
