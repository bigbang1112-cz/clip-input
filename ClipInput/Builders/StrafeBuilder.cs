using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class StrafeBuilder : BlockBuilder
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;
    private readonly EStrafe pressedState;

    public StrafeBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
        
        pressedState = config.Design.IsLeftSteer().GetValueOrDefault() ? EStrafe.Left : EStrafe.Right;
    }

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? blockEndTime, TimeInt32? inputEndTime)
    {
        if (!inputs.OfType<Strafe>().Any())
        {
            yield break;
        }

        var block = config.Design.InitiateDigitalSteer(GetFirstInputTime() + config.StartOffset, pressed: false);
        
        var prevTime = default(TimeInt32?);
        var prevPressed = default(EStrafe?);

        foreach (var strafe in inputs.OfType<Strafe>())
        {
            if (pressedState is EStrafe.Left && strafe.Pressed is EStrafe.None && prevPressed is EStrafe.Right
             || pressedState is EStrafe.Right && strafe.Pressed is EStrafe.None && prevPressed is EStrafe.Left
             || pressedState is EStrafe.Left && strafe.Pressed is EStrafe.Right && prevPressed is EStrafe.None
             || pressedState is EStrafe.Right && strafe.Pressed is EStrafe.Left && prevPressed is EStrafe.None)
            {
                prevPressed = strafe.Pressed;
                continue;
            }

            var newBlockInstance = ApplyDigital(block, strafe, prevTime);

            prevTime = strafe.Time;
            prevPressed = strafe.Pressed;

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

    private CGameCtnMediaBlock? ApplyDigital(CGameCtnMediaBlock block, Strafe strafe, TimeInt32? prevTime)
    {
        return ApplyDigital(block, strafe.Time, strafe.Pressed == pressedState, prevTime);
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
        return config.Design.InitiateDigitalSteer(timeSingle + config.StartOffset, pressed);
    }
}
