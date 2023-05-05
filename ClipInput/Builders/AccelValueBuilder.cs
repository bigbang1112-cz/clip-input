using GBX.NET;
using GBX.NET.Inputs;

namespace ClipInput.Builders;

class AccelValueBuilder : GasValueBuilder<Accelerate, AccelerateReal>
{
    private readonly ClipInputConfig config;

    protected override Vec2 AnalogPos => config.AnalogAccelPos;
    protected override Vec2 AnalogValueOffset => config.AnalogAccelValueOffset;
    protected override Vec2 AnalogValueScale => config.AnalogAccelValueScale;
    protected override Vec3 AnalogValueColor => config.AnalogAccelValueColor;

    public AccelValueBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.config = config;
    }
}
