﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace TerraAngel.Installer;

internal class Program
{
    public class SetupSettings
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

        if (args.Length == 0)
        {
            List<string> sdks = SDKUtility.GetDotnetSDKList();

            if (!sdks.Any(x => x.StartsWith("8.")))
            {
                Console.WriteLine("TerraAngel requires the .NET 8 SDK");
                Console.WriteLine("Install the .NET 8 SDK here -> https://dotnet.microsoft.com/en-us/download/dotnet/8.0");
                Console.WriteLine("Restart your computer after you do this, and re-run the installer");

                return;
            }

            HandleSelfContainedInstallation();
        }
        else
        {
            ParserResult<SetupSettings> result = Parser.Default.ParseArguments<SetupSettings>(args);

            result.WithParsed(RunSetup);
        }
    }

    private static void RunSetup(SetupSettings settings)
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
            Console.WriteLine();
        }

        if (settings.Diff)
        {
            Console.WriteLine($"Diffing {settings.DiffSourcePath} with {settings.DiffTargetPath} into {settings.DiffOutputPath}");

            Stopwatch sw = Stopwatch.StartNew();
            Differ.Diff(settings.DiffSourcePath, settings.DiffTargetPath, settings.DiffOutputPath);
            sw.Stop();

            Console.WriteLine($"Finished Diffing. Elapsed: {sw.Elapsed.TotalSeconds}s");
            Console.WriteLine();
        }

        if (settings.Patch)
        {
            Console.WriteLine($"Patching {settings.PatchSourcePath} with {settings.PatchDiffPath} into {settings.PatchOutputPath}");

            Stopwatch sw = Stopwatch.StartNew();
            Patcher.Patch(settings.PatchSourcePath, settings.PatchDiffPath, settings.PatchOutputPath);
            sw.Stop();

            Console.WriteLine($"Finished Patching. Elapsed: {sw.Elapsed.TotalSeconds}s");
            Console.WriteLine();
        }
    }

    private static void HandleSelfContainedInstallation()
    {
        Console.WriteLine("Checking for updates");

        Console.WriteLine("Retrieving latest release from github.com/chairclr/TerraAngel...");

        ReleaseDownloader.ReleaseRoot release = ReleaseDownloader.GetLatestRelease().Result;

        if (Version.TryParse(release.TagName, out Version? latestReleaseVersion))
        {
            Console.WriteLine($"Got latest release: v{latestReleaseVersion}");

            Console.WriteLine("Looking for previous install...");

            if (TryGetPreviousInstallation(out Installation previousInstall))
            {
                Console.WriteLine($"Found previous installation: v{previousInstall}");

                if (latestReleaseVersion > previousInstall.Version)
                {
                    HandleUpdate(release, latestReleaseVersion, previousInstall);
                }
            }
            else
            {
                HandleNewInstallation(release, latestReleaseVersion);
            }
        }
        else
        {
            Console.WriteLine("Unable to retrieve latest release version");
        }
    }

    private static void HandleNewInstallation(ReleaseDownloader.ReleaseRoot release, Version latestReleaseVersion)
    {
        Console.WriteLine("No previous installation found");

        Console.Write($"Would you like to install TerraAngel v{latestReleaseVersion}? (y/n): ");

        while (true)
        {
            bool validResult = false;

            switch (Console.ReadLine()?.ToLowerInvariant())
            {
                case "y":
                case "yes":
                case "sure":
                case "yeah":
                    {
                        Console.WriteLine($"Setting up temporary files");

                        string tempDir = Path.Combine(Path.GetTempPath(), "TerraAngel");

                        if (Directory.Exists(tempDir))
                        {
                            Directory.Delete(tempDir, true);
                        }

                        Directory.CreateDirectory(tempDir);

                        string releaseDir = DownloadRelease(release, latestReleaseVersion);

                        string decompDir = Path.Combine(tempDir, "decomp");

                        string patchOutputDir = Path.Combine(tempDir, "src");

                        Console.WriteLine("Setting up TerraAngel source");

                        RunSetup(new SetupSettings()
                        {
                            Decompile = true,
                            DecompilationOutputDirectory = decompDir,
                            Patch = true,
                            PatchDiffPath = Path.Combine(releaseDir, "TerraAngel.Patches"),
                            PatchSourcePath = decompDir,
                            PatchOutputPath = patchOutputDir
                        });

                        string buildDir = Path.Combine(tempDir, "build");

                        Console.WriteLine("Building TerraAngel");
                        Console.WriteLine("This could take a while... (usually less than 30 seconds)");

                        Stopwatch sw = Stopwatch.StartNew();

                        bool buildSucceeded = SDKUtility.PublishProject(Path.Combine(patchOutputDir, "Terraria", "Terraria.csproj"), buildDir);

                        sw.Stop();

                        if (!buildSucceeded)
                        {
                            return;
                        }

                        Console.WriteLine($"Build succeeded in {sw.Elapsed.TotalSeconds}s");

                        Console.WriteLine($"Your build is located in: {buildDir}");
                    }
                    validResult = true;
                    break;
                case "n":
                case "no":
                case "nope":
                case "nah":
                    validResult = true;
                    break;
            }

            if (validResult)
            {
                break;
            }
        }
    }

    private static void HandleUpdate(ReleaseDownloader.ReleaseRoot release, Version latestReleaseVersion, Installation previousInstall)
    {
        Console.WriteLine($"A new release is available (published {release.PublishedAt.ToLocalTime():D})!");
        Console.WriteLine($"v{previousInstall.Version} -> v{latestReleaseVersion}");

        Console.Write($"Would you like to update to v{latestReleaseVersion}? (y/n): ");

        while (true)
        {
            bool validResult = false;

            switch (Console.ReadLine()?.ToLowerInvariant())
            {
                case "y":
                case "yes":
                case "sure":
                case "yeah":
                    {
                        throw new NotImplementedException();
                    }
                    validResult = true;
                    break;
                case "n":
                case "no":
                case "nope":
                case "nah":
                    validResult = true;
                    break;
            }

            if (validResult)
            {
                break;
            }
        }
    }

    private static string DownloadRelease(ReleaseDownloader.ReleaseRoot release, Version latestReleaseVersion)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "TerraAngel");

        Directory.CreateDirectory(tempDir);

        Console.WriteLine($"Downloading release {latestReleaseVersion}");

        string downloadedPath = ReleaseDownloader.DownloadRelease(release, tempDir).Result;

        Console.WriteLine($"Finished downloading release");

        return downloadedPath;
    }

    private static bool TryGetPreviousInstallation([NotNullWhen(true)] out Installation installation)
    {
        installation = new Installation();

        if (File.Exists(Path.Combine(PathUtility.TerraAngelDataPath, "INSTALL.txt")))
        {
            string[] text = File.ReadAllLines(Path.Combine(PathUtility.TerraAngelDataPath, "VERSION.txt"));

            if (text.Length < 2)
            {
                return false;
            }

            if (Version.TryParse(text[0], out Version? lastInstalledVersion) && Directory.Exists(text[1]))
            {
                installation = new Installation(lastInstalledVersion, text[1]);
                return true;
            }
        }

        return false;
    }

    private readonly record struct Installation(Version Version, string InstallationRoot);
}
