using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

abstract class GasBackBuilder<TDigital, TAnalog> : GasBuilderBase<TDigital, TAnalog> where TDigital : IInputState where TAnalog : IInputReal
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;

    public GasBackBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
    }
    
    protected abstract CGameCtnMediaBlock? InitiateAnalog(TimeSingle time);

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

        var startsWithDigital = digitalOnly || !analogOnly && StartsWithDigitalGas();

        var block = startsWithDigital
            ? null
            : InitiateAnalog(GetFirstInputTime() + config.StartOffset);

        var prevDevice = startsWithDigital ? SteerDevice.Keyboard : SteerDevice.Pad;

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
                if (block is not null)
                {
                    CloseState(block, input.Time);

                    yield return block;

                    block = null;
                    prevDevice = SteerDevice.Keyboard;
                }
                
                continue;
            }
            
            if (input is not TAnalog and not Gas || prevDevice == SteerDevice.Pad)
            {
                continue;
            }

            var newBlockInstance = InitiateAnalog(input.Time);

            if (block is not null)
            {
                yield return block;
            }

            prevDevice = SteerDevice.Pad;
            block = newBlockInstance;
        }

        if (block is not null && blockEndTime.HasValue)
        {
            CloseState(block, blockEndTime.Value);

            //AnimateClosePad(accelBlock, endTime.Value); apply only if there are no input within animation time and ghost state is longer

            yield return block;
        }
    }
}
