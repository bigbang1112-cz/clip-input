using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput;

internal static class DonadigoInputFile
{
    public static IEnumerable<IInput> Parse(string text)
    {
        using var r = new StringReader(text);
        
        var isDonadigoInputFile = true;
        var donadigoInputs = new List<IInput>();

        string? line;
        while ((line = r.ReadLine()) is not null)
        {
            var enumerator = line.Split(' ').GetEnumerator();

            while (enumerator.MoveNext())
            {
                var str = (string)enumerator.Current;

                var timeRangeSplit = str.Split('-');

                if (timeRangeSplit.Length == 0)
                {
                    throw new Exception("Corrupted Donadigo input file (invalid line)");
                }

                var time = ParseTime(timeRangeSplit[0]);
                var timeEnd = timeRangeSplit.Length > 1 ? ParseTime(timeRangeSplit[1]) : default(TimeInt32?);

                if (!enumerator.MoveNext())
                {
                    throw new Exception("Corrupted Donadigo input file (missing action)");
                }

                var action = (string)enumerator.Current;

                if (!enumerator.MoveNext())
                {
                    throw new Exception("Corrupted Donadigo input file (missing value)");
                }

                var value = (string)enumerator.Current;

                switch (action)
                {
                    case "press" or "rel":
                        var pressed = action is "press";

                        IInput input = value switch
                        {
                            "up" => new Accelerate(time, pressed),
                            "down" => new Brake(time, pressed),
                            "left" => new SteerLeft(time, pressed),
                            "right" => new SteerRight(time, pressed),
                            "enter" => new Respawn(time, pressed),
                            _ => throw new Exception("Invalid value")
                        };

                        yield return input;

                        if (timeEnd.HasValue)
                        {
                            yield return value switch
                            {
                                "up" => new Accelerate(timeEnd.Value, Pressed: false),
                                "down" => new Brake(timeEnd.Value, Pressed: false),
                                "left" => new SteerLeft(timeEnd.Value, Pressed: false),
                                "right" => new SteerRight(timeEnd.Value, Pressed: false),
                                "enter" => new Respawn(timeEnd.Value, Pressed: false),
                                _ => throw new Exception("Invalid value")
                            };
                        }

                        break;
                    case "steer":
                        yield return new Steer(time, int.Parse(value));
                        break;
                    default:
                        throw new Exception("Invalid action");
                }
            }

            if (!isDonadigoInputFile)
            {
                break;
            }
        }
    }

    private static TimeInt32 ParseTime(string str)
    {
        var timeTypeSplit = str.Split(':', '.');

        var second = 0;
        var minute = 0;
        var hour = 0;

        if (timeTypeSplit.Length >= 2)
        {
            second = int.Parse(timeTypeSplit[^2]);

            if (timeTypeSplit.Length >= 3)
            {
                minute = int.Parse(timeTypeSplit[^3]);

                if (timeTypeSplit.Length >= 4)
                {
                    hour = int.Parse(timeTypeSplit[^4]);
                }
            }
        }

        var hundredths = int.Parse(timeTypeSplit[^1]);

        return new TimeInt32(hundredths * 10 + second * 1000 + minute * 60000 + hour * 3600000);
    }
}
