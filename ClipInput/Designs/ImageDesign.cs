using ClipInput.Skins;
using GBX.NET;
using GBX.NET.Engines.Control;
using GBX.NET.Engines.Game;
using System;
using TmEssentials;

namespace ClipInput.Designs;

public class ImageDesign(ClipInputConfig config) : Design<ImageDesignSkin, CGameCtnMediaBlockImage, CControlEffectSimi.Key, CGameCtnMediaBlockTriangles, CGameCtnMediaBlockTriangles.Key>(config)
{
    private readonly ClipInputConfig config = config;

    private static readonly Int3[] gasTriangles =
    [
        new Int3(0, 1, 4),
        new Int3(1, 4, 5),
        new Int3(1, 2, 5),
        new Int3(2, 5, 6),
        new Int3(2, 3, 6),
        new Int3(3, 6, 7)
    ];

    private static readonly Int3[] padTriangles =
    [
        new Int3(0, 1, 2),
        new Int3(1, 2, 3),

        new Int3(2, 3, 4),
        new Int3(3, 4, 5),

        new Int3(4, 5, 6),
    ];

    private readonly Vec4[] accelerateRealVertexColors =
    [
        config.InactiveColor,
        config.InactiveColor,
        config.ActiveColor,
        config.ActiveColor,
        config.InactiveColor,
        config.InactiveColor,
        config.ActiveColor,
        config.ActiveColor
    ];
    private readonly Vec4[] brakeRealVertexColors =
    [
        config.BrakeColor,
        config.BrakeColor,
        config.InactiveColor,
        config.InactiveColor,
        config.BrakeColor,
        config.BrakeColor,
        config.InactiveColor,
        config.InactiveColor
    ];
    private readonly Vec4[] padVertexColors =
    [
        config.ActiveColor,
        config.ActiveColor,
        config.ActiveColor,
        config.ActiveColor,
        config.InactiveColor,
        config.InactiveColor,
        config.InactiveColor
    ];

    public override void ApplyAnalogAccel(CGameCtnMediaBlockTriangles block, TimeSingle time, float value)
    {
        block.Keys.Add(GetAnalogAccelKeyframe(block, time, value));
    }

    public override void ApplyAnalogBrake(CGameCtnMediaBlockTriangles block, TimeSingle time, float value)
    {
        block.Keys.Add(GetAnalogBrakeKeyframe(block, time, value));
    }
    
