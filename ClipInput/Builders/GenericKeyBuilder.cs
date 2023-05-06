using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

abstract class GenericKeyBuilder<T> : BlockBuilder where T : IInputState
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;

    protected abstract bool Enable { get; }
    protected abstract CGameCtnMediaBlock Initiate(TimeSingle time, bool pressed);

    public GenericKeyBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
    }

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? blockEndTime, TimeInt32? inputEndTime)
    {
        if (!Enable || !inputs.OfType<T>().Any(x => x.Pressed))
        {
            yield break;
        }

        var earliestInputTime = GetFirstInputTime();

        var block = Initiate(earliestInputTime, pressed: false);

        var prevTime = default(TimeInt32?);

        foreach (var input in inputs)
        {
            CGameCtnMediaBlock? newBlockInstance;

            if (input is T keyInput)
            {
                newBlockInstance = Apply(block, keyInput, prevTime);
            }
            else if (input.Time == inputEndTime && input is FakeFinishLine)
            {
                newBlockInstance = Apply(block, input.Time, false, prevTime);
            }
            else
            {
                continue;
            }
            
            prevTime = input.Time;

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

            yield return block;
        }
    }

    private CGameCtnMediaBlock? Apply(CGameCtnMediaBlock block, T input, TimeInt32? prevTime)
    {
        return Apply(block, input.Time, input.Pressed, prevTime);
    }

    private CGameCtnMediaBlock? Apply(CGameCtnMediaBlock block, TimeInt32 time, bool pressed, TimeInt32? prevTime)
    {
        var timeSingle = time.ToTimeSingle();

        // Check with previous time and apply minimal 1 frame length if AdjustToFPS
        if (config.AdjustToFPS && !pressed && prevTime.HasValue && (time - prevTime.Value).ToTimeSingle() < config.GetMinimalFrameLength())
        {
            timeSingle = prevTime.Value.ToTimeSingle() + config.GetMinimalFrameLength();
        }

        CloseState(block, timeSingle);

        // Creates a new block with a key at the same time position
        return Initiate(timeSingle + config.StartOffset, pressed);
    }
}
