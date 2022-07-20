using System.Diagnostics;
using DotnetPatcher;
using DotnetPatcher.Decompile;
using DotnetPatcher.Diff;
using DotnetPatcher.Patch;
using DotnetPatcher.Utility;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

public class Program
{
    public const string HelpName = "-help";
    public const string HelpMessage = "For decompiling and patching Terraria,\n\tTerraAngelSetup -decompile -patch\n\nFor diffing Terraria,\n\tTerraAngelSetup -diff\n\n-di | -decompilerinput: Specifies the target Terraria path\n-do | -decompileroutput: Specifies the output path of the decompiler\n-pi | -patchinput: Specifies the input path of the patcher, aka the patches to use\n-po | -patchoutput: Specifies the path that the patcher will output to";

    public const string DecompilerInputName = "-di";
    public const string DecompilerInputNameLong = "-decompilerinput";

    public const string DecompilerOutputName = "-do";
    public const string DecompilerOutputNameLong = "-decompileroutput";

    public const string PatchesInputName = "-pi";
    public const string PatchesInputNameLong = "-patchinput";

    public const string PatchesOutputName = "-po";
    public const string PatchesOutputNameLong = "-patchoutput";

    public const string DecompileName = "-decompile";
    public const string PatchName = "-patch";
    public const string DiffName = "-diff";
    public const string AutoStartName = "-auto";
    public const string BuildDebugName = "-debug";
    public const string Buildx86Name = "-x86";
    public const string NoCopyName = "-nocopy";

    public static string TerrariaPath = @"C:\Program Files (x86)\Steam\steamapps\common\Terraria";
    public static string DecompilerOutputPath = @"src\Terraria";
    public static string PatchesPath = @"..\..\..\Patches\TerraAngelPatches";
    public static string PatchedPath = @"src\TerraAngel";

    public static bool Decomp = false;
    public static bool Patch = false;
    public static bool Diff = false;
    public static bool AutoStart = false;
    public static bool BuildDebug = false;
    public static bool Buildx86 = false;
    public static bool NoCopy = false;

    public static void Main(string[] args)
    {
        if (args.Length == 0 || (args.Length == 1 && args[0] == HelpName))
        {
            Console.WriteLine(HelpMessage);
            return;
        }
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            bool moreArgs = i < args.Length - 1;
            switch (arg)
            {
                case DecompilerInputName:
                case DecompilerInputNameLong:
                    if (moreArgs)
                    {
                        TerrariaPath = args[i + 1];
                    }
                    break;
                case DecompilerOutputName:
                case DecompilerOutputNameLong:
                    if (moreArgs)
                    {
                        DecompilerOutputPath = args[i + 1];
                    }
                    break;

                case PatchesInputName:
                case PatchesInputNameLong:
                    if (moreArgs)
                    {
                        PatchesPath = args[i + 1];
                    }
                    break;
                case PatchesOutputName:
                case PatchesOutputNameLong:
                    if (moreArgs)
                    {
                        PatchedPath = args[i + 1];
                    }
                    break;

                case DecompileName:
                    Decomp = true;
                    break;
                case PatchName:
                    Patch = true;
                    break;
                case DiffName:
                    Diff = true;
                    break;
                case AutoStartName:
                    AutoStart = true;
                    break;
                case BuildDebugName:
                    BuildDebug = true;
                    break;
                case Buildx86Name:
                    Buildx86 = true;
                    break;
                case NoCopyName:
                    NoCopy = true;
                    break;
            }
        }


