using GbxToolAPI;

namespace ClipInput;

public class ClipInputDictionary : ITextDictionary
{
    public string MediaTrackerTrackSteer { get; set; } = "CI2: Steer {0}";
    public string MediaTrackerTrackSteerValue { get; set; } = "CI2: Steer {0} (value)";
    public string MediaTrackerTrackSteerBack { get; set; } = "CI2: Steer {0} (back)";
    public string MediaTrackerTrackAccel { get; set; } = "CI2: Accel";
    public string MediaTrackerTrackAccelValue { get; set; } = "CI2: Accel (value)";
    public string MediaTrackerTrackAccelBack { get; set; } = "CI2: Accel (back)";
    public string MediaTrackerTrackBrake { get; set; } = "CI2: Brake";
    public string MediaTrackerTrackBrakeValue { get; set; } = "CI2: Brake (value)";
    public string MediaTrackerTrackBrakeBack { get; set; } = "CI2: Brake (back)";
    public string MediaTrackerTrackMouse { get; set; } = "CI2: Mouse";
    public string MediaTrackerTrackActionKey { get; set; } = "CI2: Action key {0}";
    public string MediaTrackerTrackActionKeyOverlay { get; set; } = "CI2: Action key {0} (overlay)";
    public string MediaTrackerTrackWalkForward { get; set; } = "CI2: Walk Forward";
    public string MediaTrackerTrackWalkBackward { get; set; } = "CI2: Walk Backward";
    public string MediaTrackerTrackStrafe { get; set; } = "CI2: Strafe {0}";
    public string MediaTrackerTrackJump { get; set; } = "CI2: Jump";
    public string MediaTrackerTrackHorn { get; set; } = "CI2: Horn";
    public string MediaTrackerTrackRespawn { get; set; } = "CI2: Respawn";
    public string MediaTrackerTrackSecondaryRespawn { get; set; } = "CI2: Respawn (secondary)";

    public string Left { get; set; } = "Left";
    public string Right { get; set; } = "Right";
}
