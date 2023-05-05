using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class SteerBackBuilder : SteerBuilderBase
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly IDesign design;
    private readonly bool left;

    public SteerBackBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;

        design = config.Design;
        left = design.IsLeftSteer().GetValueOrDefault();
    }

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? endTime)
    {
        var analogOnly = IsSteeringAnalogOnly();
        var keyboardOnly = analogOnly && IsAnalogSteeringKeyboardOnly();

        if (keyboardOnly)
        {
            yield break;
        }

        var startsWithKeyboard = keyboardOnly || !analogOnly && StartsWithKeyboardSteer();

        var block = startsWithKeyboard
            ? null
            : design.InitiateAnalogSteerBack(GetFirstInputTime());

        var prevSteerInput = default(IInput);

        foreach (var input in inputs)
        {
            if (block is not null && input is SteerLeft or SteerRight)
            {
                CloseState(block, input.Time);

                yield return block;

                block = null;
                prevSteerInput = input;

                continue;
            }

            if (input is not IInputSteer steer || prevSteerInput is null or IInputSteer)
            {
                continue;
            }

            var newBlockInstance = ApplyAnalogSteer(block, steer, isTransitionFromKb: prevSteerInput is SteerLeft or SteerRight);

            prevSteerInput = input;

            if (newBlockInstance is null)
            {
                continue; // growing effect
            }

            if (block is not null)
            {
                yield return block;
            }

            block = newBlockInstance;
        }

        if (block is not null && endTime.HasValue)
        {
            CloseState(block, endTime.Value);

            yield return block;
        }
    }

    private CGameCtnMediaBlock? ApplyAnalogSteer(CGameCtnMediaBlock? block, IInputSteer steer, bool isTransitionFromKb)
    {
        var val = steer.GetValue();

        if (!isTransitionFromKb && (!left || val > 0) && (left || val < 0))
        {
            return null;
        }

        if (block is not null)
        {
            CloseState(block, steer.Time);
        }

        return design.InitiateAnalogSteerBack(steer.Time);
    }
}
