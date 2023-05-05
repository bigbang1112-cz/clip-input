using GBX.NET.Inputs;

namespace ClipInput.Builders;

abstract class SteerBuilderBase : BlockBuilder
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;

    protected SteerBuilderBase(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
    }

    protected bool IsAnalogSteeringKeyboardOnly()
    {
        foreach (var input in inputs)
        {
            if (input is not IInputSteer steer)
            {
                continue;
            }

            var value = steer.GetValue();

            if (value is (-1) or 0 or 1)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    protected bool IsSteeringAnalogOnly()
    {
        foreach (var input in inputs)
        {
            if (input is SteerLeft || input is SteerRight)
            {
                return false;
            }
        }

        return true;
    }

    protected bool StartsWithKeyboardSteer()
    {
        foreach (var input in inputs)
        {
            if (input is SteerLeft || input is SteerRight)
            {
                return true;
            }

            if (input is IInputSteer)
            {
                return false;
            }
        }

        return config.DefaultDevice is SteerDevice.Keyboard;
    }

    protected bool IsCharacter()
    {
        foreach (var input in inputs)
        {
            if (input is IInputSteer)
            {
                return false;
            }

            if (input is Strafe)
            {
                return true;
            }
        }

        return false;
    }
}