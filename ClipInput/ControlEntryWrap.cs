using GBX.NET;
using System;

namespace ClipInput
{
    public struct ControlEntryWrap
    {
        public string Name { get; set; }
        public TimeSpan Time { get; set; }
        public bool? IsEnabled { get; set; }
        public float? Value { get; set; }

        public ControlEntryWrap(ControlEntry entry)
        {
            Name = entry.Name;
            Time = entry.Time;

            if (entry is ControlEntryAnalog a)
            {
                Value = a.Value;
                IsEnabled = null;
            }
            else
            {
                Value = null;
                IsEnabled = entry.IsEnabled;
            }
        }
    }
}