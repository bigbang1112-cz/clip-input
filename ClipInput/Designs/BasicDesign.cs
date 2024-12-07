using ClipInput.Skins;
using GBX.NET;
using GBX.NET.Engines.Control;
using GBX.NET.Engines.Game;
using TmEssentials;

namespace ClipInput.Designs;

public class BasicDesign : Design<BasicDesignSkin, CGameCtnMediaBlockTriangles, CGameCtnMediaBlockTriangles.Key, CGameCtnMediaBlockTriangles, CGameCtnMediaBlockTriangles.Key>
{
    private readonly ClipInputConfig config;
    
    private static readonly Int3[] keyTriangles =
    [
        new Int3(0, 1, 2),
        new Int3(1, 2, 3)
    ];

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

    private static readonly Int3[] mouseTriangles =
    [
        (0, 5, 7),
        (5, 6, 7),
        (0, 1, 7),
        (1, 7, 8),
        (9, 10, 11),
        (9, 13, 11),
        (11, 13, 14),
        (11, 12, 14),
        (2, 3, 16),
        (2, 15, 16),
        (3, 4, 17),
        (3, 16, 17),
        (18, 19, 20),
        (19, 20, 21)
    ];

    private static readonly Vec3[] mousePositions =
    [
        new Vec3(-0.18f, 0.4f, 0),
        new Vec3(-0.26f, 0.12f, 0),
        new Vec3(-0.26f, 0.104f, 0),
        new Vec3(-0.26f, -0.26f, 0),
        new Vec3(-0.18f, -0.4f, 0),

        new Vec3(-0.008f, 0.4f, 0),
        new Vec3(-0.008f, 0.28f, 0),
        new Vec3(-0.04f, 0.28f, 0),
        new Vec3(-0.04f, 0.12f, 0),

        new Vec3(0.01f, 0.4f, 0),
        new Vec3(0.01f, 0.28f, 0),
        new Vec3(0.04f, 0.28f, 0),
        new Vec3(0.04f, 0.12f, 0),

        new Vec3(0.18f, 0.4f, 0),
        new Vec3(0.26f, 0.12f, 0),
        new Vec3(0.26f, 0.104f, 0),
        new Vec3(0.26f, -0.26f, 0),
        new Vec3(0.18f, -0.4f, 0),

        new Vec3(0.026f, 0.264f, 0),
        new Vec3(0.026f, 0.12f, 0),
        new Vec3(-0.026f, 0.264f, 0),
        new Vec3(-0.026f, 0.12f, 0)
    ];

    private readonly Vec4[] keyVertexColorsOff;

    private readonly Vec4[] keyAccelVertexColorsOn;
    private readonly Vec4[] keyBrakeVertexColorsOn;
    private readonly Vec4[] accelerateRealVertexColors;
    private readonly Vec4[] brakeRealVertexColors;
    private readonly Vec4[] padVertexColors;

    private readonly Vec4[] mouseNoClickVertexColors;
    private readonly Vec4[] mouseLeftClickVertexColors;
    private readonly Vec4[] mouseRightClickVertexColors;
    private readonly Vec4[] mouseBothClickVertexColors;

