using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

abstract class WalkBuilder : BlockBuilder
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;
    private readonly EWalk pressedState;

    public WalkBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config, EWalk pressedState) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
        this.pressedState = pressedState;
    }

    protected abstract CGameCtnMediaBlock Initiate(TimeSingle time, bool pressed);

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? endTime)
    {
        if (!inputs.OfType<Walk>().Any())
        {
            yield break;
        }
        
        var block = Initiate(GetFirstInputTime() + config.StartOffset, pressed: false);
        
        var prevTime = default(TimeInt32?);
        var prevPressed = EWalk.None;

        foreach (var walk in inputs.OfType<Walk>())
        {
            if (pressedState is EWalk.Forward && walk.Pressed is EWalk.None && prevPressed is EWalk.Backward
             || pressedState is EWalk.Backward && walk.Pressed is EWalk.None && prevPressed is EWalk.Forward
             || pressedState is EWalk.Forward && walk.Pressed is EWalk.Backward && prevPressed is EWalk.None
             || pressedState is EWalk.Backward && walk.Pressed is EWalk.Forward && prevPressed is EWalk.None)
            {
                prevPressed = walk.Pressed;
                continue;
            }
            
            var newBlockInstance = ApplyDigital(block, walk, prevTime);

            prevTime = walk.Time;
            prevPressed = walk.Pressed;

            if (newBlockInstance is null)
            {
                continue; // accelBlock has a growing effect
            }

            yield return block;

            block = newBlockInstance;
        }

        if (endTime.HasValue)
        {
            CloseState(block, endTime.Value);

            //AnimateClosePad(accelBlock, endTime.Value); apply only if there are no input within animation time and ghost state is longer

            yield return block;
        }
    }

    private CGameCtnMediaBlock? ApplyDigital(CGameCtnMediaBlock block, Walk walk, TimeInt32? prevTime)
    {
        return ApplyDigital(block, walk.Time, walk.Pressed == pressedState, prevTime);
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
        return Initiate(timeSingle + config.StartOffset, pressed);
    }
}
