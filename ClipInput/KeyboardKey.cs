using GBX.NET;

namespace ClipInput
{
    public class KeyboardKey
    {
        private string entryName;

        public string EntryName
        {
            get => entryName;
            set
            {
                entryName = value;
                EntryNames = new string[] { EntryName };
            }
        }
        public string[] EntryNames { get; set; }
        public string TrackName { get; set; }
        public Vec2 Position { get; set; }
        public string ImageOff { get; set; }
        public string ImageOn { get; set; }
        public bool IsSteerInput { get; set; }
    }
}
