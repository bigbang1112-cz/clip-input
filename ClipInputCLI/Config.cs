using ClipInput;
using GBX.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipInputCLI
{
    public class Config
    {
        public float[] AspectRatio { get; set; }
        public float[] Scale { get; set; }
        public float[] Space { get; set; }
        public float[] Position { get; set; }
        public bool? ShowAfterInteraction { get; set; }
        public float[] PadOffset { get; set; }
        public float[] PadColor { get; set; }
        public float[] PadBrakeColor { get; set; }
        public float[] PadStartPosition { get; set; }
        public float[] PadEndPosition { get; set; }
        public Theme? Theme { get; set; }
        public float? StartOffset { get; set; }
        public bool? AdjustToFPS { get; set; }
        public float? FPS { get; set; }
        public KeyboardKey[] Keys { get; set; }
    }
}
