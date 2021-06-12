using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using ClipInput;
using GBX.NET;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClipInputCLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await DoMain(args);
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
                PressAnyKeyToContinue();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        static async Task DoMain(string[] args)
        {
            SetTitle();

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var fileExportYml = Path.Combine(baseDirectory, "Export.yml");
            var configYml = Path.Combine(baseDirectory, "Config.yml");
            var lastCheckForUpdatesYml = Path.Combine(baseDirectory, "LastCheckForUpdates.yml");
            var inputsFolder = Path.Combine(baseDirectory, "Inputs");

            var assemblyConsole = Assembly.GetExecutingAssembly();
            var assemblyConsoleName = assemblyConsole.GetName();
            var references = assemblyConsole.GetReferencedAssemblies();
            var assemblyClipInput = default(AssemblyName);
            var assemblyGBXNET = default(AssemblyName);

            SetTitle(assemblyConsoleName.Version);

            foreach (var reference in references)
            {
                switch (reference.Name)
                {
                    case "ClipInput":
                        assemblyClipInput = reference;
                        break;
                    case "GBX.NET":
                        assemblyGBXNET = reference;
                        break;
                }
            }

            if (assemblyClipInput is null)
                throw new DllNotFoundException("Library 'Clip Input' not found.");

            if (assemblyGBXNET is null)
                throw new DllNotFoundException("Library 'GBX.NET' not found.");

            SetTitle(assemblyConsoleName.Version, assemblyClipInput.Version, assemblyGBXNET.Version);

            var allArgsAreFiles = true;

            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    if (!File.Exists(arg))
                        allArgsAreFiles = false;
                }
            }

            string[] fileNames = Array.Empty<string>();

            if (allArgsAreFiles)
                fileNames = args;
            else if (args.Length > 0)
                fileNames = new string[] { args[0] };

            if (fileNames.Length == 0) fileNames = new string[] { null };

            foreach (var fileName in fileNames)
            {
                if (args.Length < 2 || allArgsAreFiles)
                {
                    Console.WriteLine("Welcome to Clip Input! Powered by GBX.NET: https://github.com/BigBang1112/gbx-net");
                    Console.WriteLine("A tool to extract inputs from any Trackmania replay, ghost, or clip (except TM2020 and TMTurbo)\nand export them into a Clip.Gbx visualization to be then imported straight into MediaTracker.");

                    var fileNotFound = false;

                    Dictionary<string, DateTime?> lastCheckForUpdates;

                    try
                    {
                        lastCheckForUpdates = YamlManager.Read<Dictionary<string, DateTime?>>(lastCheckForUpdatesYml);
                    }
                    catch (FileNotFoundException)
                    {
                        fileNotFound = true;

                        lastCheckForUpdates = new Dictionary<string, DateTime?>();
                    }

                    Console.WriteLine();

                    var date = default(DateTime?);

                    if (fileNotFound || !lastCheckForUpdates.TryGetValue("LastCheckForUpdates", out date)
                        || !date.HasValue || DateTime.Now - date > TimeSpan.FromHours(1))
                    {
                        lastCheckForUpdates["LastCheckForUpdates"] = DateTime.Now;

                        YamlManager.Write(lastCheckForUpdatesYml, lastCheckForUpdates, true);

                        Console.Write("Checking for updates... ");

#if NET452

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

#endif

                        using var http = new HttpClient()
                        {
                            BaseAddress = new Uri("https://api.github.com/")
                        };

                        http.DefaultRequestHeaders.UserAgent.ParseAdd("Clip Input");

                        var getReleases = await http.GetAsync($"repos/bigbang1112-cz/clip-input/releases");

                        Console.WriteLine();
                        Console.WriteLine();

                        if (getReleases.IsSuccessStatusCode)
                        {
                            dynamic releases = JsonConvert.DeserializeObject(await getReleases.Content.ReadAsStringAsync());

                            if (releases.Count > 0)
                            {
                                var release = releases[0];
                                var version = release.tag_name.Value.Substring(1);

                                if (version == assemblyClipInput.Version.ToString(3))
                                {
                                    Console.WriteLine($"Clip Input {version} is up to date.");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"Clip Input {version} has been released!");
                                    Console.WriteLine($"https://github.com/bigbang1112-cz/clip-input/releases");
                                    Console.ResetColor();

                                    Console.WriteLine();
                                    PressAnyKeyToContinue();
                                }
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Could not check for updates.");
                            Console.ResetColor();
                        }

                        Console.WriteLine();
                    }

                    if (fileName is not null)
                    {
                        Console.WriteLine($"  {Path.GetFileName(fileName)}");
                        Console.WriteLine();
                    }

                    if (!File.Exists(fileExportYml))
                    {
                        Console.WriteLine("Before you start, I'll guide you through a quick setup to ensure you have the best experience with the tool.");
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Security warning: Directory paths will be shown in the console.");
                    Console.ResetColor();

                    Console.WriteLine();
                    PressAnyKeyToContinue();

                    Dictionary<Game, string> folderDictionary;
                    Dictionary<Game, string> folderExportDictionary;
                    Dictionary<Game, string> folderMediaDictionary = new()
                    {
                        { Game.TMUF, @"MediaTracker\Images" },
                        { Game.ManiaPlanet, @"Media\Images\Inputs" }
                    };

                    var games = Enum.GetValues<Game>().SkipWhile(x => x == Game.Unknown);

                    if (File.Exists(fileExportYml))
                    {
                        folderExportDictionary = YamlManager.Read<Dictionary<Game, string>>(fileExportYml);

                        foreach (var pair in folderExportDictionary)
                        {
                            if (pair.Value is not null)
                            {
                                Directory.CreateDirectory(pair.Value);
                                Console.WriteLine($"{pair.Key} clips will be exported in the '{pair.Value}' folder.");
                            }
                            else
                            {
                                Console.WriteLine($"{pair.Key} clips will be exported in the folder relative to the Clip Input program.");
                            }
                        }
                    }
                    else
                    {
                        folderDictionary = new();
                        folderExportDictionary = new();

                        Console.WriteLine("Let's define the Clip.Gbx export folders, so you can easily access the clips ingame.");

                        foreach (var game in games)
                        {
                            var autoDetected = default(bool?);

                            while (true)
                            {
                                Console.WriteLine();

                                if (!autoDetected.HasValue)
                                {
                                    var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                                    switch (game)
                                    {
                                        case Game.TMUF:
                                            {
                                                var userFolder = Path.Combine(documentsFolder, "TrackMania");
                                                var userFidFile = Path.Combine(userFolder, @"Config\User.FidCache.Gbx");

                                                if (File.Exists(userFidFile))
                                                {
                                                    folderDictionary[game] = userFolder;
                                                    folderExportDictionary[game] = Path.Combine(userFolder, @"Tracks\Inputs");
                                                }

                                                break;
                                            }
                                        case Game.ManiaPlanet:
                                            {
                                                var userFolder = Path.Combine(documentsFolder, "ManiaPlanet");
                                                var userFidFile = Path.Combine(userFolder, @"Config\User.FidCache.Gbx");

                                                if (File.Exists(userFidFile))
                                                {
                                                    folderDictionary[game] = userFolder;
                                                    folderExportDictionary[game] = Path.Combine(userFolder, @"Replays\Clips\Inputs");
                                                }

                                                break;
                                            }
                                    }

                                    autoDetected = folderExportDictionary.TryGetValue(game, out string directoryExport);

                                    if (autoDetected.Value)
                                    {
                                        Console.WriteLine($"The default export location for {game} is going to be '{directoryExport}'.\nYou can change this anytime later.");
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else if (!autoDetected.Value)
                                {
                                    Console.Write($"Enter your {game} installation path (leave empty if not installed or interested): ");

                                    var directoryInstallation = Console.ReadLine();

                                    Console.WriteLine();

                                    string directoryUser;

                                    if (string.IsNullOrWhiteSpace(directoryInstallation))
                                    {
                                        directoryUser = null;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            directoryUser = GetUserDirectory(game, directoryInstallation);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                            Console.WriteLine();
                                            continue;
                                        }
                                    }

                                    if (directoryUser == null)
                                    {
                                        Console.WriteLine($"Thank you.\nThe default export location for {game} is going to be relative to this program.\nYou can change this anytime later.");

                                        folderDictionary[game] = null;
                                        folderExportDictionary[game] = null;
                                    }
                                    else
                                    {
                                        var directoryExport = default(string);

                                        switch (game)
                                        {
                                            case Game.TMUF:
                                                directoryExport = Path.Combine(directoryUser, @"Tracks\Inputs");
                                                break;
                                            case Game.ManiaPlanet:
                                                directoryExport = Path.Combine(directoryUser, @"Replays\Clips\Inputs");
                                                break;
                                        }

                                        folderDictionary[game] = directoryUser;
                                        folderExportDictionary[game] = directoryExport;

                                        Console.WriteLine($"Thank you.\nThe default export location for {game} is going to be '{directoryExport}'.\nYou can change this anytime later.");
                                    }
                                }

                                Console.WriteLine();

                                char? key;

                                while (true)
                                {
                                    Console.Write($"Press Y/N to approve: ");

                                    key = Console.ReadLine()?.ToLower().FirstOrDefault();

                                    if (key == 'y' || key == 'n')
                                        break;
                                }

                                if (key == 'y')
                                    break;
                                else
                                    autoDetected = false;
                            }

                            if (folderExportDictionary.TryGetValue(game, out string directory) && directory != null)
                                Directory.CreateDirectory(directory);
                        }

                        YamlManager.Write(fileExportYml, folderExportDictionary, true);

                        Console.WriteLine();
                        Console.WriteLine("Export.yml has been created.");
                        Console.WriteLine();
                        PressAnyKeyToContinue();

                        Console.WriteLine("Exported clips use custom images that can have troubles showing up on the first try.");

                        char? keyCopy;

                        while (true)
                        {
                            Console.Write("Do you want to copy these images to the user directories [Y/N]? ");

                            keyCopy = Console.ReadLine()?.ToLower().FirstOrDefault();

                            if (keyCopy == 'y' || keyCopy == 'n')
                                break;
                        }

                        if (keyCopy == 'y')
                        {
                            Console.WriteLine();

                            foreach (var game in games)
                            {
                                if (folderDictionary.TryGetValue(game, out string dir) && dir is not null)
                                {
                                    var directoryToCopy = Path.Combine(dir, folderMediaDictionary[game]);

                                    Directory.CreateDirectory(directoryToCopy);

                                    foreach (var file in Directory.GetFiles(inputsFolder))
                                    {
                                        var fName = Path.GetFileName(file);
                                        Console.Write($"Copying '{fName}' to '{directoryToCopy}'...");
                                        File.Copy(file, Path.Combine(directoryToCopy, fName), true);
                                        Console.WriteLine(" DONE");
                                    }
                                }
                            }
                        }
                    }

                    if (args.Length == 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("You can now start using the program! Here's a quick overview:");
                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Drag and drop any replay in the range of TM1.0 to TM2 (except TMTurbo) on ClipInput.exe\nto automatically create the Clip.Gbx in export folders you have specified.");
                        Console.WriteLine("This method will take the parameters defined in Config.yml.");
                        Console.ResetColor();

                        Console.WriteLine();
                        PressAnyKeyToContinue();

                        Console.WriteLine("If you want to automate through different settings, you can use command line arguments:");
                        Console.WriteLine();

                        Console.WriteLine(File.ReadAllText(Path.Combine(baseDirectory, "Reference.txt")));

                        Console.WriteLine();
                        PressAnyKeyToContinue();

                        return;
                    }
                }

                var aspectRatio = default(Vec2?);
                var scale = default(Vec2?);
                var space = default(Vec2?);
                var pos = default(Vec2?);
                var showAfterInteraction = default(bool?);
                var padOffset = default(Vec2?);
                var padColor = default(Vec4?);
                var padBrakeColor = default(Vec4?);
                var padBackgroundColor = default(Vec4?);
                var padStartPosition = default(Vec3?);
                var padEndPosition = default(Vec3?);
                var theme = default(Theme?);
                var startOffset = default(TimeSpan?);
                var adjustToFPS = default(bool?);
                var fps = default(float?);

                var enumerator = ((IEnumerable<string>)args).GetEnumerator();

                while (enumerator.MoveNext())
                {
                    var command = enumerator.Current.ToLower();

                    switch (command)
                    {
                        case "-aspectratio":
                        case "-ratio":
                            {
                                aspectRatio = ArgumentVec2(enumerator,
                                    $"Aspect ratio parameter is inproperly set. Format: [x]:[y]");
                                break;
                            }
                        case "-scale":
                            {
                                scale = ArgumentVec2OrFloat(enumerator,
                                    $"Scale parameter is inproperly set. Format: [scale] or [scaleX],[scaleY]");
                                break;
                            }
                        case "-space":
                        case "-spacing":
                            {
                                space = ArgumentVec2OrFloat(enumerator,
                                    $"Scale parameter is inproperly set. Format: [space] or [spaceX],[spaceY]");
                                break;
                            }
                        case "-position":
                        case "-pos":
                            {
                                pos = ArgumentVec2(enumerator,
                                    $"Position parameter is inproperly set. Format: [x],[y]");
                                break;
                            }
                        case "-showafterinteraction":
                            {
                                showAfterInteraction = true;
                                break;
                            }
                        case "-padoffset":
                            {
                                padOffset = ArgumentVec2(enumerator,
                                    $"Pad offset parameter is inproperly set. Format: [x],[y]");
                                break;
                            }
                        case "-padcolor":
                            {
                                padColor = ArgumentColor(enumerator,
                                    $"Pad brake color parameter is inproperly set. Format: [r],[g],[b],[a]");
                                break;
                            }
                        case "-padbrakecolor":
                            {
                                padBrakeColor = ArgumentColor(enumerator,
                                    $"Pad brake color parameter is inproperly set. Format: [r],[g],[b],[a]");
                                break;
                            }
                        case "-padbackgroundcolor":
                            {
                                padBackgroundColor = ArgumentColor(enumerator,
                                    $"Pad background color parameter is inproperly set. Format: [r],[g],[b],[a]");
                                break;
                            }
                        case "-padstartpos":
                        case "-padstartposition":
                            {
                                padStartPosition = ArgumentVec3(enumerator,
                                    $"Pad start position parameter is inproperly set. Format: [x],[y],[z]");
                                break;
                            }
                        case "-padendpos":
                        case "-padendposition":
                            {
                                padEndPosition = ArgumentVec3(enumerator,
                                    $"Pad end position parameter is inproperly set. Format: [x],[y],[z]");
                                break;
                            }
                        case "-theme":
                            {
                                theme = ArgumentEnum<Theme>(enumerator, "Theme '{0}' does not exist.");
                                break;
                            }
                        case "-start":
                        case "-startoffset":
                            {
                                startOffset = ArgumentTimeSpan(enumerator);
                                break;
                            }
                        case "-adjusttofps":
                            {
                                adjustToFPS = true;
                                break;
                            }
                        case "-fps":
                            {
                                fps = ArgumentFloat(enumerator);
                                break;
                            }
                    }
                }

                if (!File.Exists(fileName))
                {
                    Console.WriteLine();
                    Console.WriteLine($"File '{fileName}' doesn't exist.");
                    Console.WriteLine();
                    PressAnyKeyToContinue();
                    return;
                }

                Console.WriteLine();
                Console.WriteLine($"Loading {Path.GetFileName(fileName)}...");

                var gbx = GameBox.Parse(fileName);

                var tool = new ClipInputTool(gbx);

                Console.WriteLine($"Fetching the configuration...");

                if (File.Exists(configYml))
                {
                    var config = YamlManager.Read<Config>(configYml);

                    if (config.AspectRatio is not null) tool.AspectRatio = (Vec2)config.AspectRatio;
                    if (config.Scale is not null) tool.Scale = (Vec2)config.Scale;
                    if (config.Space is not null) tool.Space = (Vec2)config.Space;
                    if (config.Position is not null) tool.Position = (Vec2)config.Position;
                    if (config.ShowAfterInteraction.HasValue) tool.ShowAfterInteraction = config.ShowAfterInteraction.Value;
                    if (config.PadOffset is not null) tool.PadOffset = (Vec2)config.PadOffset;
                    if (config.PadColor is not null) tool.PadColor = (Vec4)config.PadColor;
                    if (config.PadBrakeColor is not null) tool.PadBrakeColor = (Vec4)config.PadBrakeColor;
                    if (config.PadBackgroundColor is not null) tool.PadBackgroundColor = (Vec4)config.PadBackgroundColor;
                    if (config.PadStartPosition is not null) tool.PadStartPosition = (Vec3)config.PadStartPosition;
                    if (config.PadEndPosition is not null) tool.PadEndPosition = (Vec3)config.PadEndPosition;
                    if (config.Theme.HasValue) tool.Theme = config.Theme.Value;
                    if (config.StartOffset.HasValue) tool.StartOffset = TimeSpan.FromSeconds(config.StartOffset.Value);
                    if (config.AdjustToFPS.HasValue) tool.AdjustToFPS = config.AdjustToFPS.Value;
                    if (config.FPS.HasValue) tool.FPS = config.FPS.Value;
                    if (config.Keys is not null)
                    {
                        foreach (var key in config.Keys)
                        {
                            var k = new ClipInput.KeyboardKey();

                            if (key.EntryName is not null) k.EntryName = key.EntryName;
                            if (key.EntryNames is not null) k.EntryNames = key.EntryNames;
                            if (key.TrackName is not null) k.TrackName = key.TrackName;
                            if (key.Position is not null) k.Position = (Vec2)key.Position;
                            if (key.ImageOff is not null) k.ImageOff = key.ImageOff;
                            if (key.ImageOn is not null) k.ImageOn = key.ImageOn;
                            if (key.IsSteerInput.HasValue) k.IsSteerInput = key.IsSteerInput.Value;
                        }
                    }
                }

                if (aspectRatio.HasValue) tool.AspectRatio = aspectRatio.Value;
                if (scale.HasValue) tool.Scale = scale.Value;
                if (space.HasValue) tool.Space = space.Value;
                if (pos.HasValue) tool.Position = pos.Value;
                if (showAfterInteraction.HasValue) tool.ShowAfterInteraction = showAfterInteraction.Value;
                if (padOffset.HasValue) tool.PadOffset = padOffset.Value;
                if (padColor.HasValue) tool.PadColor = padColor.Value;
                if (padBrakeColor.HasValue) tool.PadBrakeColor = padBrakeColor.Value;
                if (padBackgroundColor.HasValue) tool.PadBackgroundColor = padBackgroundColor.Value;
                if (padStartPosition.HasValue) tool.PadStartPosition = padStartPosition.Value;
                if (padEndPosition.HasValue) tool.PadEndPosition = padEndPosition.Value;
                if (theme.HasValue) tool.Theme = theme.Value;
                if (startOffset.HasValue) tool.StartOffset = startOffset.Value;
                if (adjustToFPS.HasValue) tool.AdjustToFPS = adjustToFPS.Value;
                if (fps.HasValue) tool.FPS = fps.Value;

                Console.WriteLine();
                Console.WriteLine($"Beginning the process...");
                Console.WriteLine();

                try
                {
                    var output = tool.Process();

                    Console.WriteLine();
                    Console.WriteLine("Process has finished.");
                    Console.WriteLine();
                    Console.WriteLine("Reading Export.yml...");

                    var exportDictionary = YamlManager.Read<Dictionary<string, string>>(fileExportYml);

                    if (exportDictionary.TryGetValue(tool.GameVersion.ToString(), out string path) && path is not null)
                    {
                        Console.WriteLine("Creating necessary directories...");
                        Directory.CreateDirectory(path);
                    }
                    else
                    {
                        path = baseDirectory;
                    }

                    var clipFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fileName)) + ".Clip.Gbx";

                    var outputFileName = Path.Combine(path, clipFileName);

                    Console.WriteLine($"Saving into {clipFileName}...");

                    output.Save(outputFileName);

                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Done!");
                    Console.ResetColor();
                }
                catch (TMTurboNotSupportedException)
                {
                    Console.WriteLine("Trackmania Turbo replays are not supported.\nYou can still however look for the inputs with GBX.NET.\nhttps://github.com/BigBang1112/gbx-net");
                }
                catch (NoInputsException e)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }

                if (allArgsAreFiles)
                {
                    Console.WriteLine();
                    PressAnyKeyToContinue();
                }
            }
        }

        private static void Log_OnLogEvent(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void SetTitle(Version consoleVersion = null, Version clipInputVersion = null, Version gbxnetVersion = null)
        {
            string title;

            if (clipInputVersion is not null && gbxnetVersion is not null)
                title = $"Clip Input {clipInputVersion} (GBX.NET {gbxnetVersion}) (CLI {consoleVersion})";
            else if (consoleVersion is not null)
                title = $"Clip Input (CLI {consoleVersion})";
            else
                title = "Clip Input";

            title += " - a tool by BigBang1112";

            Console.Title = title;
        }

        public static void PressAnyKeyToContinue()
        {
            Console.Write("Press any key to continue... ");
            Console.ReadKey(true);
            Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
        }

        public static string GetUserDirectory(Game version, string installationDirectory)
        {
            if (!Directory.Exists(installationDirectory))
                throw new DirectoryNotFoundException($"Directory '{installationDirectory}' was not found.");

            var nadeoIni = Path.Combine(installationDirectory, "Nadeo.ini");

            if (!File.Exists(nadeoIni))
                throw new FileNotFoundException($"This directory is not the installation directory of {version}.");

            foreach (var line in File.ReadAllLines(nadeoIni))
            {
                switch (version)
                {
                    case Game.TMUF:
                        if (line.StartsWith("UserSubDir"))
                        {
                            var split = line.Split('=');
                            if (split.Length > 1)
                                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + split[1];
                            return null;
                        }
                        break;
                    case Game.ManiaPlanet:
                        if (line.StartsWith("UserDir"))
                        {
                            var split = line.Split('=');
                            if (split.Length > 1)
                                return split[1];
                            return null;
                        }
                        break;
                }
            }

            throw new Exception($"This directory is likely not the installation directory of {version}.");
        }

        public static float ArgumentFloat(IEnumerator<string> enumerator)
        {
            enumerator.MoveNext();
            var str = enumerator.Current;

            return float.Parse(str, CultureInfo.InvariantCulture);
        }

        public static TimeSpan ArgumentTimeSpan(IEnumerator<string> enumerator)
        {
            enumerator.MoveNext();
            var str = enumerator.Current;

            var offset = float.Parse(str, CultureInfo.InvariantCulture);

            return TimeSpan.FromSeconds(offset);
        }

        public static Vec2 ArgumentVec2(IEnumerator<string> enumerator, string exceptionMessage)
        {
            enumerator.MoveNext();
            var str = enumerator.Current;

            var split = str.Split(',');
            if (split.Length != 2)
                throw new FormatException(exceptionMessage);

            var x = float.Parse(split[0], CultureInfo.InvariantCulture);
            var y = float.Parse(split[1], CultureInfo.InvariantCulture);

            return (x, y);
        }

        public static Vec2 ArgumentVec2OrFloat(IEnumerator<string> enumerator, string exceptionMessage)
        {
            enumerator.MoveNext();
            var str = enumerator.Current;

            var split = str.Split(',');
            if (split.Length == 1)
            {
                var value = float.Parse(split[0], CultureInfo.InvariantCulture);
                return (value, value);
            }
            else if (split.Length == 2)
            {
                var x = float.Parse(split[0], CultureInfo.InvariantCulture);
                var y = float.Parse(split[1], CultureInfo.InvariantCulture);
                return (x, y);
            }
            else
                throw new FormatException(exceptionMessage);
        }

        public static Vec3 ArgumentVec3(IEnumerator<string> enumerator, string exceptionMessage)
        {
            enumerator.MoveNext();
            var str = enumerator.Current;

            var split = str.Split(',');
            if (split.Length != 3)
                throw new FormatException(exceptionMessage);

            var x = float.Parse(split[0], CultureInfo.InvariantCulture);
            var y = float.Parse(split[1], CultureInfo.InvariantCulture);
            var z = float.Parse(split[2], CultureInfo.InvariantCulture);

            return (x, y, z);
        }

        public static Vec4 ArgumentColor(IEnumerator<string> enumerator, string exceptionMessage)
        {
            enumerator.MoveNext();
            var str = enumerator.Current;

            var split = str.Split(',');
            if (split.Length != 4)
                throw new FormatException(exceptionMessage);

            var r = float.Parse(split[0], CultureInfo.InvariantCulture);
            var g = float.Parse(split[1], CultureInfo.InvariantCulture);
            var b = float.Parse(split[2], CultureInfo.InvariantCulture);
            var a = float.Parse(split[3], CultureInfo.InvariantCulture);

            return (r, g, b, a);
        }

        public static TEnum ArgumentEnum<TEnum>(IEnumerator<string> enumerator, string exceptionMessage) where TEnum : struct, Enum
        {
            enumerator.MoveNext();
            var str = enumerator.Current;

            if (Enum.TryParse(str, true, out TEnum t))
                return t;
            else
                throw new FormatException(string.Format(exceptionMessage, str));
        }
    }
}
