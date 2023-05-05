using GBX.NET;
using GBX.NET.Engines.Game;
using TmEssentials;

namespace ClipInput.Designs;

public class CompactDesign : BasicDesign
{
    private readonly ClipInputConfig config;

    private static readonly Int3[] padTriangles = new Int3[]
    {
        (0, 1, 4),
        (1, 2, 4),

        (0, 3, 6),
        (0, 4, 6),
        (2, 4, 6),
        (2, 5, 6),

        (3, 6, 7),
        (5, 6, 7)
    };

    private static readonly Int3[] triangle = new Int3[] { (0, 1, 2) };
    
    private static readonly Int3[] gasTriangles = new Int3[]
    {
        (0, 2, 3),
        (1, 0, 3),
        (1, 4, 3),

        (1, 4, 6),

        (2, 3, 5),
        (3, 6, 5),
        (3, 4, 6)
    };
    
    private static readonly Int3[] digitalSteerTriangles = new Int3[] { (0, 1, 2), (0, 2, 3) };

    private readonly Vec4[] padVertexColors;
    private readonly Vec4[] accelVertexColorsOn;
    private readonly Vec4[] brakeVertexColorsOn;
    private readonly Vec4[] steerVertexColorsOn;
    private readonly Vec4[] steerVertexColorsOff;
    private readonly Vec4[] gasVertexColorsOff;
    private readonly Vec4[] accelRealVertexColors;
    private readonly Vec4[] brakeRealVertexColors;

