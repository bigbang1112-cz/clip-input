using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class BrakeBuilder : GasBuilder<Brake, BrakeReal>
{
    private readonly IDesign design;

    public BrakeBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        design = config.Design;
    }

    protected override void ApplyAnalog(CGameCtnMediaBlock block, TimeInt32 time, float value)
    {
        design.ApplyAnalogBrake(block, time, value);
    }

    protected override CGameCtnMediaBlock InitiateAnalog(TimeSingle time, float value)
    {
        return design.InitiateAnalogBrake(time, value);
    }

    protected override CGameCtnMediaBlock InitiateDigital(TimeSingle time, bool pressed)
    {
        return design.InitiateDigitalBrake(time, pressed);
    }
}
