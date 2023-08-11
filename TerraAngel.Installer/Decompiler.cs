using System.Data;
using System.Reflection.PortableExecutable;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;

namespace TerraAngel.Installer;

internal class Decompiler
{
    private static readonly string ReLogicDllName = "Terraria.Libraries.ReLogic.ReLogic.dll";

    public readonly string TargetAssembly;

    public readonly string OutputDirectory;

    private Decompiler(string targetAssembly, string outputDirectory)
    {
        TargetAssembly = targetAssembly;

        OutputDirectory = outputDirectory;

        // Clean up possible old source
        if (Directory.Exists(OutputDirectory))
        {
            Directory.Delete(OutputDirectory, true);
        }

        Directory.CreateDirectory(OutputDirectory);
        Directory.CreateDirectory(Path.Combine(OutputDirectory, "Terraria"));
        Directory.CreateDirectory(Path.Combine(OutputDirectory, "ReLogic"));
    }

    public void Decompile()
    {
        PEFile mainModule;

        using (FileStream fs = new FileStream(TargetAssembly, FileMode.Open))
        {
            mainModule = new PEFile(TargetAssembly, fs, PEStreamOptions.PrefetchEntireImage);
        }

        Resource relogicResource = mainModule.Resources.Where(x => x.ResourceType == ResourceType.Embedded).Single(x => x.Name == ReLogicDllName);
        PEFile relogicModule = new PEFile("Terraria.Libraries.ReLogic.ReLogic.dll", relogicResource.TryOpenStream()!, streamOptions: PEStreamOptions.PrefetchEntireImage);

        EmbeddedAssemblyResolver assemblyResolver = new EmbeddedAssemblyResolver(mainModule);

        WholeProjectDecompiler projectDecompiler = new WholeProjectDecompiler(assemblyResolver)
        {
            MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1)
        };

        projectDecompiler.Settings.UseNestedDirectoriesForNamespaces = true;
        projectDecompiler.Settings.UseSdkStyleProjectFormat = true;
        projectDecompiler.Settings.CSharpFormattingOptions = FormattingOptionsFactory.CreateAllman();

        projectDecompiler.ProgressIndicator = new ConsoleProgressReporter();

        projectDecompiler.DecompileProject(mainModule, Path.Combine(OutputDirectory, "Terraria"));

        projectDecompiler.ProgressIndicator = new ConsoleProgressReporter();

        projectDecompiler.DecompileProject(relogicModule, Path.Combine(OutputDirectory, "ReLogic"));
    }

    public static void Decompile(string targetAssembly, string outputDirectory)
    {
        Decompiler decompiler = new Decompiler(targetAssembly, outputDirectory);

        decompiler.Decompile();
    }

    sealed class ConsoleProgressReporter : IProgress<DecompilationProgress>
    {
        private string? Title;

        private bool Updating;

        private float LastPercent;

        public void Report(DecompilationProgress value)
        {
            if (Updating)
            {
                return;
            }

            Updating = true;

            if (value.Title != Title)
            {
                Title = value.Title;

                Console.WriteLine();
                Console.WriteLine($"-- {Title} --");
                Console.WriteLine($"0%  Completed");
            }

            float percent = value.UnitsCompleted / (float)value.TotalUnits;

            // Only print percentage once it has changed by > 1%
            if (MathF.Abs(LastPercent - percent) > 0.01f)
            {
                Console.WriteLine($"{$"{percent * 100f:F0}%",-3} Completed");
                LastPercent = percent;
            }

            Updating = false;
        }
    }
}
