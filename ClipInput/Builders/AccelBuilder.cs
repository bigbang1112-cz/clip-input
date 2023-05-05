using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class AccelBuilder : GasBuilder<Accelerate, AccelerateReal>
{
    private readonly IDesign design;

    public AccelBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        design = config.Design;
    }

    protected override void ApplyAnalog(CGameCtnMediaBlock block, TimeInt32 time, float value)
    {
        design.ApplyAnalogAccel(block, time, value);
    }

    protected override CGameCtnMediaBlock InitiateAnalog(TimeSingle time, float value)
    {
        return design.InitiateAnalogAccel(time, value);
    }

    protected override CGameCtnMediaBlock InitiateDigital(TimeSingle time, bool pressed)
    {
        return design.InitiateDigitalAccel(time, pressed);
    }
}
