using ClipInput.Skins;
using GBX.NET;
using GBX.NET.Engines.Control;
using GBX.NET.Engines.Game;
using GbxToolAPI;
using TmEssentials;

namespace ClipInput.Designs;

public class TextDesign : Design<TextDesignSkin, CGameCtnMediaBlockText, CControlEffectSimi.Key, CGameCtnMediaBlockText, CControlEffectSimi.Key>
{
    public TextDesign(ClipInputConfig config) : base(config)
    {

    }

    private CGameCtnMediaBlockText CreateDigitalTextBlock(bool pressed, Vec4 activeColor)
    {
        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        var color = pressed
            ? new Vec3(activeColor.X, activeColor.Y, activeColor.Z)
            : new Vec3(Config.InactiveColor.X, Config.InactiveColor.Y, Config.InactiveColor.Z);

        return CGameCtnMediaBlockText.Create(effect)
            .WithColor(color)
            .WithText("⏹")
            .ForTMUF()
            .Build();
    }

    public override CGameCtnMediaBlockText InitiateAnalogAccel(TimeSingle time, float value)
    {
        var block = CreateDigitalTextBlock(pressed: true, Config.ActiveColor);

        var key = GetAnalogAccelKeyframe(block, time, value);

        block.Effect.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockText InitiateAnalogBrake(TimeSingle time, float value)
    {
        var block = CreateDigitalTextBlock(pressed: true, Config.ActiveColor);

        var key = GetAnalogBrakeKeyframe(block, time, value);

        block.Effect.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockText InitiateAnalogSteer(TimeInt32 time, float value)
    {
        var effect = CControlEffectSimi.Create()
           .Centered()
           .ForTMUF()
           .Build();

        var color = new Vec3(Config.ActiveColor.X, Config.ActiveColor.Y, Config.ActiveColor.Z);

        var block = CGameCtnMediaBlockText.Create(effect)
            .WithColor(color)
            .WithText("►")
            .ForTMUF()
            .Build();

        var key = GetAnalogSteerKeyframe(block, time, value);

        block.Effect.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockText InitiateDigitalAccel(TimeSingle time, bool pressed)
    {
        var block = CreateDigitalTextBlock(pressed, Config.ActiveColor);

        var key = GetDigitalAccelKeyframe(block, time);

        block.Effect.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockText InitiateDigitalBrake(TimeSingle time, bool pressed)
    {
        var block = CreateDigitalTextBlock(pressed, Config.BrakeColor);

        var key = GetDigitalBrakeKeyframe(block, time);

        block.Effect.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockText InitiateDigitalSteer(TimeSingle time, bool pressed)
    {
        var block = CreateDigitalTextBlock(pressed, Config.ActiveColor);

        var key = GetDigitalSteerKeyframe(block, time);

        block.Effect.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockText InitiateActionSlot(TimeSingle time, int index, bool pressed, bool activated)
    {
        throw new NotImplementedException();
    }

    public override void ApplyAnalogAccel(CGameCtnMediaBlockText block, TimeSingle time, float value)
    {
        var key = GetAnalogAccelKeyframe(block, time, value);

        block.Effect.Keys.Add(key);
    }

    public override void ApplyAnalogBrake(CGameCtnMediaBlockText block, TimeSingle time, float value)
    {
        var key = GetAnalogBrakeKeyframe(block, time, value);

        block.Effect.Keys.Add(key);
    }

    public override CControlEffectSimi.Key GetActionSlotKeyframe(CGameCtnMediaBlockText block, TimeSingle time, int index)
    {
        throw new NotImplementedException();
    }

    public override CControlEffectSimi.Key GetAnalogAccelKeyframe(CGameCtnMediaBlockText block, TimeSingle time, float value)
    {
        var aspectRatio = Config.GetAspectRatio();

        var y = Config.AnalogAccelPos.Y - 0.01f + (1 - value) * 0.02f + Config.AnalogAccelSize.Y * 0.5f * value - Config.AnalogAccelSize.Y * 0.5f;

        return new CControlEffectSimi.Key()
        {
            Time = time,
            Position = new Vec2(Config.AnalogAccelPos.X * aspectRatio, y) * Config.PrimaryScale + Config.PrimaryPos,
            Scale = new Vec2(Config.AnalogAccelSize.X * aspectRatio * 20f, Config.AnalogAccelSize.Y * 15.5f * value) * Config.PrimaryScale,
        };
    }

    public override CControlEffectSimi.Key GetAnalogBrakeKeyframe(CGameCtnMediaBlockText block, TimeSingle time, float value)
    {
        var aspectRatio = Config.GetAspectRatio();

        var y = Config.AnalogBrakePos.Y - 0.01f + (1 - value) * 0.02f - Config.AnalogAccelSize.Y * 0.5f * value + Config.AnalogAccelSize.Y * 0.5f;

        return new CControlEffectSimi.Key()
        {
            Time = time,
            Position = new Vec2(Config.AnalogBrakePos.X * aspectRatio, y) * Config.PrimaryScale + Config.PrimaryPos,
            Scale = new Vec2(Config.AnalogBrakeSize.X * aspectRatio * 20f, Config.AnalogBrakeSize.Y * 15.5f * value) * Config.PrimaryScale,
        };
    }

    public override CControlEffectSimi.Key GetAnalogSteerKeyframe(CGameCtnMediaBlockText block, TimeInt32 time, float value)
    {
        var aspectRatio = Config.GetAspectRatio();

        var y = (Config.AnalogSteerPos.Y - 0.025f) * Config.PrimaryScale.Y + Config.PrimaryPos.Y;
        var width = Config.AnalogSteerSize.X;
        var height = Config.AnalogSteerSize.Y;
        var leftSideX = Config.AnalogSteerPos.X + Config.AnalogSteerSpacing.X + width * 0.5f * value;
        var x = leftSideX * GetSteerMultiplier() * aspectRatio * Config.PrimaryScale.X + Config.PrimaryPos.X;

        return new CControlEffectSimi.Key()
        {
            Time = time,
            Position = new Vec2(x, y),
            Scale = new Vec2(value * -GetSteerMultiplier() * width * aspectRatio * 20f, height * 15.5f) * Config.PrimaryScale,
        };
    }

    public override CControlEffectSimi.Key GetDigitalAccelKeyframe(CGameCtnMediaBlockText block, TimeSingle time)
    {
        var x = Config.DigitalAccelPos.X;
        var y = Config.DigitalAccelPos.Y;
        var width = Config.DigitalAccelSize.X;
        var height = Config.DigitalAccelSize.Y;

        return GetDigitalGasKeyframe(time, x, y, width, height);
    }

    public override CControlEffectSimi.Key GetDigitalBrakeKeyframe(CGameCtnMediaBlockText block, TimeSingle time)
    {
        var x = Config.DigitalBrakePos.X;
        var y = Config.DigitalBrakePos.Y;
        var width = Config.DigitalBrakeSize.X;
        var height = Config.DigitalBrakeSize.Y;

        return GetDigitalGasKeyframe(time, x, y, width, height);
    }

    private CControlEffectSimi.Key GetDigitalGasKeyframe(TimeSingle time, float x, float y, float width, float height)
    {
        var aspectRatio = Config.GetAspectRatio();

        return new CControlEffectSimi.Key()
        {
            Time = time,
            Position = new Vec2(x * aspectRatio, y - 0.01f) * Config.PrimaryScale + Config.PrimaryPos,
            Scale = new Vec2(width * aspectRatio * 20f, height * 15.5f) * Config.PrimaryScale,
        };
    }

    public override CControlEffectSimi.Key GetDigitalSteerKeyframe(CGameCtnMediaBlockText block, TimeSingle time)
    {
        var aspectRatio = Config.GetAspectRatio();

        var y = Config.DigitalSteerPos.Y - 0.01f;
        var width = Config.DigitalSteerSize.X;
        var height = Config.DigitalSteerSize.Y;
        var leftSideX = Config.DigitalSteerPos.X + Config.DigitalSteerSpacing.X + width * 0.5f;
        var x = leftSideX * GetSteerMultiplier() * aspectRatio;

        return new CControlEffectSimi.Key()
        {
            Time = time,
            Position = new Vec2(x, y + height * 0.5f) * Config.PrimaryScale + Config.PrimaryPos,
            Scale = new Vec2(width * aspectRatio * 20f, height * 15.5f) * Config.PrimaryScale,
        };
    }

    public override void ApplyAnalogSteer(CGameCtnMediaBlockText block, TimeSingle time, float value)
    {
        block.Effect.Keys.Add(GetAnalogSteerKeyframe(block, time, value));
    }

    public override CGameCtnMediaBlock? InitiateAnalogSteerBack(TimeSingle time)
    {
        var effect = CControlEffectSimi.Create()
           .Centered()
           .ForTMUF()
           .Build();

        var color = new Vec3(Config.InactiveColor.X, Config.InactiveColor.Y, Config.InactiveColor.Z);

        var block = CGameCtnMediaBlockText.Create(effect)
            .WithColor(color)
            .WithText("►")
            .ForTMUF()
            .Build();

        var key = GetAnalogSteerBackKeyframe(time);

        effect.Keys.Add(key);

        return block;
    }

    private CControlEffectSimi.Key GetAnalogSteerBackKeyframe(TimeInt32 time)
    {
        var aspectRatio = Config.GetAspectRatio();

        var scale = Config.PrimaryScale;
        var globalPos = Config.PrimaryPos;
        var x = (Config.AnalogSteerPos.X + Config.AnalogSteerSpacing.X + Config.AnalogSteerSize.X * 0.5f) * GetSteerMultiplier() * aspectRatio * scale.X + globalPos.X;
        var y = (Config.AnalogSteerPos.Y - 0.025f) * scale.Y + globalPos.Y;
        var scaleX = Config.AnalogSteerSize.X * 20f * scale.X * aspectRatio * -GetSteerMultiplier();
        var scaleY = Config.AnalogSteerSize.Y * 15.5f * scale.Y;

        return new CControlEffectSimi.Key
        {
            Time = time.ToTimeSingle() + Config.StartOffset,
            Position = new Vec2(x, y),
            Scale = new Vec2(scaleX, scaleY) * 0.99f,
            Depth = 0.6f
        };
    }

    private CGameCtnMediaBlockText CreateAnalogGasBack()
    {
        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        return CGameCtnMediaBlockText.Create(effect)
            .WithColor(new Vec3(Config.InactiveColor.X, Config.InactiveColor.Y, Config.InactiveColor.Z))
            .WithText("⏹")
            .ForTMUF()
            .Build();
    }

    public override CGameCtnMediaBlock? InitiateAnalogAccelBack(TimeSingle time)
    {
        var block = CreateAnalogGasBack();

        var key = GetAnalogAccelBackKeyframe(time);

        block.Effect.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockText? InitiateAnalogBrakeBack(TimeSingle time)
    {
        var block = CreateAnalogGasBack();

        var key = GetAnalogBrakeBackKeyframe(time);

        block.Effect.Keys.Add(key);

        return block;
    }

    private CControlEffectSimi.Key GetAnalogAccelBackKeyframe(TimeSingle time)
    {
        var x = Config.DigitalAccelPos.X;
        var y = Config.DigitalAccelPos.Y;
        var width = Config.DigitalAccelSize.X;
        var height = Config.DigitalAccelSize.Y;

        return GetAnalogGasBackframe(time, x, y, width, height);
    }

    private CControlEffectSimi.Key GetAnalogBrakeBackKeyframe(TimeSingle time)
    {
        var x = Config.DigitalBrakePos.X;
        var y = Config.DigitalBrakePos.Y;
        var width = Config.DigitalBrakeSize.X;
        var height = Config.DigitalBrakeSize.Y;

        return GetAnalogGasBackframe(time, x, y, width, height);
    }

    private CControlEffectSimi.Key GetAnalogGasBackframe(TimeSingle time, float x, float y, float width, float height)
    {
        var aspectRatio = Config.GetAspectRatio();

        return new CControlEffectSimi.Key()
        {
            Time = time,
            Position = new Vec2(x * aspectRatio, y - 0.01f) * Config.PrimaryScale + Config.PrimaryPos,
            Scale = new Vec2(width * aspectRatio * 20f, height * 15.5f) * Config.PrimaryScale,
            Depth = 0.6f
        };
    }

    public override CGameCtnMediaBlockText InitiateMouse(TimeSingle time, bool leftClick, bool rightClick, Vec2 curPos, Vec2 offset)
    {
        throw new NotImplementedException();
    }

    public override CControlEffectSimi.Key GetMouseKeyframe(CGameCtnMediaBlockText block, TimeSingle time, Vec2 curPos, Vec2 offset)
    {
        throw new NotImplementedException();
    }

    public override void ApplyMouse(CGameCtnMediaBlockText block, TimeSingle time, Vec2 curPos, Vec2 offset)
    {
        throw new NotImplementedException();
    }

    private CGameCtnMediaBlock InitiateGenericKey(string text, TimeSingle time, bool pressed, Vec2 position, Vec2 scale)
    {
        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        var color = pressed
            ? new Vec3(Config.ActiveColor.X, Config.ActiveColor.Y, Config.ActiveColor.Z)
            : new Vec3(Config.InactiveColor.X, Config.InactiveColor.Y, Config.InactiveColor.Z);

        var block = CGameCtnMediaBlockText.Create(effect)
            .WithColor(color)
            .WithText($"$n$o{text}")
            .ForTMUF()
            .Build();

        block.Effect.Keys.Add(new CControlEffectSimi.Key
        {
            Time = time + Config.StartOffset,
            Depth = 0,
            Scale = scale,
            Position = position
        });

        return block;
    }

    public override CGameCtnMediaBlock InitiateJump(TimeSingle time, bool pressed)
    {
        return InitiateGenericKey("JUMP", time, pressed, Config.JumpPos, Config.JumpScale);
    }

    public override CGameCtnMediaBlock InitiateHorn(TimeSingle time, bool pressed)
    {
        return InitiateGenericKey("HORN", time, pressed, Config.HornPos, Config.HornScale);
    }

    public override CGameCtnMediaBlock InitiateRespawn(TimeSingle time, bool pressed)
    {
        return InitiateGenericKey("RESPAWN", time, pressed, Config.RespawnPos, Config.RespawnScale);
    }

    public override CGameCtnMediaBlock InitiateSecondaryRespawn(TimeSingle time, bool pressed)
    {
        return InitiateGenericKey("SECONDARY RESPAWN", time, pressed, Config.SecondaryRespawnPos, Config.SecondaryRespawnScale);
    }
}
