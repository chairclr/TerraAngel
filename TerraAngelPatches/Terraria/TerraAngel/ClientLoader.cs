using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DiscordRPC;
using ReLogic.OS;
using TerraAngel.Client;
using TerraAngel.Client.ClientWindows;
using TerraAngel.Plugin;
using TerraAngel.UI;

namespace TerraAngel;

public unsafe class ClientLoader
{
    public static bool SetupRenderer = false;
    public static ClientRenderer? MainRenderer;
    internal static ConsoleWindow? ConsoleWindow;
    internal static ChatWindow? ChatWindow;
    public static ConfigUI ConfigUI = new ConfigUI();
    public static PluginUI PluginUI = new PluginUI();
    public static ResolutionUI ResolutionUI = new ResolutionUI();
    public static DiscordRpcClient? DiscordClient;
    public static ImGuiIOPtr? ImGuiIO => (ImGui.GetIO().NativePtr == null ? null : ImGui.GetIO());
    public static bool WantCaptureMouse => ImGuiIO?.WantCaptureMouse ?? false;
    public static bool WantCaptureKeyboard => ImGuiIO?.WantCaptureKeyboard ?? false;
    public static bool WantCaptureText => ImGuiIO?.WantTextInput ?? false;
    public static WindowManager? WindowManager;

    public static string SavePath => Path.Combine(Main.SavePath, "TerraAngel");
    public static string ConfigPath => Path.Combine(SavePath, "clientConfig.json");
    public static string WindowConfigPath => Path.Combine(SavePath, "clientWindowConfig.json");
    public static string PluginsPath => Path.Combine(SavePath, "Plugins");
    public static string TerrariaPath => Path.GetDirectoryName(typeof(Main).Assembly.Location)!;
    public static string AssetPath => "Assets";
    public static string NewLibraryPath => "Libraries";
    public static string ArchitectureString => Environment.Is64BitProcess ? "x64" : "x86";
    public static string PlatformString
    {
        get
        {
            if (Platform.IsWindows) return "win";
            if (Platform.IsOSX) return "osx";
            if (Platform.IsLinux) return "lin";
            return "win";
        }
    }
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

            if (ClientSteamUtils.TryFindTerrariaDirectory(out string foundSteamPath))
            {
                foundSteamPath = Path.Combine(foundSteamPath, "Content");
                if (Directory.Exists(foundSteamPath))
                {
                    return foundSteamPath;
                }
            }

            throw new FileNotFoundException("Could not find Content folder. Try copying the Content folder from your Terraria install to where the client is.");
        }
    }


    private static void LoadClientInteral()
    {
        NativeLibrary.SetDllImportResolver(typeof(ImGui).Assembly, (libraryName, assembly, searchPath) =>
        {
            IntPtr handle = IntPtr.Zero;

            if (libraryName == "cimgui")
            {
                if (!NativeLibrary.TryLoad($"{TerrariaPath}/{NewLibraryPath}/{PlatformString}/{ArchitectureString}/ImGui/{libraryName}", out handle))
                {
                    throw new DllNotFoundException($"Could not load {libraryName}");
                }
            }

            return handle;
        });

        NativeLibrary.SetDllImportResolver(typeof(Vector2).Assembly, (libraryName, assembly, searchPath) =>
        {
            IntPtr handle = IntPtr.Zero;

            if (libraryName == "FAudio" || libraryName == "FNA3D" || libraryName == "libtheorafile" || libraryName == "SDL2")
            {
                if (!NativeLibrary.TryLoad($"{TerrariaPath}/{NewLibraryPath}/{PlatformString}/{ArchitectureString}/FNA/{libraryName}", out handle))
                {
                    throw new DllNotFoundException($"Could not load {libraryName}");
                }
            }

            return handle;
        });

        NativeLibrary.SetDllImportResolver(typeof(Steamworks.SteamAPI).Assembly, (libraryName, assembly, searchPath) =>
        {
            IntPtr handle = IntPtr.Zero;

            if (libraryName == "steam_api" || libraryName == "steam_api64")
            {
                if (!NativeLibrary.TryLoad($"{TerrariaPath}/{NewLibraryPath}/{PlatformString}/{ArchitectureString}/Steamworks/{libraryName}", out handle))
                {
                    throw new DllNotFoundException($"Could not load {libraryName}");
                }
            }

            return handle;
        });

        GameHooks.Generate();

        ClientConfig.ReadFromFile();

        Type[] cringeTypes = typeof(Cringe).Assembly.GetTypes().Where(x =>
                                                                    !x.IsAbstract &&
                                                                    x.IsSubclassOf(typeof(Cringe)) &&
                                                                    x.GetConstructor(Array.Empty<Type>()) != null).ToArray();
        for (int i = 0; i < cringeTypes.Length; i++)
        {
            Type type = cringeTypes[i];
            CringeManager.AddCringe(type);
            ClientConfig.SetDefaultCringeValues(CringeManager.GetCringe(type));
        }

        CringeManager.SortTabs();

        if (ClientConfig.Settings.UseDiscordRPC)
        {
            InitDiscord();
        }

        
    }

    public static void LoadClient()
    {
        if (typeof(Main).Assembly.GetType("TerraAngel.Sexer") is null) throw new Exception("No sexer.");
#if !DEBUG
            try
            {
#endif
        AppDomain.CurrentDomain.AssemblyResolve += (sender, sargs) =>
        {
            if (sargs.Name.StartsWith("Steamworks.NET"))
            {
                return Assembly.LoadFrom($"{TerrariaPath}/{NewLibraryPath}/{ArchitectureString}/Steamworks/Steamworks.NET.dll");
            }
            return null;
        };
        LoadClientInteral();
#if !DEBUG
            }
            catch (Exception e)
            {
                Program.DisplayException(e);
            }
#endif
    }

    public static void SetupImGuiRenderer(Game main)
    {
        if (!SetupRenderer)
        {
            SetupRenderer = true;
            MainRenderer = new ClientRenderer(main);
            ClientConfig.Settings.PluginsToEnable = ClientConfig.Settings.pluginsToEnable;
        }


    }

    public static void InitDiscord()
    {
        DiscordClient = new DiscordRpcClient("1002097004672995369");

        DiscordClient.Initialize();

        DiscordClient.SetPresence(new RichPresence()
        {
            Timestamps = new Timestamps()
            {
                Start = DateTime.UtcNow
            },
            Assets = new Assets()
            {
                LargeImageKey = "angel-icon",
                LargeImageText = "TerraAngel Client",
            },
        });
    }

    public static class Console
    {
        public static void WriteLine(string message) => ConsoleWindow?.WriteLine(message);
        public static void WriteLine(string message, Color color) => ConsoleWindow?.WriteLine(message, color);
        public static void WriteError(string message) => ConsoleWindow?.WriteError(message);
        public static void AddCommand(string name, Action<ConsoleWindow.CmdStr> action, string description = "No Description Given") => ConsoleWindow?.AddCommand(name, action, description);
        public static void ClearConsole() => ConsoleWindow?.ClearConsole();
    }

    public static class Chat
    {
        public static void WriteLine(string message) => ChatWindow?.WriteLine(message);
        public static void WriteLine(string message, Color color) => ChatWindow?.WriteLine(message, color);
        public static void AddText(string message) => ChatWindow?.AddText(message);
    }
}
