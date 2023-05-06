using GBX.NET;
using GBX.NET.Engines.Control;
using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class SteerValueBuilder : SteerBuilderBase
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly IDesign design;
    private readonly ClipInputConfig config;
    private readonly bool left;
    private readonly bool inverse;

    public SteerValueBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;

        design = config.Design;
        left = design.IsLeftSteer().GetValueOrDefault();
        inverse = inputs.FirstOrDefault(x => x is FakeDontInverseAxis a && a.Pressed) is not null;
    }

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? blockEndTime, TimeInt32? inputEndTime)
    {
        var analogOnly = IsSteeringAnalogOnly();
        var keyboardOnly = analogOnly && IsAnalogSteeringKeyboardOnly();

        if (keyboardOnly)
        {
            yield break;
        }

        var startsWithKeyboard = keyboardOnly || !analogOnly && StartsWithKeyboardSteer();
        var earliestInputTime = GetFirstInputTime();

        var block = startsWithKeyboard
            ? null
            : InitiateAnalogSteer(earliestInputTime, value: 0);

        var prevSteerInput = default(IInput);
        var prevValue = default(float?);

        foreach (var input in inputs)
        {
            if (block is not null && input is SteerLeft or SteerRight)
            {
                CloseState(block, input.Time);

                yield return block;

                block = null;
                prevSteerInput = input;

                continue;
            }

            if (input.Time == inputEndTime && input is FakeFinishLine && prevSteerInput is IInputSteer) // End of race input reset
            {
                var zeroingInstance = ApplyAnalogSteer(block, new Steer(input.Time, 0), isTransitionFromKb: prevSteerInput is SteerLeft or SteerRight, prevValue);

                if (block is not null)
                {
                    yield return block;
                }

                block = zeroingInstance;

                continue;
            }

            if (input is not IInputSteer steer)
            {
                continue;
            }

            var newBlockInstance = ApplyAnalogSteer(block, steer, isTransitionFromKb: prevSteerInput is SteerLeft or SteerRight, prevValue);

            prevSteerInput = input;
            prevValue = steer.NormalizedValue * (inverse ? -1 : 1);

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time">Actual time without an applied <see cref="ClipInputConfig.StartOffset"/>.</param>
    /// <param name="value"></param>
    /// <returns></returns>
    private CGameCtnMediaBlockText InitiateAnalogSteer(TimeInt32 time, float value)
    {
        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        var block = CGameCtnMediaBlockText.Create(effect)
            .WithText(string.Format(config.AnalogValueTextFormat, value.ToString(config.AnalogValueNumberFormat, config.Formatting)))
            .WithColor(config.AnalogSteerValueColor)
            .ForTMUF()
            .Build();

        var key = GetAnalogKeyframe(time);

        effect.Keys.Add(key);

        return block;
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

        var x = (config.AnalogSteerPos.X + config.AnalogSteerSpacing.X + config.AnalogSteerValueOffset.X) * design.GetSteerMultiplier() * aspectRatio;
        var y = config.AnalogSteerPos.Y + config.AnalogSteerValueOffset.Y;

        return new CControlEffectSimi.Key
        {
            Time = time.ToTimeSingle() + config.StartOffset,
            Position = new Vec2(x, y) * config.PrimaryScale + config.PrimaryPos,
            Scale = config.AnalogSteerValueScale * config.PrimaryScale,
            Depth = 0
        };
    }

    private CGameCtnMediaBlockText? ApplyAnalogSteer(CGameCtnMediaBlockText? block, IInputSteer steer, bool isTransitionFromKb, float? prevValue)
    {
        var val = steer.GetValue() * (inverse ? -1 : 1);

        if (!isTransitionFromKb && prevValue.HasValue) // Optimizes pads when they are unused
        {
            if (!left && val < 0 && prevValue <= 0)
            {
                return null;
            }

            if (left && val > 0 && prevValue >= 0)
            {
                return null;
            }
        }

        if (block is not null)
        {
            CloseState(block, steer.Time);
        }

        if (left)
        {
            val = val >= 0 ? 0 : -val;
        }
        else if (!left && val <= 0)
        {
            val = 0;
        }

        return InitiateAnalogSteer(steer.Time, val);
    }
}
