using System.Diagnostics;
using CommandLine;

namespace TerraAngel.Installer;

internal class Program
{
    public class InstallSettings
    {
        [Option('d', "decompile", Required = false, HelpText = "Whether or not to decompile Terraria source")]
        public bool Decompile { get; set; }

        [Option('t', "decomp-target", Required = false, HelpText = "Target Terraria executable for decompilation.")]
        public string? DecompilationTarget { get; set; }

        [Option('o', "decomp-output", Required = false, HelpText = "Output directory for decompilation.")]
        public string DecompilationOutputDirectory { get; set; } = Path.Combine(Environment.CurrentDirectory, "temp", "decomp");

        [Option('f', "diff", Required = false, HelpText = "Whether or not to create diffs")]
        public bool Diff { get; set; }

        [Option("diff-source", Required = false, HelpText = "Path to source material for diffing")]
        public string DiffSourcePath { get; set; } = Path.Combine(Environment.CurrentDirectory, "temp", "decomp");

        [Option("diff-target", Required = false, HelpText = "Path to modified material for diffing")]
        public string DiffTargetPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "temp", "src");

        [Option("diff-output", Required = false, HelpText = "Path to output directory for diffs")]
        public string DiffOutputPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "TerraAngel.Patches");

        [Option('p', "patch", Required = false, HelpText = "Whether or not to apply patches")]
        public bool Patch { get; set; }

        [Option("patch-source", Required = false, HelpText = "Path to source material to patch")]
        public string PatchSourcePath { get; set; } = Path.Combine(Environment.CurrentDirectory, "temp", "decomp");

        [Option("patch-diffs", Required = false, HelpText = "Path to patches to apply")]
        public string PatchDiffPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "TerraAngel.Patches");

        [Option("patch-output", Required = false, HelpText = "Path to output for patched source")]
        public string PatchOutputPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "temp", "src");
    }

    private static void Main(string[] args)
    {
        ParserResult<InstallSettings> result = Parser.Default.ParseArguments<InstallSettings>(args);

        result.WithParsed(Run);
    }

    private static void Run(InstallSettings settings)
    {
        if (settings.DecompilationTarget is null)
        {
            if (PathUtility.TryGetTerrariaInstall(out string? terrariaInstall))
            {
                settings.DecompilationTarget = terrariaInstall;
            }
        }

        if (settings.Decompile)
        {
            while (!File.Exists(settings.DecompilationTarget))
            {
                Console.WriteLine($"Terraria executable path not found");
                Console.WriteLine($"Please enter exact path to target Terraria executable");
                settings.DecompilationTarget = Console.ReadLine();
            }

            Console.WriteLine($"Target Terraria executable: {settings.DecompilationTarget}");

            Console.WriteLine($"Decompiling {settings.DecompilationTarget} into {settings.DecompilationOutputDirectory}");

            Stopwatch sw = Stopwatch.StartNew();
            Decompiler.Decompile(settings.DecompilationTarget, settings.DecompilationOutputDirectory);
            sw.Stop();

            Console.WriteLine($"Finished Decompiling. Elapsed: {sw.Elapsed.TotalSeconds}s");
        }

        if (settings.Diff)
        {
            Console.WriteLine($"Diffing {settings.DiffSourcePath} with {settings.DiffTargetPath} into {settings.DiffOutputPath}");

            Stopwatch sw = Stopwatch.StartNew();
            Differ.Diff(settings.DiffSourcePath, settings.DiffTargetPath, settings.DiffOutputPath);
            sw.Stop();

            Console.WriteLine($"Finished Diffing. Elapsed: {sw.Elapsed.TotalSeconds}s");
        }

        if (settings.Patch)
        {
            Console.WriteLine($"Patching {settings.PatchSourcePath} with {settings.PatchDiffPath} into {settings.PatchOutputPath}");

            Stopwatch sw = Stopwatch.StartNew();
            Patcher.Patch(settings.PatchSourcePath, settings.PatchDiffPath, settings.PatchOutputPath);
            sw.Stop();

            Console.WriteLine($"Finished Patching. Elapsed: {sw.Elapsed.TotalSeconds}s");
        }
    }
}
