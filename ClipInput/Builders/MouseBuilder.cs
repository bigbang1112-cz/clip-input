using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class MouseBuilder : BlockBuilder
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;

    public MouseBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
    }

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? blockEndTime, TimeInt32? inputEndTime)
    {
        if (!config.EnableMouse || !IncludesMouse())
        {
            yield break;
        }

        var earliestInputTime = GetFirstInputTime();

        var block = config.Design.InitiateMouse(earliestInputTime, leftClick: false, rightClick: false, curPos: (0, 0), offset: (0, 0));

        if (block is null)
        {
            yield break;
        }

        var prevMouseAccu = default(MouseAccu?);

        var curPos = new Vec2();
        var offset = new Vec2();

        var leftClick = false;
        var rightClick = false;

        foreach (var input in inputs)
        {
            if (input is not MouseAccu mouseAccu)
            {
                switch (input)
                {
                    case GunTrigger gunTrigger:
                        leftClick = gunTrigger.Pressed;
                        break;
                    case GBX.NET.Inputs.Action action:
                        rightClick = action.Pressed;
                        break;
                }

                if (input is GunTrigger or GBX.NET.Inputs.Action)
                {
                    CloseState(block, input.Time);

                    yield return block;

                    block = config.Design.InitiateMouse(input.Time, leftClick, rightClick, curPos, offset);
                }

                continue; // GunTrigger, Action
            }

            if (!config.EnableMouseMovement)
            {
                continue;
            }

            if (prevMouseAccu.HasValue)
            {
                var changeX = -CalcSubPixelDistance(mouseAccu.X, prevMouseAccu.Value.X); // Fix for MediaTracker
                var changeY = CalcSubPixelDistance(mouseAccu.Y, prevMouseAccu.Value.Y);

                curPos += (changeX, changeY);

                // offset: gradually follow curPos
                offset += (curPos - offset) * 0.1f;
            }

            if (prevMouseAccu.HasValue && (mouseAccu.Time - prevMouseAccu.Value.Time > new TimeInt32(200)))
            {
                CloseState(block, mouseAccu.Time - new TimeInt32(150));
            }

            config.Design.ApplyMouse(block, mouseAccu.Time.ToTimeSingle() + config.StartOffset, curPos, offset);

            prevMouseAccu = mouseAccu;
        }

        if (blockEndTime.HasValue)
        {
            CloseState(block, blockEndTime.Value);
        }

        yield return block;
    }

    private static int CalcSubPixelDistance(ushort mouseAccu, ushort prevMouseAccu)
    {
        var change = mouseAccu - prevMouseAccu;

        // Overflow fix
        if (change > short.MaxValue)
        {
            change -= ushort.MaxValue;
        }
        else if (change < short.MinValue)
        {
            change += ushort.MaxValue;
        }

        return -change; // Invert for correct direction
    }

    private bool IncludesMouse()
    {
        var mouseAccuTimes = 0;

        foreach (var input in inputs)
        {
            if (input is GunTrigger || input is GBX.NET.Inputs.Action)
            {
                return true;
            }

            if (input is MouseAccu)
            {
                mouseAccuTimes++;

                if (mouseAccuTimes >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
