using DiffMatchPatch;

namespace TerraAngel.Installer;

internal class Differ
{
    public readonly string SourceDirectory;

    public readonly string TargetDirectory;

    public readonly string OutputDirectory;

    private Differ(string sourceDirectory, string targetDirectory, string outputDirectory)
    {
        SourceDirectory = sourceDirectory;

        TargetDirectory = targetDirectory;

        OutputDirectory = outputDirectory;

        if (Directory.Exists(OutputDirectory))
        {
            Directory.Delete(OutputDirectory, true);
        }
    }

    public void Diff()
    {
        diff_match_patch dmp = new diff_match_patch();

        // 42 solves all problems
        dmp.Patch_Margin = 42;

        List<Action> workActions = new List<Action>();

        foreach (LocalPath path in DirectoryUtility.EnumerateFiles(TargetDirectory).Where(x => !x.RelativePath.Split('/', '\\').Any(x => x == "bin" || x == "obj" || x == ".vs")))
        {
            string assumedSourcePath = Path.Combine(SourceDirectory, path.RelativePath);

            if (File.Exists(assumedSourcePath))
            {
                workActions.Add(() =>
                {
                    List<Patch> diffs = dmp.patch_make(File.ReadAllText(assumedSourcePath), File.ReadAllText(path.FullPath));

                    if (diffs.Count != 0)
                    {
                        string outputPath = Path.Combine(OutputDirectory, $"{path.RelativePath}.patch");

                        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

                        File.WriteAllText(outputPath, dmp.patch_toText(diffs));
                    }
                });
            }
            else
            {
                workActions.Add(() =>
                {
                    DirectoryUtility.CopyFile(path.FullPath, Path.Combine(OutputDirectory, path.RelativePath));
                });
            }
        }

        Parallel.Invoke(workActions.ToArray());

        string[] removedFiles = DirectoryUtility.EnumerateFiles(SourceDirectory)
            .Where(x => !x.RelativePath.Split('/', '\\').Any(x => x == "bin" || x == "obj" || x == ".vs"))
            .Where(x => !File.Exists(Path.Combine(TargetDirectory, x.RelativePath)))
            .Select(x => x.RelativePath)
            .ToArray();

        if (removedFiles.Length > 0)
        {
            File.WriteAllLines(Path.Combine(OutputDirectory, "removed_files.txt"), removedFiles);
        }
    }

    public static void Diff(string sourceDirectory, string targetDirectory, string outputDirectory)
    {
        Differ differ = new Differ(sourceDirectory, targetDirectory, outputDirectory);

        differ.Diff();
    }
}