    public CompactDesign(ClipInputConfig config) : base(config)
    {
        this.config = config;

        var accelColor = Skin.AccelColor ?? config.ActiveColor;
        var brakeColor = Skin.BrakeColor ?? config.BrakeColor;
        var steerColor = Skin.SteerColor ?? config.ActiveColor;
        var inactiveColor = Skin.InactiveColor ?? config.InactiveColor;

        padVertexColors = new[]
        {
            steerColor,
            steerColor,
            steerColor,
            inactiveColor,
            steerColor,
            inactiveColor,
            inactiveColor,
            inactiveColor
        };

        accelVertexColorsOn = new[]
        {
            accelColor,
            accelColor,
            accelColor
        };

        brakeVertexColorsOn = new[]
        {
            brakeColor,
            brakeColor,
            brakeColor
        };

        steerVertexColorsOn = new[]
        {
            steerColor,
            steerColor,
            steerColor,
            steerColor
        };

        steerVertexColorsOff = new[]
        {
            inactiveColor,
            inactiveColor,
            inactiveColor,
            inactiveColor
        };

        gasVertexColorsOff = new[]
        {
            inactiveColor,
            inactiveColor,
            inactiveColor
        };

        accelRealVertexColors = new[]
        {
            inactiveColor,
            accelColor,
            inactiveColor,
            inactiveColor,
            accelColor,
            inactiveColor,
            accelColor
        };

        brakeRealVertexColors = new[]
        {
            inactiveColor,
            brakeColor,
            inactiveColor,
            inactiveColor,
            brakeColor,
            inactiveColor,
            brakeColor
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
        var width = config.AnalogSteerSize.X;
        var height = config.AnalogSteerSize.Y;

        var leftSideX = config.AnalogSteerPos.X + 0.01f;

        var firstX = leftSideX * GetSteerMultiplier() * aspectRatio * scale.X + globalPos.X;
        var secondX = (leftSideX + 0.1f) * GetSteerMultiplier() * aspectRatio * scale.X + globalPos.X;
        var firstTopY = (yLocal + height * 0.5f) * scale.Y + globalPos.Y;
        var firstBottomY = (yLocal - height * 0.5f) * scale.Y + globalPos.Y;

        var thirdX = ((leftSideX + 0.1f * (1 - value)) + width * value) * GetSteerMultiplier() * aspectRatio * scale.X + globalPos.X;
        var thirdY = yLocal * scale.Y + globalPos.Y;

        var fourthX = (leftSideX + width) * GetSteerMultiplier() * aspectRatio * scale.X + globalPos.X;

        return new CGameCtnMediaBlockTriangles.Key(node)
        {
            Time = time.ToTimeSingle() + config.StartOffset,
            Positions = new[]
            {
                new Vec3(firstX, firstTopY, 0),
                new Vec3(secondX, thirdY, 0),
                new Vec3(firstX, firstBottomY, 0),
                new Vec3(firstX, firstTopY, 0),
                new Vec3(thirdX, thirdY, 0),
                new Vec3(firstX, firstBottomY, 0),
                new Vec3(thirdX, thirdY, 0),
                new Vec3(fourthX, thirdY, 0),
            }
        };
    }

    public override CGameCtnMediaBlockTriangles InitiateDigitalAccel(TimeSingle time, bool pressed)
    {
        var block = CGameCtnMediaBlockTriangles2D.Create(pressed ? accelVertexColorsOn : gasVertexColorsOff)
            .WithTriangles(triangle)
            .ForTMUF()
            .Build();

        var key = GetDigitalAccelKeyframe(block, time);

        block.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockTriangles.Key GetDigitalAccelKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time)
    {
        var aspectRatio = config.GetAspectRatio();
        
        var x = config.DigitalAccelPos.X;
        var y = config.DigitalAccelPos.Y - 0.01f;
        var width = config.DigitalAccelSize.X;
        var height = config.DigitalAccelSize.Y;

        var firstX = x * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var firstY = (y + height / 2) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        var secondX = (x + width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var secondY = (y - height / 2) + config.PrimaryPos.Y;
        
        var thirdX = (x - width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;

        return new CGameCtnMediaBlockTriangles.Key(block)
        {
            Time = time,
            Positions = new[]
            {
                new Vec3(firstX, firstY, 0),
                new Vec3(secondX, secondY, 0),
                new Vec3(thirdX, secondY, 0)
            }
        };
    }

    public override CGameCtnMediaBlockTriangles InitiateAnalogAccel(TimeSingle time, float value)
    {
        var block = CGameCtnMediaBlockTriangles2D.Create(accelRealVertexColors)
            .WithTriangles(gasTriangles)
            .ForTMUF()
            .Build();

        var key = GetAnalogAccelKeyframe(block, time, value);

        block.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockTriangles.Key GetAnalogAccelKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time, float value)
    {
        var aspectRatio = config.GetAspectRatio();

        var x = config.DigitalAccelPos.X;
        var y = config.DigitalAccelPos.Y - 0.01f;
        var width = config.DigitalAccelSize.X;
        var height = config.DigitalAccelSize.Y;

        var firstX = (x - width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var firstY = (y + height / 2) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        var secondY = (y - height / 2 + height * value) * config.PrimaryScale.Y + config.PrimaryPos.Y;
        var secondX = (x + width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;

        var thirdX = x * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var thirdY = (y - height / 2) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        return new CGameCtnMediaBlockTriangles.Key(block)
        {
            Time = time,
            Positions = new[]
            {
                new Vec3(firstX, thirdY, 0),
                new Vec3(firstX, thirdY, 0),
                new Vec3(thirdX, firstY, 0),
                new Vec3(thirdX, secondY, 0),
                new Vec3(thirdX, secondY, 0),
                new Vec3(secondX, thirdY, 0),
                new Vec3(secondX, thirdY, 0)
            }
        };
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

    public override CGameCtnMediaBlockTriangles.Key GetAnalogBrakeKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time, float value)
    {
        var aspectRatio = config.GetAspectRatio();

        var x = config.DigitalBrakePos.X;
        var y = config.DigitalBrakePos.Y + 0.01f;
        var width = config.DigitalBrakeSize.X;
        var height = config.DigitalBrakeSize.Y;

        var firstX = (x - width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var firstY = (y + height / 2) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        var secondY = (y + height / 2 - height * value) * config.PrimaryScale.Y + config.PrimaryPos.Y;
        var secondX = (x + width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;

        var thirdX = x * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var thirdY = (y - height / 2) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        return new CGameCtnMediaBlockTriangles.Key(block)
        {
            Time = time,
            Positions = new[]
            {
                new Vec3(firstX, firstY, 0),
                new Vec3(firstX, firstY, 0),
                new Vec3(thirdX, thirdY, 0),
                new Vec3(thirdX, secondY, 0),
                new Vec3(thirdX, secondY, 0),
                new Vec3(secondX, firstY, 0),
                new Vec3(secondX, firstY, 0)
            }
        };
    }

    public override CGameCtnMediaBlockTriangles InitiateDigitalBrake(TimeSingle time, bool pressed)
    {
        var block = CGameCtnMediaBlockTriangles2D.Create(pressed ? brakeVertexColorsOn : gasVertexColorsOff)
            .WithTriangles(triangle)
            .ForTMUF()
            .Build();

        var key = GetDigitalBrakeKeyframe(block, time);

        block.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockTriangles.Key GetDigitalBrakeKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time)
    {
        var aspectRatio = config.GetAspectRatio();

        var x = config.DigitalBrakePos.X;
        var y = config.DigitalBrakePos.Y + 0.01f;
        var width = config.DigitalBrakeSize.X;
        var height = config.DigitalBrakeSize.Y;

        var firstX = x * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var firstY = (y - height / 2) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        var secondX = (x - width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var secondY = (y + height / 2) + config.PrimaryPos.Y;

        var thirdX = (x + width / 2) * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;

        return new CGameCtnMediaBlockTriangles.Key(block)
        {
            Time = time,
            Positions = new[]
            {
                new Vec3(firstX, firstY, 0),
                new Vec3(secondX, secondY, 0),
                new Vec3(thirdX, secondY, 0)
            }
        };
    }

    public override CGameCtnMediaBlockTriangles InitiateDigitalSteer(TimeSingle time, bool pressed)
    {
        var block = CGameCtnMediaBlockTriangles2D.Create(pressed ? steerVertexColorsOn : steerVertexColorsOff)
            .WithTriangles(digitalSteerTriangles)
            .ForTMUF()
            .Build();

        var key = GetDigitalSteerKeyframe(block, time);

        block.Keys.Add(key);

        return block;
    }

    public override CGameCtnMediaBlockTriangles.Key GetDigitalSteerKeyframe(CGameCtnMediaBlockTriangles block, TimeSingle time)
    {
        var aspectRatio = config.GetAspectRatio();

        var y = config.DigitalSteerPos.Y;
        var width = config.DigitalSteerSize.X;
        var height = config.DigitalSteerSize.Y;

        var leftSideX = config.DigitalSteerPos.X + 0.01f;
        
        var firstX = (leftSideX + width * 0.5f) * GetSteerMultiplier() * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;

        var secondX = leftSideX * GetSteerMultiplier() * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var firstY = (y + height) * config.PrimaryScale.Y + config.PrimaryPos.Y;

        var thirdX = (leftSideX + width) * GetSteerMultiplier() * aspectRatio * config.PrimaryScale.X + config.PrimaryPos.X;
        var secondY = y * config.PrimaryScale.Y + config.PrimaryPos.Y;

        return new CGameCtnMediaBlockTriangles.Key(block)
        {
            Time = time,
            Positions = new[]
            {
                new Vec3(firstX, firstY, 0),
                new Vec3(secondX, secondY, 0),
                new Vec3(thirdX, secondY, 0),
                new Vec3(thirdX, firstY, 0)
            }
        };
    }
}
