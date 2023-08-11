using DiffMatchPatch;

namespace TerraAngel.Installer;

internal class Patcher
{
    public readonly string SourceDirectory;

    public readonly string PatchDirectory;

    public readonly string OutputDirectory;

    private Patcher(string sourceDirectory, string patchDirectory, string outputDirectory)
    {
        SourceDirectory = sourceDirectory;

        PatchDirectory = patchDirectory;

        OutputDirectory = outputDirectory;
    }

    public void Patch()
    {
        diff_match_patch dmp = new diff_match_patch();

        List<Action> workActions = new List<Action>();

        List<string> alreadyPatched = new List<string>();

        int successCount = 0;

        int failureCount = 0;

        foreach (LocalPath path in DirectoryUtility.EnumerateFiles(PatchDirectory))
        {
            if (path.RelativePath.EndsWith(".patch"))
            {
                string regularPathName = path.RelativePath[..^6];

                string sourcePath = Path.Combine(SourceDirectory, regularPathName);
                string outputPath = Path.Combine(OutputDirectory, regularPathName);

                if (File.Exists(sourcePath))
                {
                    workActions.Add(() =>
                    {
                        List<Patch> patches = dmp.patch_fromText(File.ReadAllText(path.FullPath));

                        object[] rawPatchOutput = dmp.patch_apply(patches, File.ReadAllText(sourcePath));

                        (string patchedText, bool[] success) = ((string)rawPatchOutput[0], (bool[])rawPatchOutput[1]);

                        successCount += success.Count(x => x);
                        failureCount += success.Count(x => !x);

                        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                        File.WriteAllText(outputPath, patchedText);

                        alreadyPatched.Add(regularPathName);
                    });
                }
                else
                {
                    Console.WriteLine("Error during patching:");
                    Console.WriteLine($"File {path.RelativePath} doesn't exist in source (probably removed)");
                }
            }
        }

        foreach (LocalPath path in DirectoryUtility.EnumerateFiles(SourceDirectory))
        {
            if (alreadyPatched.Contains(path.RelativePath))
            {
                continue;
            }

            workActions.Add(() =>
            {
                DirectoryUtility.CopyFile(path.FullPath, Path.Combine(OutputDirectory, path.RelativePath));
            });
        }

        Parallel.Invoke(workActions.ToArray());

        foreach (string file in File.ReadAllLines(Path.Combine(PatchDirectory, "removed_files.txt")))
        {
            File.Delete(Path.Combine(OutputDirectory, file));
        }

        int total = successCount + failureCount;

        float failureRate = failureCount / (float)total;

        Console.WriteLine("Patch Statistics:");
        Console.WriteLine($"{total} Patches applied, {successCount} succeeded");

        if (failureCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        Console.WriteLine($"{failureRate * 100f:F2}% Failed");
        Console.ResetColor();
    }

    public static void Patch(string sourceDirectory, string patchDirectory, string outputDirectory)
    {
        Patcher patcher = new Patcher(sourceDirectory, patchDirectory, outputDirectory);

        patcher.Patch();
    }
}
