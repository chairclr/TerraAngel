using CommandLine;
using ICSharpCode.Decompiler.Util;

namespace TerraAngel.Installer;

internal class Program
{
    public class InstallSettings
    {
        [Option('d', "decomp-target", Required = false, HelpText = "Target Terraria executable for decompilation.")]
        public string? DecompilationTarget { get; set; }


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

        while (!File.Exists(settings.DecompilationTarget))
        {
            Console.WriteLine($"Terraria executable path not found");
            Console.WriteLine($"Please enter exact path to target Terraria executable");
            settings.DecompilationTarget = Console.ReadLine();
        }

        Console.WriteLine($"Target Terraria executable: {settings.DecompilationTarget}");
    }
}
