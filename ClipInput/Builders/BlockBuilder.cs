using GBX.NET.Engines.Control;
using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using GbxToolAPI;
using TmEssentials;

namespace ClipInput.Builders;

abstract class BlockBuilder
{
    private readonly IReadOnlyCollection<IInput> inputs;
    private readonly ClipInputConfig config;

    public BlockBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config)
    {
        this.inputs = inputs;
        this.config = config;
    }

    public abstract IEnumerable<CGameCtnMediaBlock> BuildBlocks(TimeInt32? blockEndTime, TimeInt32? inputEndTime);

    protected TimeSingle GetFirstInputTime()
    {
        var firstInputTime = inputs.FirstOrDefault()?.Time.ToTimeSingle() ?? TimeSingle.Zero;

        if (firstInputTime > TimeSingle.Zero)
        {
            return TimeSingle.Zero;
        }
        
        return firstInputTime;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="block"></param>
    /// <param name="time">Actual time without an applied <see cref="ClipInputConfig.StartOffset"/>.</param>
    /// <exception cref="NotSupportedException"></exception>
    protected void CloseState(CGameCtnMediaBlock block, TimeSingle time)
    {
        // Ends the previous section with the properties of the latest key

        switch (block)
        {
            case CGameCtnMediaBlockTriangles trianglesBlock:
                trianglesBlock.Keys.Add(new CGameCtnMediaBlockTriangles.Key(trianglesBlock)
                {
                    Time = time + config.StartOffset,
                    Positions = trianglesBlock.Keys.Last().Positions // Takes the latest key
                });
                break;
            case CGameCtnMediaBlockImage imageBlock:
                CloneSimiKey(time, imageBlock.Effect);
                break;
            case CGameCtnMediaBlockText textBlock:
                CloneSimiKey(time, textBlock.Effect);
                break;
            default:
                throw new NotSupportedException("This MediaTracker block is not supported");
        }
    }

    private void CloneSimiKey(TimeSingle time, CControlEffectSimi effect)
    {
        var lastKey = effect.Keys.Last(); // Takes the latest key

        effect.Keys.Add(new CControlEffectSimi.Key()
        {
            Time = time + config.StartOffset,
            Depth = lastKey.Depth,
            IsContinuousEffect = lastKey.IsContinuousEffect,
            Opacity = lastKey.Opacity,
            Position = lastKey.Position,
            Rotation = lastKey.Rotation,
            Scale = lastKey.Scale,
            U01 = lastKey.U01,
            U02 = lastKey.U02,
            U03 = lastKey.U03
        });
    }
}