        try
        {
            if (!Directory.Exists(TerrariaPath))
            {
                if (!SteamUtils.TryFindTerrariaDirectory(out TerrariaPath))
                {
                    Console.WriteLine("Could not find Terraria path");
                }
            }

            if (Decomp || AutoStart)
            {
                DecompileTerraria();
                FormatDir(DecompilerOutputPath);
            }
            if (Patch || AutoStart)
            {
                PatchTerraria();
            }
            if (Diff)
            {
                DiffTerraria();
            }
            if (AutoStart)
            {
                if (!Buildx86)
                {
                    if (BuildDebug)
                    {
                        Console.WriteLine("Building TerraAngel as debug x64");
                        ExecCommand(@$"dotnet build {PatchedPath}\Terraria\Terraria.csproj -p:Configuration=Debug;Platform=x64 > build_log_x64.txt");
                    }
                    else
                    {
                        Console.WriteLine("Building TerraAngel as release x64");
                        ExecCommand(@$"dotnet build {PatchedPath}\Terraria\Terraria.csproj -p:Configuration=Release;Platform=x64 > build_log_x64.txt");
                    }
                }
                else
                {
                    if (BuildDebug)
                    {
                        Console.WriteLine("Building TerraAngel as debug x86");
                        ExecCommand(@$"dotnet build {PatchedPath}\Terraria\Terraria.csproj -p:Configuration=Debug;Platform=x86 > build_log_x86.txt");
                    }
                    else
                    {
                        Console.WriteLine("Building TerraAngel as release x86");
                        ExecCommand(@$"dotnet build {PatchedPath}\Terraria\Terraria.csproj -p:Configuration=Release;Platform=x86 > build_log_x86.txt");
                    }
                }

                if (!NoCopy)
                {
                    if (Buildx86)
                    {
                        if (BuildDebug)
                        {
                            Console.WriteLine($"Copying \"{TerrariaPath}\\Content\" to \"src\\TerraAngel\\Terraria\\bin\\x86\\Debug\\net6.0\\Content\\\"");
                            ExecCommand($"xcopy \"{TerrariaPath}\\Content\\\" \"src\\TerraAngel\\Terraria\\bin\\x86\\Debug\\net6.0\\Content\\\" /E > NUL");
                        }
                        else
                        {
                            Console.WriteLine($"Copying \"{TerrariaPath}\\Content\" to \"src\\TerraAngel\\Terraria\\bin\\x86\\Release\\net6.0\\Content\\\"");
                            ExecCommand($"xcopy \"{TerrariaPath}\\Content\\\" \"src\\TerraAngel\\Terraria\\bin\\x86\\Release\\net6.0\\Content\\\" /E > NUL");
                        }
                    }
                    else
                    {
                        if (BuildDebug)
                        {
                            Console.WriteLine($"Copying \"{TerrariaPath}\\Content\" to \"src\\TerraAngel\\Terraria\\bin\\x64\\Debug\\net6.0\\Content\\\"");
                            ExecCommand($"xcopy \"{TerrariaPath}\\Content\\\" \"src\\TerraAngel\\Terraria\\bin\\x64\\Debug\\net6.0\\Content\\\" /E > NUL");
                        }
                        else
                        {
                            Console.WriteLine($"Copying \"{TerrariaPath}\\Content\" to \"src\\TerraAngel\\Terraria\\bin\\x64\\Release\\net6.0\\Content\\\"");
                            ExecCommand($"xcopy \"{TerrariaPath}\\Content\\\" \"src\\TerraAngel\\Terraria\\bin\\x64\\Release\\net6.0\\Content\\\" /E > NUL");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.ToString());
        }
    }
    public static void DecompileTerraria()
    {
        if (!Directory.Exists(TerrariaPath))
        {
            throw new DirectoryNotFoundException($"Directory '{TerrariaPath}' not found");
        }

        Console.WriteLine($"Decompiling {TerrariaPath}\\Terraria.exe into {DecompilerOutputPath}");

        Decompiler terrariaDecompiler = new Decompiler(TerrariaPath + "\\Terraria.exe", DecompilerOutputPath);

        terrariaDecompiler.Decompile(new string[] { "ReLogic" });

        Console.WriteLine($"Decompiled {TerrariaPath}\\Terraria.exe into {DecompilerOutputPath}");
    }
    public static void PatchTerraria()
    {
        if (!Directory.Exists(PatchesPath))
        {
            throw new DirectoryNotFoundException($"Directory '{PatchesPath}' not found");
        }

        Console.WriteLine($"Patching {PatchedPath} with {PatchesPath} into {PatchedPath}");

        Patcher terrariaPatcher = new Patcher(DecompilerOutputPath, PatchesPath, PatchedPath);

        terrariaPatcher.Patch();

        Console.WriteLine($"Patched {PatchedPath} with {PatchesPath} into {PatchedPath}");
    }
    public static void DiffTerraria()
    {
        if (!Directory.Exists(PatchesPath))
        {
            throw new DirectoryNotFoundException($"Directory '{PatchesPath}' not found");
        }

        if (!Directory.Exists(PatchedPath))
        {
            throw new DirectoryNotFoundException($"Directory '{PatchedPath}' not found");
        }

        Console.WriteLine($"Diffing {PatchedPath} with {DecompilerOutputPath} into {PatchesPath}");

        Differ terrariaDiffer = new Differ(DecompilerOutputPath, PatchesPath, PatchedPath);

        terrariaDiffer.Diff();

        Console.WriteLine($"Diffed {PatchedPath} with {DecompilerOutputPath} into {PatchesPath}");
    }
    public static void FormatDir(string dir)
    {
        Console.Write($"Formatting {dir}");

        AdhocWorkspace cw = new AdhocWorkspace();
        OptionSet options = cw.Options;
        options = options.WithChangedOption(CSharpFormattingOptions.IndentBraces, false);
        options = options.WithChangedOption(CSharpFormattingOptions.IndentBlock, true);
        options = options.WithChangedOption(new OptionKey(FormattingOptions.UseTabs, "CSharp"), false);

        List<WorkTask> tasks = new List<WorkTask>();
        foreach ((string file, string relPath) in DirectoryUtility.EnumerateSrcFiles(dir))
        {
            if (file.EndsWith(".cs"))
            {
                tasks.Add(new WorkTask(
                    () =>
                    {
                        string source = File.ReadAllText(file);
                        SyntaxNode formattedNode = Formatter.Format(CSharpSyntaxTree.ParseText(source).GetRoot().NormalizeWhitespace("    "), cw, options);
                        string text = formattedNode.GetText().ToString();
                        File.WriteAllText(file, text);
                    }));

            }
        }
        WorkTask.ExecuteParallel(tasks);
        Console.WriteLine($"Formatted {dir}");
    }
    public static void ExecCommand(string cmd)
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = $"/C {cmd}";
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }

    public static class SteamUtils
    {
        public const int TerrariaAppId = 105600;

        public readonly static string TerrariaManifestFile = $"appmanifest_{TerrariaAppId}.acf";

        private readonly static Regex SteamLibraryFoldersRegex = new(@"""(\d+)""[^\S\r\n]+""(.+)""", RegexOptions.Compiled);
        private readonly static Regex SteamManifestInstallDirRegex = new(@"""installdir""[^\S\r\n]+""([^\r\n]+)""", RegexOptions.Compiled);

        public static bool TryFindTerrariaDirectory(out string path)
        {
            if (TryGetSteamDirectory(out string steamDirectory) && TryGetTerrariaDirectoryFromSteam(steamDirectory, out path))
            {
                return true;
            }

            path = null;

            return false;
        }

        public static bool TryGetTerrariaDirectoryFromSteam(string steamDirectory, out string path)
        {
            string steamApps = Path.Combine(steamDirectory, "steamapps");

            var libraries = new List<string>() {
                steamApps
            };

            string libraryFoldersFile = Path.Combine(steamApps, "libraryfolders.vdf");

            if (File.Exists(libraryFoldersFile))
            {
                string contents = File.ReadAllText(libraryFoldersFile);

                var matches = SteamLibraryFoldersRegex.Matches(contents);

                foreach (Match match in matches)
                {
                    string directory = Path.Combine(match.Groups[2].Value.Replace(@"\\", @"\"), "steamapps");

                    if (Directory.Exists(directory))
                    {
                        libraries.Add(directory);
                    }
                }
            }

            for (int i = 0; i < libraries.Count; i++)
            {
                string directory = libraries[i];
                string manifestPath = Path.Combine(directory, TerrariaManifestFile);

                if (File.Exists(manifestPath))
                {
                    string contents = File.ReadAllText(manifestPath);
                    var match = SteamManifestInstallDirRegex.Match(contents);

                    if (match.Success)
                    {
                        path = Path.Combine(directory, "common", match.Groups[1].Value);

                        if (Directory.Exists(path))
                        {
                            return true;
                        }
                    }
                }
            }

            path = null;

            return false;
        }

        public static bool TryGetSteamDirectory(out string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = GetSteamDirectoryWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                path = "~/Library/Application Support/Steam";
            }
            else
            {
                path = "~/.local/share/Steam";
            }

            return path != null && Directory.Exists(path);
        }

        private static string GetSteamDirectoryWindows()
        {
            string keyPath = Environment.Is64BitOperatingSystem ? @"SOFTWARE\Wow6432Node\Valve\Steam" : @"SOFTWARE\Valve\Steam";

            using RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath);

            return (string)key.GetValue("InstallPath");
        }
    }
}
