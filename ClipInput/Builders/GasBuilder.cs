using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

abstract class GasBuilder<TDigital, TAnalog> : GasBuilderBase<TDigital, TAnalog> where TDigital : IInputState where TAnalog : IInputReal
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;

    public GasBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
    }

    protected abstract CGameCtnMediaBlock InitiateAnalog(TimeSingle timeSingle, float value);
    protected abstract CGameCtnMediaBlock InitiateDigital(TimeSingle time, bool pressed);
    protected abstract void ApplyAnalog(CGameCtnMediaBlock block, TimeInt32 time, float value);

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? blockEndTime, TimeInt32? inputEndTime)
    {
        if (IsCharacter())
        {
            yield break;
        }
        
        var initialGasHappenedAt = default(TimeInt32?);
        var initialGasSolved = false;

        var analogOnly = IsAnalogOnly();
        var digitalOnly = analogOnly && IsAnalogGasDigitalOnly();
        var startsWithDigital = digitalOnly || !analogOnly && StartsWithDigitalGas();
        var earliestInputTime = GetFirstInputTime();

        var block = startsWithDigital
            ? InitiateDigital(earliestInputTime + config.StartOffset, pressed: false)
            : InitiateAnalog(earliestInputTime + config.StartOffset, value: 0);

        var keyPressed = analogOnly && startsWithDigital ? false : default(bool?);

        var prevDevice = SteerDevice.None;
        var prevTime = default(TimeInt32?);

        var prevGasValue = 0f;

        foreach (var input in inputs)
        {
            // This is a solution to analog bindings which occur at maximum value at the start of the replay.
            if (SkipInitialGasAtSameTick(input, ref initialGasHappenedAt, ref initialGasSolved))
            {
                continue;
            }
            //

            if (input is Gas gasToOptimize) // optimization of the gas keyframing
            {
                if (this is AccelBuilder && prevGasValue <= 0 && gasToOptimize.NormalizedValue <= 0)
                {
                    continue;
                }
                
                if (this is BrakeBuilder && prevGasValue >= 0 && gasToOptimize.NormalizedValue >= 0)
                {
                    continue;
                }

                prevGasValue = gasToOptimize.NormalizedValue;
            }
            
            var newBlockInstance = input switch
            {
                TDigital digital => ApplyDigital(block, digital, prevTime),
                TAnalog analog => ApplyAnalog(block, analog, ref keyPressed, prevDevice, prevTime),
                Gas gas => ApplyAnalog(block, gas, ref keyPressed, prevDevice, prevTime),
                _ => null
            };

            if (input.Time == inputEndTime && input is FakeFinishLine) // End of race input reset
            {
                newBlockInstance = prevDevice switch
                {
                    SteerDevice.Pad => ApplyAnalog(block, new Gas(input.Time, 0), ref keyPressed, prevDevice, prevTime),
                    SteerDevice.Keyboard => ApplyDigital(block, input.Time, false, prevTime),
                    _ => null
                };
            }

            switch (input)
            {
                case TDigital:
                    prevDevice = SteerDevice.Keyboard;
                    prevTime = input.Time;
                    break;
                case TAnalog:
                case Gas:
                    prevDevice = SteerDevice.Pad;
                    prevTime = input.Time;
                    break;
            }

            if (newBlockInstance is null)
            {
                continue; // accelBlock has a growing effect
            }

            yield return block;

            block = newBlockInstance;
        }

        if (blockEndTime.HasValue)
        {
            CloseState(block, blockEndTime.Value);

            //AnimateClosePad(accelBlock, endTime.Value); apply only if there are no input within animation time and ghost state is longer

            yield return block;
        }
    }

    private CGameCtnMediaBlock? ApplyDigital(CGameCtnMediaBlock block, TDigital digital, TimeInt32? prevTime)
    {
        return ApplyDigital(block, digital.Time, digital.Pressed, prevTime);
    }

    private CGameCtnMediaBlock? ApplyDigital(CGameCtnMediaBlock block, TimeInt32 time, bool pressed, TimeInt32? prevTime)
    {
        var timeSingle = time.ToTimeSingle();

        // Check with previous time and apply minimal 1 frame length if AdjustToFPS
        if (config.AdjustToFPS && !pressed && prevTime.HasValue && (time - prevTime.Value).ToTimeSingle() < config.GetMinimalFrameLength())
        {
            timeSingle = prevTime.Value.ToTimeSingle() + config.GetMinimalFrameLength();
        }

        CloseState(block, timeSingle);

        // Creates a new block with a key at the same time position
        return InitiateDigital(timeSingle + config.StartOffset, pressed);
    }

    private CGameCtnMediaBlock? ApplyAnalog(CGameCtnMediaBlock block, IInputReal input, ref bool? keyPressed, SteerDevice prevDevice, TimeInt32? prevTime)
    {
        var val = input.GetValue();

        if (this is AccelBuilder && val < 0)
        {
            val = 0;
        }

        if (this is BrakeBuilder)
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

        if (keyPressed.HasValue)
        {
            var pressed = val switch
            {
                -1 => false,
                0 => false,
                1 => true,
                _ => throw new InvalidOperationException()
            };

            if (keyPressed.Value == pressed)
            {
                return null;
            }

            keyPressed = pressed;

            return ApplyDigital(block, input.Time, pressed, prevTime);
        }

        if (prevDevice == SteerDevice.Keyboard)
        {
            CloseState(block, input.Time);

            return InitiateAnalog(input.Time, val);
        }

        CloseState(block, input.Time.ToTimeSingle() - config.InterpTolerance);

        ApplyAnalog(block, input.Time, val);

        return null;
    }
}
