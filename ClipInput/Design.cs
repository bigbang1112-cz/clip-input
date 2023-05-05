using ClipInput.Builders;
using GBX.NET;
using GBX.NET.Engines.Game;
using TmEssentials;

namespace ClipInput;

public interface IDesign
{
    Skin Skin { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time">Time with already applied <see cref="ClipInputConfig.StartOffset"/>.</param>
    /// <param name="pressed"></param>
    /// <param name="left"></param>
    /// <returns></returns>
    CGameCtnMediaBlock InitiateDigitalSteer(TimeSingle time, bool pressed);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="time">Time with already applied <see cref="ClipInputConfig.StartOffset"/>.</param>
    /// <returns></returns>
    CGameCtnMediaBlock.Key GetDigitalSteerKeyframe(CGameCtnMediaBlock block, TimeSingle time);

    CGameCtnMediaBlock InitiateDigitalAccel(TimeSingle time, bool pressed);
    CGameCtnMediaBlock.Key GetDigitalAccelKeyframe(CGameCtnMediaBlock block, TimeSingle time);
    CGameCtnMediaBlock InitiateAnalogAccel(TimeSingle time, float value);
    CGameCtnMediaBlock.Key GetAnalogAccelKeyframe(CGameCtnMediaBlock block, TimeSingle time, float value);
    void ApplyAnalogAccel(CGameCtnMediaBlock block, TimeInt32 time, float val);

    CGameCtnMediaBlock InitiateDigitalBrake(TimeSingle time, bool pressed);
    CGameCtnMediaBlock.Key GetDigitalBrakeKeyframe(CGameCtnMediaBlock block, TimeSingle time);
    CGameCtnMediaBlock InitiateAnalogBrake(TimeSingle time, float value);
    CGameCtnMediaBlock.Key GetAnalogBrakeKeyframe(CGameCtnMediaBlock block, TimeSingle time, float value);
    void ApplyAnalogBrake(CGameCtnMediaBlock block, TimeInt32 time, float value);

    CGameCtnMediaBlock InitiateActionSlot(TimeSingle time, int index, bool pressed, bool activated);
    CGameCtnMediaBlock.Key GetActionSlotKeyframe(CGameCtnMediaBlock block, TimeSingle time, int index);
    CGameCtnMediaBlock? InitiateActionSlotNum(TimeSingle timeStart, TimeInt32 timeEnd, int index);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time">Actual time without an applied <see cref="ClipInputConfig.StartOffset"/>.</param>
    /// <param name="val"></param>
    /// <returns></returns>
    CGameCtnMediaBlock InitiateAnalogSteer(TimeInt32 time, float value);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="time">Actual time without an applied <see cref="ClipInputConfig.StartOffset"/>.</param>
    /// <param name="value"></param>
    /// <returns></returns>
    CGameCtnMediaBlock.Key GetAnalogSteerKeyframe(CGameCtnMediaBlock block, TimeInt32 time, float value);
    void ApplyAnalogSteer(CGameCtnMediaBlock block, TimeInt32 time, float value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time">Actual time without an applied <see cref="ClipInputConfig.StartOffset"/>.</param>
    /// <param name="value"></param>
    /// <returns></returns>
    CGameCtnMediaBlock? InitiateAnalogSteerBack(TimeSingle time);
    CGameCtnMediaBlock? InitiateAnalogAccelBack(TimeSingle time);
    CGameCtnMediaBlock? InitiateAnalogBrakeBack(TimeSingle time);
    
    CGameCtnMediaBlock InitiateMouse(TimeSingle time, bool leftClick, bool rightClick, Vec2 curPos, Vec2 offset);
    CGameCtnMediaBlock.Key GetMouseKeyframe(CGameCtnMediaBlock block, TimeSingle time, Vec2 curPos, Vec2 offset);
    void ApplyMouse(CGameCtnMediaBlock block, TimeSingle time, Vec2 curPos, Vec2 offset);

    CGameCtnMediaBlock InitiateJump(TimeSingle time, bool pressed);
    
    CGameCtnMediaBlock InitiateHorn(TimeSingle time, bool pressed);
    
    CGameCtnMediaBlock InitiateRespawn(TimeSingle time, bool pressed);
    CGameCtnMediaBlock InitiateSecondaryRespawn(TimeSingle time, bool pressed);

    /// <summary>
    /// Sets the steering direction. This shouldn't be called before or after <see cref="SteerBuilder"/> work, not inbetween.
    /// </summary>
    /// <param name="left"></param>
    void SetSteer(bool? left);
    
    bool? IsLeftSteer();
    int GetSteerMultiplier();
    
    void SetupForManiaPlanet();
}

public abstract class Design<TSkin, TDigitalBlock, TDigitalKeyframe, TAnalogBlock, TAnalogKeyframe> : IDesign
    where TSkin : Skin, new()
    where TDigitalBlock : CGameCtnMediaBlock
    where TDigitalKeyframe : CGameCtnMediaBlock.Key
    where TAnalogBlock : CGameCtnMediaBlock
    where TAnalogKeyframe : CGameCtnMediaBlock.Key
{
    private bool? left;
    
    protected const int ActionSlotCountPerRow = 5;

    protected ClipInputConfig Config { get; }
    protected bool IsManiaPlanet { get; private set; }

    public TSkin Skin => Config.Skin as TSkin ?? new TSkin();

    Skin IDesign.Skin => Skin;

    public Design(ClipInputConfig config)
    {
        Config = config;
    }

    public void SetupForManiaPlanet()
    {
        IsManiaPlanet = true;
    }

    public abstract TDigitalBlock InitiateDigitalSteer(TimeSingle time, bool pressed);
    public abstract TDigitalKeyframe GetDigitalSteerKeyframe(TDigitalBlock block, TimeSingle time);
    
    public abstract TDigitalBlock InitiateDigitalAccel(TimeSingle time, bool pressed);
    public abstract TDigitalKeyframe GetDigitalAccelKeyframe(TDigitalBlock block, TimeSingle time);
    public abstract TAnalogBlock InitiateAnalogAccel(TimeSingle time, float value);
    public abstract TAnalogKeyframe GetAnalogAccelKeyframe(TAnalogBlock block, TimeSingle time, float value);
    public abstract void ApplyAnalogAccel(TAnalogBlock block, TimeSingle time, float value);

    public abstract TDigitalBlock InitiateDigitalBrake(TimeSingle time, bool pressed);
    public abstract TDigitalKeyframe GetDigitalBrakeKeyframe(TDigitalBlock block, TimeSingle time);
    public abstract TAnalogBlock InitiateAnalogBrake(TimeSingle time, float value);
    public abstract TAnalogKeyframe GetAnalogBrakeKeyframe(TAnalogBlock block, TimeSingle time, float value);
    public abstract void ApplyAnalogBrake(TAnalogBlock block, TimeSingle time, float value);
    
    public abstract TDigitalBlock InitiateActionSlot(TimeSingle time, int index, bool pressed, bool activated);
    public abstract TDigitalKeyframe GetActionSlotKeyframe(TDigitalBlock block, TimeSingle time, int index);
    public virtual CGameCtnMediaBlock? InitiateActionSlotNum(TimeSingle timeStart, TimeInt32 timeEnd, int index) => null;

    public abstract TAnalogBlock InitiateAnalogSteer(TimeInt32 time, float value);
    public abstract TAnalogKeyframe GetAnalogSteerKeyframe(TAnalogBlock block, TimeInt32 time, float value);
    public abstract void ApplyAnalogSteer(TAnalogBlock block, TimeSingle time, float value);
    
    public abstract TDigitalBlock InitiateMouse(TimeSingle time, bool leftClick, bool rightClick, Vec2 curPos, Vec2 offset);
    public abstract TDigitalKeyframe GetMouseKeyframe(TDigitalBlock block, TimeSingle time, Vec2 curPos, Vec2 offset);
    public abstract void ApplyMouse(TDigitalBlock block, TimeSingle time, Vec2 curPos, Vec2 offset);
    
    public abstract CGameCtnMediaBlock InitiateJump(TimeSingle time, bool pressed);
    public abstract CGameCtnMediaBlock InitiateHorn(TimeSingle time, bool pressed);
    public abstract CGameCtnMediaBlock InitiateRespawn(TimeSingle time, bool pressed);
    public abstract CGameCtnMediaBlock InitiateSecondaryRespawn(TimeSingle time, bool pressed);

    CGameCtnMediaBlock IDesign.InitiateDigitalSteer(TimeSingle time, bool pressed) => InitiateDigitalSteer(time, pressed);
    CGameCtnMediaBlock.Key IDesign.GetDigitalSteerKeyframe(CGameCtnMediaBlock block, TimeSingle time) => GetDigitalSteerKeyframe((TDigitalBlock)block, time);
    
    CGameCtnMediaBlock IDesign.InitiateDigitalAccel(TimeSingle time, bool pressed) => InitiateDigitalAccel(time, pressed);
    CGameCtnMediaBlock.Key IDesign.GetDigitalAccelKeyframe(CGameCtnMediaBlock block, TimeSingle time) => GetDigitalAccelKeyframe((TDigitalBlock)block, time);
    CGameCtnMediaBlock IDesign.InitiateAnalogAccel(TimeSingle time, float value) => InitiateAnalogAccel(time, value);
    CGameCtnMediaBlock.Key IDesign.GetAnalogAccelKeyframe(CGameCtnMediaBlock block, TimeSingle time, float value) => GetAnalogAccelKeyframe((TAnalogBlock)block, time, value);
    void IDesign.ApplyAnalogAccel(CGameCtnMediaBlock block, TimeInt32 time, float value) => ApplyAnalogAccel((TAnalogBlock)block, time, value);

    CGameCtnMediaBlock IDesign.InitiateDigitalBrake(TimeSingle time, bool pressed) => InitiateDigitalBrake(time, pressed);
    CGameCtnMediaBlock.Key IDesign.GetDigitalBrakeKeyframe(CGameCtnMediaBlock block, TimeSingle time) => GetDigitalBrakeKeyframe((TDigitalBlock)block, time);
    CGameCtnMediaBlock IDesign.InitiateAnalogBrake(TimeSingle time, float value) => InitiateAnalogBrake(time, value);
    CGameCtnMediaBlock.Key IDesign.GetAnalogBrakeKeyframe(CGameCtnMediaBlock block, TimeSingle time, float value) => GetAnalogBrakeKeyframe((TAnalogBlock)block, time, value);
    void IDesign.ApplyAnalogBrake(CGameCtnMediaBlock block, TimeInt32 time, float value) => ApplyAnalogBrake((TAnalogBlock)block, time, value);
    
    CGameCtnMediaBlock IDesign.InitiateAnalogSteer(TimeInt32 time, float value) => InitiateAnalogSteer(time, value);
    CGameCtnMediaBlock.Key IDesign.GetAnalogSteerKeyframe(CGameCtnMediaBlock block, TimeInt32 time, float value) => GetAnalogSteerKeyframe((TAnalogBlock)block, time, value);
    void IDesign.ApplyAnalogSteer(CGameCtnMediaBlock block, TimeInt32 time, float value) => ApplyAnalogSteer((TAnalogBlock)block, time, value);

    CGameCtnMediaBlock IDesign.InitiateActionSlot(TimeSingle time, int index, bool pressed, bool activated) => InitiateActionSlot(time, index, pressed, activated);
    CGameCtnMediaBlock.Key IDesign.GetActionSlotKeyframe(CGameCtnMediaBlock block, TimeSingle time, int index) => GetActionSlotKeyframe((TDigitalBlock)block, time, index);

    CGameCtnMediaBlock IDesign.InitiateMouse(TimeSingle time, bool leftClick, bool rightClick, Vec2 curPos, Vec2 offset) => InitiateMouse(time, leftClick, rightClick, curPos, offset);
    CGameCtnMediaBlock.Key IDesign.GetMouseKeyframe(CGameCtnMediaBlock block, TimeSingle time, Vec2 curPos, Vec2 offset) => GetMouseKeyframe((TDigitalBlock)block, time, curPos, offset);
    void IDesign.ApplyMouse(CGameCtnMediaBlock block, TimeSingle time, Vec2 curPos, Vec2 offset) => ApplyMouse((TDigitalBlock)block, time, curPos, offset);

    public virtual CGameCtnMediaBlock? InitiateAnalogSteerBack(TimeSingle time) => null;
    public virtual CGameCtnMediaBlock? InitiateAnalogAccelBack(TimeSingle time) => null;
    public virtual CGameCtnMediaBlock? InitiateAnalogBrakeBack(TimeSingle time) => null;

    public void SetSteer(bool? left)
    {
        this.left = left;
    }

    public bool? IsLeftSteer()
    {
        return left;
    }

    /// <summary>
    /// Multiplier for the steer value.
    /// </summary>
    /// <returns>1 if left, -1 if right.</returns>
    /// <exception cref="NotSupportedException"></exception>
    public int GetSteerMultiplier()
    {
        if (IsLeftSteer() is not bool left)
        {
            throw new NotSupportedException("Not supported outside steering");
        }

        return left ? 1 : -1; // MEDIATRACKER COORDS, NOT INPUT VALUES
    }

    protected FileRef GetFileRefImagePath(string relativePath, string? locatorUrl = null)
    {
        var extension = IsManiaPlanet ? ".webp" : ".png";

        return new FileRef(2, checksum: null, (IsManiaPlanet ? "Media\\Images\\" : "MediaTracker\\Images\\") + relativePath + extension, locatorUrl + extension);
    }
}
