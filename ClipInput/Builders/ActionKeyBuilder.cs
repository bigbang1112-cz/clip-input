using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class ActionKeyBuilder : BlockBuilder
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;
    private readonly int index;
    private readonly bool isShootMania;
    private readonly bool newActionKeyLayout;

    public ActionKeyBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config, int index, bool isShootMania, bool newActionKeyLayout) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
        this.index = index;
        this.isShootMania = isShootMania;
        this.newActionKeyLayout = newActionKeyLayout;
    }

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? blockEndTime, TimeInt32? inputEndTime)
    {        
        var earliestInputTime = GetFirstInputTime();

        var block = Initiate(earliestInputTime + config.StartOffset, pressed: false, activated: false);
        
        var slot = (newActionKeyLayout ? (index * 2 + 2) : (index + 1)) % 10;

        var prevTime = default(TimeInt32?);
        var activated = false;

        foreach (var input in inputs.OfType<ActionSlot>())
        {
            var pressed = input.Pressed;

            if (input.Slot != slot)
            {
                if (pressed && activated)
                {
                    activated = false;
                    pressed = false;
                }
                else
                {
                    continue;
                }
            }
            else if (!isShootMania && pressed && slot != 0) // + not reset key
            {
                activated = !activated;
            }

            var newBlockInstance = Apply(block, input.Time, pressed, prevTime, activated);

            prevTime = input.Time;
            
            yield return block;
            block = newBlockInstance;
        }

        if (blockEndTime.HasValue)
        {
            CloseState(block, blockEndTime.Value);
            
            yield return block;
        }
    }

    private CGameCtnMediaBlock Initiate(TimeSingle time, bool pressed, bool activated)
    {
        return config.Design.InitiateActionSlot(time, index, pressed, activated);
    }

    private CGameCtnMediaBlock Apply(CGameCtnMediaBlock block, TimeInt32 time, bool pressed, TimeInt32? prevTime, bool activated)
    {
        var timeSingle = time.ToTimeSingle();

        // Check with previous time and apply minimal 1 frame length if AdjustToFPS
        if (config.AdjustToFPS && !pressed && prevTime.HasValue && (time - prevTime.Value).ToTimeSingle() < config.GetMinimalFrameLength())
        {
            timeSingle = prevTime.Value.ToTimeSingle() + config.GetMinimalFrameLength();
        }

        CloseState(block, timeSingle);

        // Creates a new block with a key at the same time position
        return Initiate(timeSingle + config.StartOffset, pressed, activated);
    }
}
