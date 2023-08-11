namespace TerraAngel.Installer;

internal static class DirectoryUtility
{
    public static string GetRelativePath(string basePath, string path)
    {
        if (path.Last() == Path.DirectorySeparatorChar)
        {
            path = path[..^1];
        }

        if (basePath.Last() != Path.DirectorySeparatorChar)
        {
            basePath += Path.DirectorySeparatorChar;
        }

        if (path + Path.DirectorySeparatorChar == basePath)
        {
            return "";
        }

        if (!path.StartsWith(basePath))
        {
            path = Path.GetFullPath(path);
            basePath = Path.GetFullPath(basePath);
        }

        if (!path.StartsWith(basePath))
        {
            throw new ArgumentException($"Path \"{path}\" is not relative to \"{basePath}\"");
        }

        return path[basePath.Length..];
    }

    public static IEnumerable<LocalPath> EnumerateFiles(string dir)
    {
        return Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Select(path => new LocalPath(path, GetRelativePath(dir, path)));
    }

    public static void CopyFile(string from, string to)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(to)!);

        if (File.Exists(to))
        {
            File.SetAttributes(to, FileAttributes.Normal);
        }

        File.Copy(from, to, true);
    }
}

internal record struct LocalPath(string FullPath, string RelativePath);