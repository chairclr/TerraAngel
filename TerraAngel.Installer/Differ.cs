using System.Security.Cryptography;
using DiffMatchPatch;
using ICSharpCode.Decompiler.Metadata;

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

        List<Action> workActions = new List<Action>();

        foreach (LocalPath path in DirectoryUtility.EnumerateFiles(TargetDirectory).Where(x => !x.RelativePath.Split('/', '\\').Any(x => x == "bin" || x == "obj" || x == ".vs")))
        {
            string assumedSourcePath = Path.Combine(SourceDirectory, path.RelativePath);

            if (File.Exists(assumedSourcePath))
            {
                workActions.Add(() =>
                {
                    List<Diff> diffs = dmp.diff_main(File.ReadAllText(assumedSourcePath), File.ReadAllText(path.FullPath));

                    bool noDifference = diffs.Count == 1 && diffs.Single().operation == Operation.EQUAL;

                    if (!noDifference)
                    { 
                        string text = dmp.diff_toDelta(diffs);

                        string outputPath = Path.Combine(OutputDirectory, $"{path.RelativePath}.patch");

                        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

                        File.WriteAllText(outputPath, text);
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