    public BasicDesign(ClipInputConfig config) : base(config)
    {
        this.config = config;

        var accelColor = Skin.AccelColor ?? config.ActiveColor;
        var brakeColor = Skin.BrakeColor ?? config.BrakeColor;
        var steerColor = Skin.SteerColor ?? config.ActiveColor;
        var mouseLeftClickColor = Skin.MouseLeftClickColor ?? config.ActiveColor;
        var mouseRightClickColor = Skin.MouseRightClickColor ?? config.ActiveColor;
        var inactiveColor = Skin.InactiveColor ?? config.InactiveColor;

        keyAccelVertexColorsOn = [accelColor, accelColor, accelColor, accelColor];
        keyBrakeVertexColorsOn = [brakeColor, brakeColor, brakeColor, brakeColor];
        keyVertexColorsOff = [inactiveColor, inactiveColor, inactiveColor, inactiveColor];

        accelerateRealVertexColors =
        [
            inactiveColor,
            inactiveColor,
            accelColor,
            accelColor,
            inactiveColor,
            inactiveColor,
            accelColor,
            accelColor
        ];

        brakeRealVertexColors =
        [
            brakeColor,
            brakeColor,
            inactiveColor,
            inactiveColor,
            brakeColor,
            brakeColor,
            inactiveColor,
            inactiveColor
        ];

        padVertexColors =
        [
            steerColor,
            steerColor,
            steerColor,
            steerColor,
            inactiveColor,
            inactiveColor,
            inactiveColor
        ];

        mouseNoClickVertexColors = new Vec4[mousePositions.Length];
        Array.Fill(mouseNoClickVertexColors, config.InactiveColor);

        mouseLeftClickVertexColors =
        [
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            mouseLeftClickColor,
            mouseLeftClickColor,
            mouseLeftClickColor,
            mouseLeftClickColor,
            mouseLeftClickColor,
            mouseLeftClickColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor
        ];

        mouseRightClickVertexColors =
        [
            mouseRightClickColor,
            mouseRightClickColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            mouseRightClickColor,
            mouseRightClickColor,
            mouseRightClickColor,
            mouseRightClickColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor
        ];

        mouseBothClickVertexColors =
        [
            mouseRightClickColor,
            mouseRightClickColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            mouseRightClickColor,
            mouseRightClickColor,
            mouseRightClickColor,
            mouseRightClickColor,
            mouseLeftClickColor,
            mouseLeftClickColor,
            mouseLeftClickColor,
            mouseLeftClickColor,
            mouseLeftClickColor,
            mouseLeftClickColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor
        ];
    }