    public override CGameCtnMediaBlockImage InitiateDigitalSteer(TimeSingle time, bool pressed)
    {
        var left = IsLeftSteer() ?? throw new Exception("Analog steer track build problem");

        var image = GetClipInputFileRef(left ? "Left" : "Right", pressed);

        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        var block = CGameCtnMediaBlockImage.Create(effect, image);

        var key = GetDigitalSteerKeyframe(block, time);

        block.Effect.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockImage InitiateDigitalAccel(TimeSingle time, bool pressed)
    {
        var image = GetClipInputFileRef("Up", pressed);

        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        var block = CGameCtnMediaBlockImage.Create(effect, image);

        var key = GetDigitalAccelKeyframe(block, time);

        block.Effect.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockTriangles InitiateAnalogAccel(TimeSingle time, float value)
    {
        var block = CGameCtnMediaBlockTriangles2D.Create(accelerateRealVertexColors)
            .WithTriangles(gasTriangles)
            .ForTMUF()
            .Build();

        var key = GetAnalogAccelKeyframe(block, time, value);

        block.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockImage InitiateDigitalBrake(TimeSingle time, bool pressed)
    {
        var image = GetClipInputFileRef("Down", pressed);

        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        var block = CGameCtnMediaBlockImage.Create(effect, image);

        var key = GetDigitalBrakeKeyframe(block, time);

        block.Effect.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockTriangles InitiateAnalogBrake(TimeSingle time, float value)
    {
        var block = CGameCtnMediaBlockTriangles2D.Create(brakeRealVertexColors)
            .WithTriangles(gasTriangles)
            .ForTMUF()
            .Build();

        var key = GetAnalogBrakeKeyframe(block, time, value);

        block.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockImage InitiateActionSlot(TimeSingle time, int index, bool pressed, bool activated)
    {
        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        var num = index + 1 % 10;

        var block = CGameCtnMediaBlockImage.Create(effect, GetClipInputFileRef(num.ToString(), pressed, activated));

        var key = GetActionSlotKeyframe(block, time, index);

        block.Effect.Keys.Add(key);

        return block;
    }

    public static Vec2 GetDigitalSteerPos(ClipInputConfig config, float steerMultiplier)
    {
        var leftSideX = config.DigitalSteerPos.X + config.DigitalSteerSpacing.X + config.DigitalSteerSize.X * 0.5f;
        var x = leftSideX * steerMultiplier * config.GetAspectRatio();
        var y = config.DigitalSteerPos.Y + config.DigitalSteerSize.Y * 0.5f;

        return (x, y) * config.PrimaryScale + config.PrimaryPos;
    }

    public static Vec2 GetDigitalSteerScale(ClipInputConfig config)
    {
        var width = config.DigitalSteerSize.X * 2 * config.GetAspectRatio();
        var height = config.DigitalSteerSize.Y * 2;

        return (width, height) * config.PrimaryScale;
    }

    public override CControlEffectSimi.Key GetDigitalSteerKeyframe(CGameCtnMediaBlockImage block, TimeSingle time)
    {
        return new CControlEffectSimi.Key()
        {
            Time = time,
            Position = GetDigitalSteerPos(Config, GetSteerMultiplier()),
            Scale = GetDigitalSteerScale(Config),
        };
    }

    public static Vec2 GetDigitalAccelPos(ClipInputConfig config)
    {
        return config.DigitalAccelPos * config.PrimaryScale + config.PrimaryPos;
    }

    public static Vec2 GetDigitalAccelScale(ClipInputConfig config)
    {
        return config.DigitalAccelSize * 2 * (config.GetAspectRatio(), 1) * config.PrimaryScale;
    }

    public static Vec2 GetDigitalBrakePos(ClipInputConfig config)
    {
        return config.DigitalBrakePos * config.PrimaryScale + config.PrimaryPos;
    }

    public static Vec2 GetDigitalBrakeScale(ClipInputConfig config)
    {
        return config.DigitalBrakeSize * 2 * (config.GetAspectRatio(), 1) * config.PrimaryScale;
    }

    public override CControlEffectSimi.Key GetDigitalAccelKeyframe(CGameCtnMediaBlockImage block, TimeSingle time)
    {
        return new CControlEffectSimi.Key()
        {
            Time = time,
            Position = GetDigitalAccelPos(Config),
            Scale = GetDigitalAccelScale(Config)
        };
    }

    public override CGameCtnMediaBlockTriangles.Key GetAnalogAccelKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time, float value)
    {
        var aspectRatio = config.GetAspectRatio();

        var x = config.AnalogAccelPos.X + Skin.AnalogAccelOffset.X;
        var y = config.AnalogAccelPos.Y + Skin.AnalogAccelOffset.Y;
        var width = config.AnalogAccelSize.X * 0.8f;
        var height = config.AnalogAccelSize.Y * 0.8f;

        var firstX = (x - width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var firstY = (y + height / 2) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        var secondY = (y - height / 2 + height * value) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        var secondX = (x + width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var thirdY = (y - height / 2) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        return new CGameCtnMediaBlockTriangles.Key(block)
        {
            Time = time,
            Positions =
            [
                new Vec3(firstX, firstY, 0),
                new Vec3(firstX, secondY, 0),
                new Vec3(firstX, secondY, 0),
                new Vec3(firstX, thirdY, 0),
                new Vec3(secondX, firstY, 0),
                new Vec3(secondX, secondY, 0),
                new Vec3(secondX, secondY, 0),
                new Vec3(secondX, thirdY, 0)
            ]
        };
    }


    public override CControlEffectSimi.Key GetDigitalBrakeKeyframe(CGameCtnMediaBlockImage block, TimeSingle time)
    {
        return new CControlEffectSimi.Key()
        {
            Time = time,
            Position = GetDigitalBrakePos(Config),
            Scale = GetDigitalBrakeScale(Config)
        };
    }

    public override CGameCtnMediaBlockTriangles.Key GetAnalogBrakeKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time, float value)
    {
        var aspectRatio = config.GetAspectRatio();

        var x = config.AnalogBrakePos.X + Skin.AnalogBrakeOffset.X;
        var y = config.AnalogBrakePos.Y + Skin.AnalogBrakeOffset.Y;
        var width = config.AnalogBrakeSize.X * 0.8f;
        var height = config.AnalogBrakeSize.Y * 0.8f;

        var firstX = (x - width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var firstY = (y + height / 2) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        var secondY = (y + height / 2 - height * value) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        var secondX = (x + width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var thirdY = (y - height / 2) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        return new CGameCtnMediaBlockTriangles.Key(block)
        {
            Time = time,
            Positions =
            [
                new Vec3(firstX, firstY, 0),
                new Vec3(firstX, secondY, 0),
                new Vec3(firstX, secondY, 0),
                new Vec3(firstX, thirdY, 0),
                new Vec3(secondX, firstY, 0),
                new Vec3(secondX, secondY, 0),
                new Vec3(secondX, secondY, 0),
                new Vec3(secondX, thirdY, 0)
            ]
        };
    }

    public static Vec2 GetActionSlotPos(ClipInputConfig config, int index)
    {
        var xPercentage = (index % ActionSlotCountPerRow / (float)(ActionSlotCountPerRow - 1) - 0.5f) * 2;
        var yPercentage = index / ActionSlotCountPerRow / (float)(ActionSlotCountPerRow - 1) * 2;

        var x = -xPercentage * config.ActionKeysScale.X;
        var y = -yPercentage * config.ActionKeysScale.Y + config.ActionKeysPos.Y;

        return new Vec2(x * config.GetAspectRatio() + config.ActionKeysPos.X, y);
    }

    public static Vec2 GetActionSlotScale(ClipInputConfig config)
    {
        var aspectRatio = config.GetAspectRatio();

        var width = config.ActionKeysScale.X * 2 / (ActionSlotCountPerRow - 1 + config.ActionKeysSpacing.X);
        var height = width;

        return new Vec2(width * aspectRatio, height) * 2;
    }

    public override CControlEffectSimi.Key GetActionSlotKeyframe(CGameCtnMediaBlockImage block, TimeSingle time, int index)
    {
        return new CControlEffectSimi.Key()
        {
            Time = time,
            Position = GetActionSlotPos(config, index),
            Scale = GetActionSlotScale(config)
        };
    }
    
    public override CGameCtnMediaBlockTriangles InitiateAnalogSteer(TimeInt32 time, float value)
    {
        var block = CGameCtnMediaBlockTriangles2D.Create(padVertexColors)
            .WithTriangles(padTriangles)
            .ForTMUF()
            .Build();

        var key = GetAnalogSteerKeyframe(block, time, value);

        block.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockTriangles.Key GetAnalogSteerKeyframe(CGameCtnMediaBlockTriangles node, TimeInt32 time, float value)
    {
        var aspectRatio = config.GetAspectRatio();

        var scale = config.PrimaryScale;
        var globalPos = config.PrimaryPos;
        var yLocal = config.AnalogSteerPos.Y;
        var width = config.AnalogSteerSize.X * .9f; //
        var height = config.AnalogSteerSize.Y * .9f; //

        var leftSideX = config.AnalogSteerPos.X + config.AnalogSteerSpacing.X + 0.016f; //

        var firstX = leftSideX * GetSteerMultiplier() * aspectRatio * scale.X + globalPos.X;
        var firstTopY = (yLocal + height * 0.5f) * scale.Y + globalPos.Y;
        var firstBottomY = (yLocal - height * 0.5f) * scale.Y + globalPos.Y;

        var secondX = (leftSideX + width * value) * GetSteerMultiplier() * aspectRatio * scale.X + globalPos.X;
        var secondTopY = (yLocal + height * 0.5f - height * 0.5f * value) * scale.Y + globalPos.Y;
        var secondBottomY = (yLocal - height * 0.5f + height * 0.5f * value) * scale.Y + globalPos.Y;

        var thirdX = (leftSideX + width) * GetSteerMultiplier() * aspectRatio * scale.X + globalPos.X;
        var thirdY = yLocal * scale.Y + globalPos.Y;

        return new CGameCtnMediaBlockTriangles.Key(node)
        {
            Time = time.ToTimeSingle() + config.StartOffset,
            Positions =
            [
                new Vec3(firstX, firstTopY, 0),
                new Vec3(firstX, firstBottomY, 0),
                new Vec3(secondX, secondTopY, 0),
                new Vec3(secondX, secondBottomY, 0),
                new Vec3(secondX, secondTopY, 0),
                new Vec3(secondX, secondBottomY, 0),
                new Vec3(thirdX, thirdY, 0),
            ]
        };
    }

    public override CGameCtnMediaBlock? InitiateAnalogSteerBack(TimeSingle time)
    {
        var left = IsLeftSteer() ?? throw new Exception("Analog steer track build problem");
        
        var image = GetClipInputFileRef("Pad" + (left ? "Left" : "Right"));
        
        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        var block = CGameCtnMediaBlockImage.Create(effect, image);

        var key = GetAnalogSteerBackKeyframe(time);

        effect.Keys.Add(key);

        return block;
    }

    public static Vec2 GetAnalogSteerBackPos(ClipInputConfig config, float steerMultiplier)
    {
        var aspectRatio = config.GetAspectRatio();
        var scale = config.PrimaryScale;
        var globalPos = config.PrimaryPos;

        var x = (config.AnalogSteerPos.X + config.AnalogSteerSpacing.X + config.AnalogSteerSize.X * 0.5f) * steerMultiplier * aspectRatio * scale.X + globalPos.X;
        var y = config.AnalogSteerPos.Y * scale.Y + globalPos.Y;

        return new(x, y);
    }

    public static Vec2 GetAnalogSteerBackScale(ClipInputConfig config)
    {
        var scaleX = config.AnalogSteerSize.X * 2 * config.PrimaryScale.X * config.GetAspectRatio();
        var scaleY = config.AnalogSteerSize.Y * 2 * config.PrimaryScale.Y;

        return new(scaleX, scaleY);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time">Actual time without an applied <see cref="ClipInputConfig.StartOffset"/>.</param>
    /// <param name="value"></param>
    /// <returns></returns>
    private CControlEffectSimi.Key GetAnalogSteerBackKeyframe(TimeInt32 time)
    {
        return new CControlEffectSimi.Key
        {
            Time = time.ToTimeSingle() + config.StartOffset,
            Position = GetAnalogSteerBackPos(config, GetSteerMultiplier()),
            Scale = GetAnalogSteerBackScale(config),
            Depth = 0.6f
        };
    }

    public override CGameCtnMediaBlock? InitiateAnalogAccelBack(TimeSingle time)
    {
        var image = GetClipInputFileRef("Analog");

        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        var block = CGameCtnMediaBlockImage.Create(effect, image);

        var key = GetAnalogAccelBackKeyframe(time);

        effect.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlock? InitiateAnalogBrakeBack(TimeSingle time)
    {
        var image = GetClipInputFileRef("Analog");

        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        var block = CGameCtnMediaBlockImage.Create(effect, image);

        var key = GetAnalogBrakeBackKeyframe(time);

        effect.Keys.Add(key);

        return block;
    }

    private CControlEffectSimi.Key GetAnalogAccelBackKeyframe(TimeInt32 time)
    {
        var aspectRatio = config.GetAspectRatio();
        
        var x = config.AnalogAccelPos.X * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var y = config.AnalogAccelPos.Y * config.PrimaryScale.Y + config.PrimaryPos.Y;
        var scaleX = config.AnalogAccelSize.X * 2 * config.PrimaryScale.X * aspectRatio;
        var scaleY = config.AnalogAccelSize.Y * 2 * config.PrimaryScale.Y;

        return new CControlEffectSimi.Key
        {
            Time = time.ToTimeSingle() + config.StartOffset,
            Position = new Vec2(x, y),
            Scale = new Vec2(scaleX, scaleY),
            Depth = 0.6f
        };
    }

    private CControlEffectSimi.Key GetAnalogBrakeBackKeyframe(TimeInt32 time)
    {
        var aspectRatio = config.GetAspectRatio();

        var x = config.AnalogBrakePos.X * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var y = config.AnalogBrakePos.Y * config.PrimaryScale.Y + config.PrimaryPos.Y;
        var scaleX = config.AnalogBrakeSize.X * 2 * config.PrimaryScale.X * aspectRatio;
        var scaleY = config.AnalogBrakeSize.Y * 2 * config.PrimaryScale.Y;

        return new CControlEffectSimi.Key
        {
            Time = time.ToTimeSingle() + config.StartOffset,
            Position = new Vec2(x, y),
            Scale = new Vec2(scaleX, scaleY),
            Depth = 0.6f
        };
    }

    public override void ApplyAnalogSteer(CGameCtnMediaBlockTriangles block, TimeSingle time, float value)
    {
        block.Keys.Add(GetAnalogSteerKeyframe(block, time, value));
    }

    public override CGameCtnMediaBlockImage InitiateMouse(TimeSingle time, bool leftClick, bool rightClick, Vec2 curPos, Vec2 offset)
    {
        FileRef image;

        if (leftClick && rightClick)
        {
            image = GetClipInputFileRef("Mouse_Both");
        }
        else if (leftClick)
        {
            image = GetClipInputFileRef("Mouse_Left");
        }
        else if (rightClick)
        {
            image = GetClipInputFileRef("Mouse_Right");
        }
        else
        {
            image = GetClipInputFileRef("Mouse");
        }

        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Build();

        var block = CGameCtnMediaBlockImage.Create(effect, image);

        ApplyMouse(block, time, curPos, offset);

        return block;
    }

    public static Vec2 GetMousePos(ClipInputConfig config, Vec2 curPos, Vec2 offset)
    {
        var x = Math.Clamp(curPos.X - offset.X, -2000, 2000);
        var y = Math.Clamp(curPos.Y - offset.Y, -1000, 1000);

        return new Vec2(x, y) * 0.0002f * (config.GetAspectRatio(), 1) * config.MouseScale + config.MousePos;
    }

    public static Vec2 GetMouseScale(ClipInputConfig config)
    {
        return config.MouseScale * (config.GetAspectRatio(), 1) * 1.8f;
    }

    public override CControlEffectSimi.Key GetMouseKeyframe(CGameCtnMediaBlockImage block, TimeSingle time, Vec2 curPos, Vec2 offset)
    {
        return new CControlEffectSimi.Key()
        {
            Time = time + config.StartOffset,
            Depth = 0,
            Scale = GetMouseScale(Config),
            Position = GetMousePos(Config, curPos, offset)
        };
    }

    public override void ApplyMouse(CGameCtnMediaBlockImage block, TimeSingle time, Vec2 curPos, Vec2 offset)
    {
        block.Effect.Keys.Add(GetMouseKeyframe(block, time, curPos, offset));
    }
    
    private CGameCtnMediaBlockImage InitiateGenericKey(string imageName, TimeSingle time, bool pressed, Vec2 position, Vec2 scale)
    {
        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Interpolated()
            .Build();

        var block = CGameCtnMediaBlockImage.Create(effect, GetClipInputFileRef(imageName, pressed));

        block.Effect.Keys.Add(new CControlEffectSimi.Key
        {
            Time = time + config.StartOffset,
            Depth = 0,
            Scale = scale,
            Position = position
        });

        return block;
    }

    public static Vec2 GetJumpPos(ClipInputConfig config) => config.JumpPos;
    public static Vec2 GetJumpScale(ClipInputConfig config) => config.JumpScale * (config.GetAspectRatio(), 1);

    public override CGameCtnMediaBlock InitiateJump(TimeSingle time, bool pressed)
    {
        return InitiateGenericKey("Jump", time, pressed, GetJumpPos(config), GetJumpScale(config));
    }

    public static Vec2 GetHornPos(ClipInputConfig config) => config.HornPos;
    public static Vec2 GetHornScale(ClipInputConfig config) => config.HornScale * (config.GetAspectRatio(), 1);

    public override CGameCtnMediaBlock InitiateHorn(TimeSingle time, bool pressed)
    {
        return InitiateGenericKey("Horn", time, pressed, GetHornPos(Config), GetHornScale(Config));
    }
    
    public static Vec2 GetRespawnPos(ClipInputConfig config) => config.RespawnPos;
    public static Vec2 GetRespawnScale(ClipInputConfig config) => config.RespawnScale * (config.GetAspectRatio(), 1);

    public override CGameCtnMediaBlock InitiateRespawn(TimeSingle time, bool pressed)
    {
        return InitiateGenericKey("Respawn", time, pressed, GetRespawnPos(Config), GetRespawnScale(Config));
    }

    public static Vec2 GetSecondaryRespawnPos(ClipInputConfig config) => config.SecondaryRespawnPos;
    public static Vec2 GetSecondaryRespawnScale(ClipInputConfig config) => config.SecondaryRespawnScale * (config.GetAspectRatio(), 1) * 1.5f;

    public override CGameCtnMediaBlock InitiateSecondaryRespawn(TimeSingle time, bool pressed)
    {
        return InitiateGenericKey("SecondaryRespawn", time, pressed, GetSecondaryRespawnPos(Config), GetSecondaryRespawnScale(Config));
    }

    private readonly Dictionary<(string, bool, bool), FileRef> imageCache = [];

    private FileRef GetClipInputFileRef(string imageName, bool pressed = false, bool activated = false)
    {
        if (imageCache.TryGetValue((imageName, pressed, activated), out var value))
        {
            return value;
        }

        var imgName = imageName + (pressed || activated ? $"_On{(pressed ? "" : "2")}" : "");

        value = GetFileRefImagePath(@$"{Constants.MediaTrackerSkinsFolder}\{Skin.Name}\{imgName}",
            $"https://gbx.bigbang1112.cz/assets/tools/clip-input/images/{Skin.Name.ToLowerInvariant()}/{imgName}");

        imageCache.Add((imageName, pressed, activated), value);

        return value;
    }
}
