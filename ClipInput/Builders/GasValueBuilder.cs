using GBX.NET;
using GBX.NET.Engines.Control;
using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

abstract class GasValueBuilder<TDigital, TAnalog> : GasBuilderBase<TDigital, TAnalog> where TDigital : IInputState where TAnalog : IInputReal
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;

    public GasValueBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
    }

    protected abstract Vec2 AnalogPos { get; }
    protected abstract Vec2 AnalogValueOffset { get; }
    protected abstract Vec2 AnalogValueScale { get; }
    protected abstract Vec3 AnalogValueColor { get; }

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? blockEndTime, TimeInt32? inputEndTime)
    {
        var analogOnly = IsAnalogOnly();
        var digitalOnly = analogOnly && IsAnalogGasDigitalOnly();

        if (digitalOnly)
        {
            yield break;
        }
        
        var initialGasHappenedAt = default(TimeInt32?);
        var initialGasSolved = false;

        var startsWithDigital = !analogOnly && StartsWithDigitalGas();
        var earliestInputTime = GetFirstInputTime();

        var block = startsWithDigital
            ? null
            : InitiateAnalog(earliestInputTime + config.StartOffset, value: 0);

        var isPrevDigital = startsWithDigital;

        foreach (var input in inputs)
        {
            // This is a solution to analog bindings which occur at maximum value at the start of the replay.
            if (SkipInitialGasAtSameTick(input, ref initialGasHappenedAt, ref initialGasSolved))
            {
                continue;
            }
            //

            if (block is not null && input is TDigital)
            {
                CloseState(block, input.Time);

                yield return block;

                block = null;
                isPrevDigital = true;

                continue;
            }

            if (input.Time == inputEndTime && input is FakeFinishLine && !isPrevDigital) // End of race input reset
            {
                var zeroingInstance = ApplyAnalog(block, new Gas(input.Time, 0));

                if (block is not null)
                {
                    yield return block;
                }

                block = zeroingInstance;

                continue;
            }

            if (input is not TAnalog and not Gas)
            {
                continue;
            }

            isPrevDigital = false;

            var newBlockInstance = ApplyAnalog(block, (IInputReal)input);

            if (newBlockInstance is null)
            {
                continue; // growing effect
            }

            if (block is not null)
            {
                yield return block;
            }

            block = newBlockInstance;
        }

        if (block is not null && blockEndTime.HasValue)
        {
            CloseState(block, blockEndTime.Value);

            yield return block;
        }
    }

    private CGameCtnMediaBlockText InitiateAnalog(TimeSingle time, float value)
    {
        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        var block = CGameCtnMediaBlockText.Create(effect)
            .WithText(string.Format(config.AnalogValueTextFormat, value.ToString(config.AnalogValueNumberFormat, config.Formatting)))
            .WithColor(AnalogValueColor)
            .ForTMUF()
            .Build();

        var key = GetAnalogKeyframe(time);

        effect.Keys.Add(key);

        return block;
    }

    private CGameCtnMediaBlockText? ApplyAnalog(CGameCtnMediaBlockText? block, IInputReal input)
    {
        var val = input.GetValue();

        if (block is not null)
        {
            CloseState(block, input.Time);
        }

        if (this is AccelValueBuilder && val < 0)
        {
            val = 0;
        }

        if (this is BrakeValueBuilder)
        {
            if (input is Gas)
            {
                val = val >= 0 ? 0 : -val;
            }
            else if (input is BrakeReal)
            {
                val = val <= 0 ? 0 : val;
            }
        }

        return InitiateAnalog(input.Time, val);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time">Actual time without an applied <see cref="ClipInputConfig.StartOffset"/>.</param>
    /// <param name="value"></param>
    /// <returns></returns>
    private CControlEffectSimi.Key GetAnalogKeyframe(TimeInt32 time)
    {
        var aspectRatio = config.GetAspectRatio();

        var x = (AnalogPos.X + AnalogValueOffset.X) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var y = (AnalogPos.Y + AnalogValueOffset.Y) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        return new CControlEffectSimi.Key
        {
            Time = time.ToTimeSingle() + config.StartOffset,
            Position = (x, y),
            Scale = AnalogValueScale * config.PrimaryScale,
            Depth = 0
        };
    }
}
