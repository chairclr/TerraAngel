using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace TerraAngel.Utility;

public static class SteamLocator
{
    private static readonly int TerrariaAppId = 105600;

    private readonly static string TerrariaManifestFile = $"appmanifest_{TerrariaAppId}.acf";

    private readonly static Regex SteamLibraryFoldersRegex = new Regex(@"^\s+""(path)""\s+""(.+)""", RegexOptions.Compiled);

    private readonly static Regex SteamManifestInstallDirRegex = new Regex(@"""installdir""[^\S\r\n]+""([^\r\n]+)""", RegexOptions.Compiled);

    public static bool TryFindTerrariaDirectory(out string? path)
    {
        if (TryGetSteamDirectory(out string? steamDirectory) && TryGetTerrariaDirectoryFromSteam(steamDirectory!, out path))
        {
            return true;
        }

        path = null;

        return false;
    }

    public static bool TryGetTerrariaDirectoryFromSteam(string steamDirectory, out string? path)
    {
        string steamApps = Path.Combine(steamDirectory, "steamapps");

        List<string> libraries = new List<string>
        {
            steamApps
        };

        string libraryFoldersFile = Path.Combine(steamApps, "libraryfolders.vdf");

        if (File.Exists(libraryFoldersFile))
        {
            string[] contents = File.ReadAllLines(libraryFoldersFile);

            for (int i = 0; i < contents.Length; i++)
            {
                MatchCollection matches = SteamLibraryFoldersRegex.Matches(contents[i]);

                foreach (Match match in matches)
                {
                    string directory = Path.Combine(match.Groups[2].Value.Replace(@"\\", @"\"), "steamapps");

                    if (Directory.Exists(directory))
                    {
                        libraries.Add(directory);
                    }
                }
            }
        }

        for (int i = 0; i < libraries.Count; i++)
        {
            string directory = libraries[i];
            string manifestPath = Path.Combine(directory, TerrariaManifestFile);

            if (File.Exists(manifestPath))
            {
                string contents = File.ReadAllText(manifestPath);
                var match = SteamManifestInstallDirRegex.Match(contents);

                if (match.Success)
                {
                    path = Path.Combine(directory, "common", match.Groups[1].Value);

                    if (Directory.Exists(path))
                    {
                        return true;
                    }
                }
            }
        }

        path = null;

        return false;
    }

    public static bool TryGetSteamDirectory(out string? path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            path = GetSteamDirectoryWindows();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            path = "~/Library/Application Support/Steam";
        }
        else
        {
            path = "~/.local/share/Steam";
        }

        return path != null && Directory.Exists(path);
    }

    [SupportedOSPlatform("Windows")]
    private static string? GetSteamDirectoryWindows()
    {
        string keyPath = Environment.Is64BitOperatingSystem ? @"SOFTWARE\Wow6432Node\Valve\Steam" : @"SOFTWARE\Valve\Steam";

        using RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath);

        return (string?)key.GetValue("InstallPath");
    }
}
