using System.IO;

namespace TerraAngel.Utility;

public class DirectoryUtility
{
    /// <returns>Whether or not the directory was created</returns>
    public static bool TryCreateDirectory(string? dir)
    {
        if (dir is null)
        {
            return false;
        }

        if (Directory.Exists(dir))
        {
            return false;
        }

        Directory.CreateDirectory(dir);

        return true;
    }

    /// <returns>Whether or not the directory was created</returns>
    public static void TryCreateParentDirectory(string? path) => TryCreateDirectory(Path.GetDirectoryName(path));
}
