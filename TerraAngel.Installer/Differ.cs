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
    }

    public void Diff()
    {

    }

    public static void Diff(string sourceDirectory, string targetDirectory, string outputDirectory)
    {
        Differ differ = new Differ(sourceDirectory, targetDirectory, outputDirectory);

        differ.Diff();
    }
}
