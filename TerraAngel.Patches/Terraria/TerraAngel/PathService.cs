using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using Terraria;

namespace TerraAngel;

public static class PathService
{
    private static readonly string TerrariaAssemblyPath = typeof(Main).Assembly.Location;

    private static readonly string TerrariaAppId = "105600";

    private static readonly string TerrariaAppManifest = $"appmanifest_{TerrariaAppId}.acf";

    public static string TerrariaPath => File.Exists(TerrariaAssemblyPath) ? Path.GetDirectoryName(TerrariaAssemblyPath)! : Environment.CurrentDirectory;

    public static readonly string TerraAngelDataPath;

    public static string PluginsRootFolder => Path.Combine(TerraAngelDataPath, "Plugins");

    public static string ContentFolder
    {
        get
        {
            string localCopyFolder = Path.Combine(TerrariaPath, "Content");
            if (Directory.Exists(localCopyFolder))
            {
                return localCopyFolder;
            }

            string defaultContentPath = @"C:\Program Files (x86)\Steam\steamapps\common\Terraria\Content";
            if (Directory.Exists(defaultContentPath))
            {
                return defaultContentPath;
            }

            if (TryGetTerrariaInstall(out string? foundSteamPath))
            {
                foundSteamPath = Path.Combine(Path.GetDirectoryName(foundSteamPath)!, "Content");
                if (Directory.Exists(foundSteamPath))
                {
                    return foundSteamPath;
                }
            }

            throw new FileNotFoundException("Could not find Content folder. Try copying the Content folder from your Terraria install to where the client is.");
        }
    }

    static PathService()
    {
        TerraAngelDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TerraAngel");

        Directory.CreateDirectory(TerraAngelDataPath);
    }

    private static bool TryGetTerrariaInstall([NotNullWhen(true)] out string? terrariaInstall)
    {
        if (TryGetSteamDirectory(out string? steamDirectory))
        {
            if (TryGetTerrariaDirectory(steamDirectory, out terrariaInstall))
            {
                return true;
            }
        }

        // Install not found from steam
        terrariaInstall = null;

        return false;
    }

    private static bool TryGetTerrariaDirectory(string steamDirectory, [NotNullWhen(true)] out string? path)
    {
        string steamapps = Path.Combine(steamDirectory, "steamapps");

        HashSet<string> libraries = new HashSet<string>()
        {
            steamapps
        };

        string libraryfoldersPath = Path.Combine(steamapps, "libraryfolders.vdf");

        if (File.Exists(libraryfoldersPath))
        {
            dynamic libraryFolders = VdfConvert.Deserialize(File.ReadAllText(libraryfoldersPath));

            foreach (dynamic libraryFolder in libraryFolders.Value)
            {
                bool containsTerrariaAppid = false;

                // Only add libraries that contain the terraria steam appid
                foreach (VProperty appid in libraryFolder.Value.apps)
                {
                    if (appid.Key == TerrariaAppId)
                    {
                        containsTerrariaAppid = true;
                        break;
                    }
                }

                if (containsTerrariaAppid)
                {
                    libraries.Add(Path.Combine((string)libraryFolder.Value.path.Value, "steamapps"));
                }
            }
        }

        foreach (string library in libraries)
        {
            string possibleManifest = Path.Combine(library, TerrariaAppManifest);

            if (File.Exists(possibleManifest))
            {
                dynamic manifest = VdfConvert.Deserialize(File.ReadAllText(possibleManifest));

                if ((string)manifest.Value.appid.Value == TerrariaAppId)
                {
                    path = Path.Combine(library, "common", (string)manifest.Value.installdir.Value, "Terraria.exe");

                    return true;
                }
            }
        }

        path = null;

        return false;
    }

    private static bool TryGetSteamDirectory([NotNullWhen(true)] out string? path)
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

    [SupportedOSPlatform("windows")]
    private static string? GetSteamDirectoryWindows()
    {
        string keyPath = Environment.Is64BitOperatingSystem ? @"SOFTWARE\Wow6432Node\Valve\Steam" : @"SOFTWARE\Valve\Steam";

        using RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath);

        return (string?)key.GetValue("InstallPath");
    }
}