    public override CGameCtnMediaBlockTriangles InitiateDigitalSteer(TimeSingle time, bool pressed)
    {
        // spawn keyboard left/right key
        var block = CGameCtnMediaBlockTriangles2D.Create(pressed ? keyAccelVertexColorsOn : keyVertexColorsOff)
            .WithTriangles(keyTriangles)
            .ForTMUF()
            .Build();

        var key = GetDigitalSteerKeyframe(block, time);

        block.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockTriangles InitiateDigitalAccel(TimeSingle time, bool pressed)
    {
        var block = CGameCtnMediaBlockTriangles2D.Create(pressed ? keyAccelVertexColorsOn : keyVertexColorsOff)
            .WithTriangles(keyTriangles)
            .ForTMUF()
            .Build();

        var key = GetDigitalAccelKeyframe(block, time);

        block.Keys.Add(key);

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

    public override CGameCtnMediaBlockTriangles InitiateDigitalBrake(TimeSingle time, bool pressed)
    {
        var block = CGameCtnMediaBlockTriangles2D.Create(pressed ? keyBrakeVertexColorsOn : keyVertexColorsOff)
            .WithTriangles(keyTriangles)
            .ForTMUF()
            .Build();

        var key = GetDigitalBrakeKeyframe(block, time);

        block.Keys.Add(key);

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

    public override CGameCtnMediaBlockTriangles InitiateActionSlot(TimeSingle time, int index, bool pressed, bool activated)
    {
        var block = CGameCtnMediaBlockTriangles2D.Create(pressed ? keyAccelVertexColorsOn : keyVertexColorsOff)
            .WithTriangles(keyTriangles)
            .ForTMUF()
            .Build();

        var key = GetActionSlotKeyframe(block, time, index);

        block.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockTriangles.Key GetDigitalSteerKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time)
    {
        var aspectRatio = config.GetAspectRatio();
        
        var y = config.DigitalSteerPos.Y;
        var width = config.DigitalSteerSize.X;
        var height = config.DigitalSteerSize.Y;

        var leftSideX = config.DigitalSteerPos.X + config.DigitalSteerSpacing.X;

        var firstX = leftSideX * GetSteerMultiplier() * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var firstY = (y + height) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        var secondX = (leftSideX + width) * GetSteerMultiplier() * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var secondY = y * config.PrimaryScale.Y + config.PrimaryPos.Y;

        return new CGameCtnMediaBlockTriangles.Key(block)
        {
            Time = time,
            Positions =
            [
                new Vec3(firstX, firstY, 0),
                new Vec3(firstX, secondY, 0),
                new Vec3(secondX, firstY, 0),
                new Vec3(secondX, secondY, 0)
            ]
        };
    }

    private CGameCtnMediaBlockTriangles.Key GetDigitalGasKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time, float x, float y, float width, float height)
    {
        var aspectRatio = config.GetAspectRatio();

        var firstX = (x - width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var firstY = (y + height / 2) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        var secondX = (x + width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var secondY = (y - height / 2) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        return new CGameCtnMediaBlockTriangles.Key(block)
        {
            Time = time,
            Positions =
            [
                new Vec3(firstX, firstY, 0),
                new Vec3(firstX, secondY, 0),
                new Vec3(secondX, firstY, 0),
                new Vec3(secondX, secondY, 0)
            ]
        };
    }

    public override CGameCtnMediaBlockTriangles.Key GetDigitalAccelKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time)
    {
        var x = config.DigitalAccelPos.X;
        var y = config.DigitalAccelPos.Y;
        var width = config.DigitalAccelSize.X;
        var height = config.DigitalAccelSize.Y;

        return GetDigitalGasKeyframe(block, time, x, y, width, height);
    }

    public override CGameCtnMediaBlockTriangles.Key GetAnalogAccelKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time, float value)
    {
        var aspectRatio = config.GetAspectRatio();

        var x = config.DigitalAccelPos.X;
        var y = config.DigitalAccelPos.Y;
        var width = config.DigitalAccelSize.X;
        var height = config.DigitalAccelSize.Y;

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
    
    public override CGameCtnMediaBlockTriangles.Key GetDigitalBrakeKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time)
    {
        var x = config.DigitalBrakePos.X;
        var y = config.DigitalBrakePos.Y;
        var width = config.DigitalBrakeSize.X;
        var height = config.DigitalBrakeSize.Y;

        return GetDigitalGasKeyframe(block, time, x, y, width, height);
    }

    public override CGameCtnMediaBlockTriangles.Key GetAnalogBrakeKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time, float value)
    {
        var aspectRatio = config.GetAspectRatio();

        var x = config.DigitalBrakePos.X;
        var y = config.DigitalBrakePos.Y;
        var width = config.DigitalBrakeSize.X;
        var height = config.DigitalBrakeSize.Y;

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

    public override CGameCtnMediaBlockTriangles.Key GetActionSlotKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time, int index)
    {
        var aspectRatio = config.GetAspectRatio();

        var actionKeyCountPerRow = 5;

        var xPercentage = (index % actionKeyCountPerRow / (float)(actionKeyCountPerRow - 1) - 0.5f) * 2;
        var yPercentage = index / actionKeyCountPerRow / (float)(actionKeyCountPerRow - 1) * 2;

        var width = config.ActionKeysScale.X * 2 / (actionKeyCountPerRow - 1 + config.ActionKeysSpacing.X);
        var height = width;

        var x = -xPercentage * config.ActionKeysScale.X;
        var y = -yPercentage * config.ActionKeysScale.Y + config.ActionKeysPos.Y;

        var firstX = (x - width / 2) * aspectRatio + config.ActionKeysPos.X;
        var firstY = y + height / 2;

        var secondX = (x + width / 2) * aspectRatio + config.ActionKeysPos.X;
        var secondY = y - height / 2;

        return new CGameCtnMediaBlockTriangles.Key(block)
        {
            Time = time,
            Positions =
            [
                new Vec3(firstX, firstY, 0),
                new Vec3(firstX, secondY, 0),
                new Vec3(secondX, firstY, 0),
                new Vec3(secondX, secondY, 0)
            ]
        };
    }

    public override void ApplyAnalogAccel(CGameCtnMediaBlockTriangles block, TimeSingle time, float value)
    {
        block.Keys.Add(GetAnalogAccelKeyframe(block, time, value));
    }

    public override void ApplyAnalogBrake(CGameCtnMediaBlockTriangles block, TimeSingle time, float value)
    {
        block.Keys.Add(GetAnalogBrakeKeyframe(block, time, value));
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="time">Actual time without an applied <see cref="ClipInputConfig.StartOffset"/>.</param>
    /// <param name="value"></param>
    /// <returns></returns>
    public override CGameCtnMediaBlockTriangles.Key GetAnalogSteerKeyframe(CGameCtnMediaBlockTriangles node, TimeInt32 time, float value)
    {
        var aspectRatio = config.GetAspectRatio();

        var scale = config.PrimaryScale;
        var globalPos = config.PrimaryPos;
        var yLocal = config.AnalogSteerPos.Y;
        var width = config.AnalogSteerSize.X;
        var height = config.AnalogSteerSize.Y;

        var leftSideX = config.AnalogSteerPos.X + config.AnalogSteerSpacing.X;

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

    public override void ApplyAnalogSteer(CGameCtnMediaBlockTriangles block, TimeSingle time, float value)
    {
        block.Keys.Add(GetAnalogSteerKeyframe(block, time, value));
    }

    public override CGameCtnMediaBlock? InitiateActionSlotNum(TimeSingle timeStart, TimeInt32 timeEnd, int index)
    {
        var slot = (index + 1) % 10;
        
        var aspectRatio = config.GetAspectRatio();

        var actionKeyCountPerRow = 5;

        var xPercentage = (index % actionKeyCountPerRow / (float)(actionKeyCountPerRow - 1) - 0.5f) * 2;
        var yPercentage = index / actionKeyCountPerRow / (float)(actionKeyCountPerRow - 1) * 2;

        var x = -xPercentage * config.ActionKeysScale.X * aspectRatio + config.ActionKeysPos.X;
        var y = -yPercentage * config.ActionKeysScale.Y + config.ActionKeysPos.Y;

        var pos = (x + 0.005f, y + 0.02f);

        var effect = CControlEffectSimi.Create()
            .Centered()
            .WithKeys([
                new CControlEffectSimi.Key
                {
                    Time = timeStart,
                    Depth = 0,
                    Scale = config.ActionKeysScale * 2,
                    Position = pos
                },
                new CControlEffectSimi.Key
                {
                    Time = timeEnd,
                    Depth = 0,
                    Scale = config.ActionKeysScale * 2,
                    Position = pos
                }
            ])
            .ForTMUF()
            .Build();

        return CGameCtnMediaBlockText.Create(effect)
            .WithText(string.Format(config.ActionKeysTextFormat, slot))
            .WithColor(config.ActionKeysTextColor)
            .ForTMUF()
            .Build();
    }

    public override CGameCtnMediaBlockTriangles InitiateMouse(TimeSingle time, bool leftClick, bool rightClick, Vec2 curPos, Vec2 offset)
    {
        var colors = mouseNoClickVertexColors;

        if (leftClick && rightClick)
        {
            colors = mouseBothClickVertexColors;
        }
        else if (leftClick)
        {
            colors = mouseLeftClickVertexColors;
        }
        else if (rightClick)
        {
            colors = mouseRightClickVertexColors;
        }

        var block = CGameCtnMediaBlockTriangles2D.Create(colors)
            .WithTriangles(mouseTriangles)
            .ForTMUF()
            .Build();

        ApplyMouse(block, time, curPos, offset);

        return block;
    }

    public override CGameCtnMediaBlockTriangles.Key GetMouseKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time, Vec2 curPos, Vec2 offset)
    {
        var positions = new Vec3[mousePositions.Length];

        for (var i = 0; i < positions.Length; i++)
        {
            var x = Math.Clamp(curPos.X - offset.X, -2000, 2000);
            var y = Math.Clamp(curPos.Y - offset.Y, -1000, 1000);

            positions[i] = (mousePositions[i] + new Vec2(x, y) * 0.0002f) * (config.GetAspectRatio(), 1) * config.MouseScale + config.MousePos;
        }

        return new CGameCtnMediaBlockTriangles.Key(block)
        {
            Time = time + config.StartOffset,
            Positions = positions
        };
    }

    public override void ApplyMouse(CGameCtnMediaBlockTriangles block, TimeSingle time, Vec2 curPos, Vec2 offset)
    {
        block.Keys.Add(GetMouseKeyframe(block, time, curPos, offset));
    }

    private CGameCtnMediaBlockText InitiateGenericKey(string text, TimeSingle time, bool pressed, Vec2 position, Vec2 scale)
    {
        var effect = CControlEffectSimi.Create()
            .Centered()
            .ForTMUF()
            .Interpolated()
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
            Time = time + config.StartOffset,
            Depth = 0,
            Scale = scale,
            Position = position
        });

        return block;
    }

    public override CGameCtnMediaBlock InitiateJump(TimeSingle time, bool pressed)
    {
        return InitiateGenericKey("JUMP", time, pressed, config.JumpPos, config.JumpScale);
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

    public static Vec3[] GetMousePositions() => mousePositions;
    public static Int3[] GetMouseTriangles() => mouseTriangles;
}
