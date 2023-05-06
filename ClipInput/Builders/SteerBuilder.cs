using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using System.Diagnostics.Metrics;
using TmEssentials;
using static GBX.NET.Engines.Plug.CPlugSolid2Model;

namespace ClipInput.Builders;

class SteerBuilder : SteerBuilderBase
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly IDesign design;
    private readonly ClipInputConfig config;
    private readonly bool left;
    private readonly bool inverse;

    public SteerBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;

        design = config.Design;
        left = design.IsLeftSteer().GetValueOrDefault();
        inverse = inputs.FirstOrDefault(x => x is FakeDontInverseAxis a && a.Pressed) is not null;
    }

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? blockEndTime, TimeInt32? inputEndTime)
    {
        if (IsCharacter())
        {
            yield break;
        }

        var analogOnly = IsSteeringAnalogOnly();
        var keyboardOnly = analogOnly && IsAnalogSteeringKeyboardOnly();
        var startsWithKeyboard = keyboardOnly || !analogOnly && StartsWithKeyboardSteer();
        var earliestInputTime = GetFirstInputTime();

        var steerBlock = startsWithKeyboard
            ? design.InitiateDigitalSteer(earliestInputTime + config.StartOffset, pressed: false)
            : design.InitiateAnalogSteer(earliestInputTime, value: 0);

        var keyPressed = analogOnly && startsWithKeyboard ? false : default(bool?);

        var prevDevice = SteerDevice.None;
        var prevTime = default(TimeInt32?);
        var prevValue = default(float?);

        foreach (var input in inputs)
        {
            var newBlockInstance = input switch
            {
                SteerLeft left => ApplyDigitalSteer(steerBlock, left, prevDevice, prevTime),
                SteerRight right => ApplyDigitalSteer(steerBlock, right, prevDevice, prevTime),
                IInputSteer steer => ApplyAnalogSteer(steerBlock, steer, ref keyPressed, prevDevice, prevTime, prevValue),
                _ => null
            };

            if (input.Time == inputEndTime && input is FakeFinishLine) // End of race input reset
            {
                newBlockInstance = prevDevice switch
                {
                    SteerDevice.Pad => ApplyAnalogSteer(steerBlock, new Steer(input.Time, 0), ref keyPressed, prevDevice, prevTime, prevValue),
                    SteerDevice.Keyboard => ApplyDigitalSteer(steerBlock, input.Time, pressed: false, prevTime),
                    _ => null
                };
            }

            switch (input)
            {
                case SteerLeft:
                case SteerRight:
                    prevDevice = SteerDevice.Keyboard;
                    prevTime = input.Time;
                    break;
                case IInputSteer s:
                    prevDevice = SteerDevice.Pad;
                    prevTime = input.Time;
                    prevValue = s.NormalizedValue * (inverse ? -1 : 1);
                    break;
            }

            if (newBlockInstance is null)
            {
                continue; // steerBlock has a growing effect
            }

            yield return steerBlock;

            steerBlock = newBlockInstance;
        }

        if (blockEndTime.HasValue)
        {
            CloseState(steerBlock, blockEndTime.Value);

            //AnimateClosePad(steerBlock, end.Value); apply only if there are no input within animation time and ghost state is longer

            yield return steerBlock;
        }
    }

    private CGameCtnMediaBlock? ApplyAnalogSteer(CGameCtnMediaBlock block, IInputSteer steer, ref bool? keyPressed, SteerDevice prevDevice, TimeInt32? prevTime, float? prevValue)
    {
        // under specific conditions, this could sometimes be emitted as keyboard state
        
        var val = steer.NormalizedValue * (inverse ? -1 : 1);

        if (keyPressed.HasValue)
        {
            var pressed = val switch
            {
                -1 => left,
                0 => false,
                1 => !left,
                _ => throw new InvalidOperationException()
            };

            if (keyPressed.Value == pressed)
            {
                return null;
            }

            keyPressed = pressed;

            return ApplyDigitalSteer(block, steer.Time, pressed, prevTime);
        }

        var isRightPad = val >= 0 && !left;
        var isLeftPad = val <= 0 && left;

        var visualValue = 0f;

        if (isLeftPad)
        {
            visualValue = -val;
        }
        else if (isRightPad)
        {
            visualValue = val;
        }

        if (prevDevice == SteerDevice.Keyboard)
        {
            CloseState(block, steer.Time);

            return design.InitiateAnalogSteer(steer.Time, isRightPad || isLeftPad ? visualValue : 0);
        }

        if (prevValue.HasValue) // Optimizes pads when they are unused
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
        
        CloseState(block, steer.Time.ToTimeSingle() - config.InterpTolerance);

        design.ApplyAnalogSteer(block, steer.Time, visualValue);

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="block"></param>
    /// <param name="steer"></param>
    /// <param name="prevDevice"></param>
    /// <returns>Null if to continue using <paramref name="block"/>, a new instance if to use a new block.</returns>
    private CGameCtnMediaBlock? ApplyDigitalSteer(CGameCtnMediaBlock block, IInputState steer, SteerDevice prevDevice, TimeInt32? prevTime)
    {
        var steerRelatesToThisBuilder = steer is SteerLeft && left || steer is SteerRight && !left;

        if (prevDevice == SteerDevice.Pad)
        {
            // Switches to keyboard no matter if pressed or not
            // prevTime null because AdjustToFPS feature shouldn't apply here
            return ApplyDigitalSteer(block, steer.Time, steerRelatesToThisBuilder && steer.Pressed, prevTime: null);
        }

        if (steerRelatesToThisBuilder)
        {
            return ApplyDigitalSteer(block, steer.Time, steer.Pressed, prevTime);
        }

        return null; // continue using the previous block
    }

    /// <summary>
    /// Ends the MediaTracker block of the previous state and adds a new one starting with a given time and press state.
    /// </summary>
    /// <param name="block"></param>
    /// <param name="time">Actual time without an applied <see cref="ClipInputConfig.StartOffset"/>.</param>
    /// <param name="pressed"></param>
    /// <param name="prevTime"></param>
    /// <returns></returns>
    private CGameCtnMediaBlock? ApplyDigitalSteer(CGameCtnMediaBlock block, TimeInt32 time, bool pressed, TimeInt32? prevTime)
    {
        var timeSingle = time.ToTimeSingle();

        // Check with previous time and apply minimal 1 frame length if AdjustToFPS
        if (config.AdjustToFPS && !pressed && prevTime.HasValue && (time - prevTime.Value).ToTimeSingle() < config.GetMinimalFrameLength())
        {
            timeSingle = prevTime.Value.ToTimeSingle() + config.GetMinimalFrameLength();
        }

        CloseState(block, timeSingle);

        // Creates a new block with a key at the same time position
        return design.InitiateDigitalSteer(timeSingle + config.StartOffset, pressed);
    }

    private static void AnimateClosePad(CGameCtnMediaBlockTriangles block, TimeSingle time)
    {
        var avgY = block.Keys.Last().Positions.Average(x => x.Y);

        // Ends the previous section with the properties of the latest key
        block.Keys.Add(new CGameCtnMediaBlockTriangles.Key(block)
        {
            Time = time + TimeSingle.FromSeconds(0.25f),
            Positions = Array.ConvertAll<Vec3, Vec3>(block.Keys.Last().Positions, vec => new(vec.X, avgY, vec.Z)) // Takes the latest key
        });
    }
}
