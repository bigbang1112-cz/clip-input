using GBX.NET;
using GBX.NET.Inputs;

namespace ClipInput.Builders;

class BrakeValueBuilder : GasValueBuilder<Brake, BrakeReal>
{
    private readonly ClipInputConfig config;

    protected override Vec2 AnalogPos => config.AnalogBrakePos;
    protected override Vec2 AnalogValueOffset => config.AnalogBrakeValueOffset;
    protected override Vec2 AnalogValueScale => config.AnalogBrakeValueScale;
    protected override Vec3 AnalogValueColor => config.AnalogBrakeValueColor;

    public BrakeValueBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.config = config;
    }
}
