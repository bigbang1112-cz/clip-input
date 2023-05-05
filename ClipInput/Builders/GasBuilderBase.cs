using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

abstract class GasBuilderBase<TDigital, TAnalog> : BlockBuilder where TDigital : IInputState where TAnalog : IInputReal
{
    private readonly IReadOnlyCollection<IInput> inputs;

    protected GasBuilderBase(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
    }

    protected bool IsAnalogOnly()
    {
        foreach (var input in inputs)
        {
            if (input is TDigital)
            {
                return false;
            }
        }

        return true;
    }

    protected bool IsAnalogGasDigitalOnly()
    {
        foreach (var input in inputs)
        {
            if (input is not IInputReal inputReal || inputReal is not TAnalog and not Gas)
            {
                continue;
            }

            var value = inputReal.GetValue();

            if (value is (-1) or 0 or 1)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    protected bool StartsWithDigitalGas()
    {
        var initialGasHappenedAt = default(TimeInt32?);
        var initialGasSolved = false;
        
        foreach (var input in inputs)
        {
            // This is a solution to analog bindings which occur at maximum value at the start of the replay.
            if (SkipInitialGasAtSameTick(input, ref initialGasHappenedAt, ref initialGasSolved))
            {
                continue;
            }
            //
            
            if (input is TDigital)
            {
                return true;
            }

            if (input is TAnalog or Gas)
            {
                return false;
            }
        }

        return true;
    }

    protected static bool SkipInitialGasAtSameTick(IInput input, ref TimeInt32? initialGasHappenedAt, ref bool initialGasSolved)
    {
        if (initialGasSolved)
        {
            return false;
        }
        
        if (input is Gas or Accelerate or AccelerateReal or Brake or BrakeReal)
        {
            if (initialGasHappenedAt is null)
            {
                initialGasHappenedAt = input.Time;
            }
            else if (input.Time == initialGasHappenedAt.Value)
            {
                return true;
            }
        }

        if (initialGasHappenedAt.HasValue && input.Time > initialGasHappenedAt.Value)
        {
            initialGasSolved = true;
        }

        return false;
    }

    protected bool IsCharacter()
    {
        foreach (var input in inputs)
        {
            if (input is Walk)
            {
                return true;
            }
            
            if (input is Accelerate or AccelerateReal or Brake or BrakeReal)
            {
                return false;
            }
        }

        return false;
    }
}