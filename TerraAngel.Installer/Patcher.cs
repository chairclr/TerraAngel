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

    }

    public static void Patch(string sourceDirectory, string patchDirectory, string outputDirectory)
    {
        Patcher patcher = new Patcher(sourceDirectory, patchDirectory, outputDirectory);

        patcher.Patch();
    }
}
