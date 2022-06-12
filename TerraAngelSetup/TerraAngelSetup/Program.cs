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

    public const string CompilerOutputName = "-co";
    public const string CompilerOutputNameLong = "-compileroutput";

    public const string DecompileName = "-decompile";
    public const string PatchName = "-patch";
    public const string DiffName = "-diff";
    public const string PrebuildName = "-prebuild";
    public const string AutoStartName = "-auto";
    public const string BuildDebugName = "-debug";

    public static string TerrariaPath = @"C:\Program Files (x86)\Steam\steamapps\common\Terraria\Terraria.exe";
    public static string DecompilerOutputPath = @"src\Terraria";
    public static string PatchesPath = @"..\..\..\Patches\TerraAngelPatches";
    public static string PatchedPath = @"src\TerraAngel";
    public static string CompiledPath = @"src\TerraAngel\Terraria\bin\Release\net6.0";

    public static bool Decomp = false;
    public static bool Patch = false;
    public static bool Diff = false;
    public static bool Prebuild = false;
    public static bool AutoStart = false;
    public static bool BuildDebug = false;

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
                case CompilerOutputName:
                case CompilerOutputNameLong:
                    if (moreArgs)
                    {
                        CompiledPath = args[i + 1];
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
                case PrebuildName:
                    Prebuild = true;
                    break;
                case AutoStartName:
                    AutoStart = true;
                    break;
                case BuildDebugName:
                    BuildDebug = true;
                    break;
            }
        }

        if (BuildDebug)
        {
            CompiledPath.Replace("Release", "Debug");
        }

        try
        {
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
            if (Prebuild)
            {
                CopyTerrariaThings();
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
        if (!File.Exists(TerrariaPath))
        {
            throw new FileNotFoundException($"File '{TerrariaPath}' not found");
        }

        Console.WriteLine($"Decompiling {TerrariaPath} into {DecompilerOutputPath}");

        Decompiler terrariaDecompiler = new Decompiler(TerrariaPath, DecompilerOutputPath);

        terrariaDecompiler.Decompile(new string[] { "ReLogic" });

        Console.WriteLine($"Decompiled {TerrariaPath} into {DecompilerOutputPath}");
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
    public static void CopyTerrariaThings()
    {
        string dirName = Path.GetDirectoryName(TerrariaPath) + "\\Content";
        if (!Directory.Exists(dirName))
            throw new DirectoryNotFoundException($"Directory '{TerrariaPath}' not found");

        if (!Directory.Exists(CompiledPath))
            throw new DirectoryNotFoundException($"Directory '{CompiledPath}' not found");

        Console.WriteLine($"Copying {Path.GetFullPath(dirName)} into {Path.GetFullPath(CompiledPath)}");

        CopyDirectory(Path.GetFullPath(dirName), Path.GetFullPath(CompiledPath) + "\\Content", true);

        Console.WriteLine($"Copyed {Path.GetFullPath(dirName)} into {Path.GetFullPath(CompiledPath)}");
    }
    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        DirectoryInfo[] dirs = dir.GetDirectories();

        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
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
}
