using GBX.NET.Engines.Control;
using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

abstract class GenericKeyTapBuilder<T> : BlockBuilder where T : struct, IInput
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;

    protected abstract bool Enable { get; }

    protected abstract CGameCtnMediaBlock Initiate(TimeSingle time, bool pressed);

    public GenericKeyTapBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.inputs = inputs;
        this.config = config;
    }

    public override IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? blockEndTime, TimeInt32? inputEndTime)
    {
        if (!Enable || !inputs.OfType<T>().Any())
        {
            yield break;
        }

        var input = default(T?);

        foreach (var nextInput in inputs.OfType<T>())
        {
            if (nextInput is IInputState nextInputState && !nextInputState.Pressed) // Release state is unnecessary as it is 1 tick right after
            {
                continue;
            }

            if (input is null)
            {
                input = nextInput;

                var block = Initiate(GetFirstInputTime(), pressed: false);
                CloseState(block, input.Value.Time);
                yield return block;

                continue;
            }

            var pressEnd = nextInput.Time - input.Value.Time < config.KeyTapPressTime ? nextInput.Time : input.Value.Time + config.KeyTapPressTime;

            var curTime = input.Value.Time.ToTimeSingle() + config.StartOffset;

            var pressedBlock = Initiate(curTime, pressed: true);
            var pressedEffect = GetEffectFromBlock(pressedBlock);
            CloseState(pressedBlock, pressEnd);
            pressedEffect.Keys.Last().Opacity = 0;
            yield return pressedBlock;

            var releasedBlock = Initiate(curTime, pressed: false);
            var releasedEffect = GetEffectFromBlock(releasedBlock);
            CloseState(releasedBlock, pressEnd);
            releasedEffect.Keys.First().Opacity = 0.5f;

            CloseState(releasedBlock, nextInput.Time);
            yield return releasedBlock;

            input = nextInput;
        }

        if (input is null)
        {
            yield break;
        }

        var lastPressEnd = input.Value.Time + config.KeyTapPressTime;

        var lastCurTime = input.Value.Time.ToTimeSingle() + config.StartOffset;

        var lastPressedBlock = Initiate(lastCurTime, pressed: true);
        var lastPressedEffect = GetEffectFromBlock(lastPressedBlock);
        CloseState(lastPressedBlock, lastPressEnd);
        lastPressedEffect.Keys.Last().Opacity = 0;
        yield return lastPressedBlock;

        var lastReleasedBlock = Initiate(lastCurTime, pressed: false);
        var lastReleasedEffect = GetEffectFromBlock(lastReleasedBlock);
        CloseState(lastReleasedBlock, lastPressEnd);
        lastReleasedEffect.Keys.First().Opacity = 0;

        if (blockEndTime.HasValue)
        {
            CloseState(lastReleasedBlock, blockEndTime.Value);
        }

        yield return lastReleasedBlock;
    }

    private static CControlEffectSimi GetEffectFromBlock(CGameCtnMediaBlock block) => block switch
    {
        CGameCtnMediaBlockText blockText => blockText.Effect,
        CGameCtnMediaBlockImage blockImage => blockImage.Effect,
        _ => throw new NotSupportedException(block.GetType().Name + " is not supported for key taps."),
    };
}
