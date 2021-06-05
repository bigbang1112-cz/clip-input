namespace ClipInputCLI
{
    public class KeyboardKey
    {
        public string EntryName { get; set; }
        public string[] EntryNames { get; set; }
        public string TrackName { get; set; }
        public float[] Position { get; set; }
        public string ImageOff { get; set; }
        public string ImageOn { get; set; }
        public bool? IsSteerInput { get; set; }
    }
}
