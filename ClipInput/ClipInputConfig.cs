using ClipInput.Designs;
using GBX.NET;
using GbxToolAPI;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using TmEssentials;

namespace ClipInput;

public class ClipInputConfig : Config, IHasTextDictionary<ClipInputDictionary>
{
    private string designId = "Basic";

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    private Type? design;

    public Vec2 AspectRatio { get; set; } = new(16, 9);
    public TimeSingle StartOffset { get; set; }

    [Color] public Vec4 ActiveColor { get; set; } = new(0.2f, 0.6f, 0.9f, 1);
    [Color] public Vec4 InactiveColor { get; set; } = new(0.1f, 0.1f, 0.1f, 1);
    [Color] public Vec4 BrakeColor { get; set; } = new(0.9f, 0.3f, 0.2f, 1);

    public TimeSingle InterpTolerance { get; set; } = new TimeSingle(0.0001f);

    /// <summary>
    /// When no steering inputs are identified in the run, layout of this specific device should be used by default.
    /// </summary>
    public SteerDevice DefaultDevice { get; set; } = SteerDevice.Keyboard;

    public bool AdjustToFPS { get; set; }
    public float FPS { get; set; } = 30;

    public Vec2 PrimaryPos { get; set; } = (0, 0.55f);
    public Vec2 PrimaryScale { get; set; } = (0.8f, 0.8f);

    public Vec2 AnalogSteerPos { get; set; } = (0, 0f);
    public Vec2 AnalogSteerSpacing { get; set; } = (0.11f, 0);
    public Vec2 AnalogSteerSize { get; set; } = (0.4f, 0.4f);

    public Vec2 DigitalSteerPos { get; set; } = (0, -0.2f);
    public Vec2 DigitalSteerSpacing { get; set; } = (0.11f, 0);
    public Vec2 DigitalSteerSize { get; set; } = (0.19f, 0.19f);

    public Vec2 AnalogAccelPos { get; set; } = (0, 0.105f);
    public Vec2 AnalogAccelSize { get; set; } = (0.19f, 0.19f);

    public Vec2 DigitalAccelPos { get; set; } = (0, 0.105f);
    public Vec2 DigitalAccelSize { get; set; } = (0.19f, 0.19f);

    public Vec2 AnalogBrakePos { get; set; } = (0, -0.105f);
    public Vec2 AnalogBrakeSize { get; set; } = (0.19f, 0.19f);

    public Vec2 DigitalBrakePos { get; set; } = (0, -0.105f);
    public Vec2 DigitalBrakeSize { get; set; } = (0.19f, 0.19f);

    public bool EnableAnalogSteerValue { get; set; } = true;
    public bool EnableAnalogAccelValue { get; set; } = true;
    public bool EnableAnalogBrakeValue { get; set; } = true;

    public Vec2 AnalogSteerValueScale { get; set; } = (0.75f, 0.75f);
    public Vec2 AnalogSteerValueOffset { get; set; } = (0.5f, 0);
    [Color] public Vec3 AnalogSteerValueColor { get; set; } = (1, 1, 1);

    public Vec2 AnalogAccelValueScale { get; set; } = (0.6f, 0.6f);
    public Vec2 AnalogAccelValueOffset { get; set; } = (-0.005f, 0.17f);
    [Color] public Vec3 AnalogAccelValueColor { get; set; } = (1, 1, 1);

    public Vec2 AnalogBrakeValueScale { get; set; } = (0.6f, 0.6f);
    public Vec2 AnalogBrakeValueOffset { get; set; } = (-0.005f, -0.15f);
    [Color] public Vec3 AnalogBrakeValueColor { get; set; } = (1, 1, 1);

    public string AnalogValueTextFormat { get; set; } = "$s$o$n{0}";
    public string AnalogValueNumberFormat { get; set; } = "0.00";

    public bool EnableMouse { get; set; } = true;
    public bool EnableMouseMovement { get; set; } = true;
    public Vec2 MousePos { get; set; } = (-0.65f, -0.35f);
    public Vec2 MouseScale { get; set; } = (0.8f, 0.8f);

    public bool EnableActionKeys { get; set; } = true;
    public Vec2 ActionKeysPos { get; set; } = (0, 0.34f);
    public Vec2 ActionKeysScale { get; set; } = (0.15f, 0.15f);
    public Vec2 ActionKeysSpacing { get; set; } = (0.5f, 0.5f);
    [Color] public Vec3 ActionKeysTextColor { get; set; } = (1, 1, 1);
    public string ActionKeysTextFormat { get; set; } = "$o$n{0}";

    public bool EnableJump { get; set; } = true;
    public Vec2 JumpScale { get; set; } = (1.5f, 1.5f);
    public Vec2 JumpPos { get; set; } = (0.6f, -0.6f);

    public bool EnableHorn { get; set; } = true;
    public Vec2 HornScale { get; set; } = (0.75f, 0.75f);
    public Vec2 HornPos { get; set; } = (-0.65f, 0.65f);

    public bool EnableRespawn { get; set; } = true;
    public Vec2 RespawnScale { get; set; } = (0.75f, 0.75f);
    public Vec2 RespawnPos { get; set; } = (-0.65f, 0.5f);

    public bool EnableSecondaryRespawn { get; set; } = true;
    public Vec2 SecondaryRespawnScale { get; set; } = (0.5f, 0.5f);
    public Vec2 SecondaryRespawnPos { get; set; } = (-0.65f, 0.39f);

    public TimeInt32 KeyTapPressTime { get; set; } = new(200);

    public ClipInputDictionary Dictionary { get; set; } = new();

    public CultureInfo Formatting { get; set; } = CultureInfo.InvariantCulture;

    public string DesignId
    {
        get => designId;
        set
        {
            design = value.ToLowerInvariant() switch
            {
                "basic" => typeof(BasicDesign),
                "compact" => typeof(CompactDesign),
                "image" => typeof(ImageDesign),
                "text" => typeof(TextDesign),
                _ => throw new NotImplementedException($"{value} is not a valid design ID.")
            };

            designId = value;
        }
    }

    internal IDesign Design { get; set; }

    public string SkinId { get; set; } = "";
    internal Skin? Skin { get; set; }

    public bool PreferManiaPlanet { get; set; }

    public ClipInputConfig()
    {
        Design = (IDesign)Activator.CreateInstance(typeof(BasicDesign), new[] { this })!;
    }

    internal void UpdateDesign()
    {
        Design = design is null ? new BasicDesign(this) : (IDesign)Activator.CreateInstance(design, new[] { this })!;
    }

    public float GetAspectRatio()
    {
        return AspectRatio.Y / AspectRatio.X;
    }

    public TimeSingle GetMinimalFrameLength()
    {
        return new TimeSingle(1 / FPS);
    }
}
