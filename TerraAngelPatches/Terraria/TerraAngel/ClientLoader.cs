using Microsoft.Xna.Framework;
using TerraAngel.Hooks;
using TerraAngel.Graphics;
using TerraAngel.Input;
using TerraAngel.Client;
using TerraAngel.Client.Config;
using TerraAngel.Client.ClientWindows;
using System;
using System.IO;
using TerraAngel.Plugin;
using Terraria;
using TerraAngel.Cheat;
using TerraAngel.Cheat.Cringes;
using System.Reflection;
using System.Linq;
using TerraAngel.UI;
using DiscordRPC;
using System.Runtime.InteropServices;
using ImGuiNET;
using Terraria.Audio;
using System.Diagnostics;

namespace TerraAngel
{
    public unsafe class ClientLoader
    {
        public static bool SetupRenderer = false;
        public static ClientRenderer? MainRenderer;
        internal static ConsoleWindow? ConsoleWindow;
        internal static ChatWindow? ChatWindow;
        public static ConfigUI ConfigUI = new ConfigUI();
        public static PluginUI PluginUI = new PluginUI();
        public static ResolutionUI ResolutionUI = new ResolutionUI();
        public static ClientConfig Config = new ClientConfig();
        public static DiscordRpcClient? DiscordClient;
        public static ImGuiNET.ImGuiIOPtr? ImGuiIO => (ImGuiNET.ImGui.GetIO().NativePtr == null ? null : ImGuiNET.ImGui.GetIO());
        public static bool WantCaptureMouse => ImGuiIO?.WantCaptureMouse ?? false;
        public static bool WantCaptureKeyboard => ImGuiIO?.WantCaptureKeyboard ?? false;


        public static string SavePath => Path.Combine(Main.SavePath, "TerraAngel");
        public static string ConfigPath => Path.Combine(SavePath, "clientConfig.json");
        public static string PluginsPath => Path.Combine(SavePath, "Plugins");
        public static string TerrariaPath => Path.GetDirectoryName(typeof(Main).Assembly.Location);
        public static string AssetPath => "Assets";
        public static string NativeLibraryPath => "LibNew";
        public static string Platform => Environment.Is64BitProcess ? "x64" : "x86";

        private static void LoadClientInteral()
        {
            NativeLibrary.SetDllImportResolver(typeof(ImGui).Assembly, (libraryName, assembly, searchPath) =>
            {
                IntPtr handle = IntPtr.Zero;

                if (libraryName == "cimgui")
                {
                    if (!NativeLibrary.TryLoad($"{TerrariaPath}/{NativeLibraryPath}/{Platform}/ImGui/{libraryName}", out handle))
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
                    if (!NativeLibrary.TryLoad($"{TerrariaPath}/{NativeLibraryPath}/{Platform}/FNA/{libraryName}", out handle))
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
                    if (!NativeLibrary.TryLoad($"{TerrariaPath}/{NativeLibraryPath}/{Platform}/Steamworks/{libraryName}", out handle))
                    {
                        throw new DllNotFoundException($"Could not load {libraryName}");
                    }
                }

                return handle;
            });

            GameHooks.Generate();

            Config = ClientConfig.ReadFromFile();

            Type[] cringeTypes = typeof(Cringe).Assembly.GetTypes().Where(x =>
                                                                        !x.IsAbstract &&
                                                                        x.IsSubclassOf(typeof(Cringe)) &&
                                                                        x.GetConstructor(Array.Empty<Type>()) != null).ToArray();
            for (int i = 0; i < cringeTypes.Length; i++)
            {
                Type type = cringeTypes[i];
                CringeManager.AddCringe(type);
            }

            CringeManager.SortTabs();

            CringeManager.GetCringe<AntiHurtCringe>().Enabled = Config.DefaultAntiHurt;
            CringeManager.GetCringe<InfiniteManaCringe>().Enabled = Config.DefaultInfiniteMana;
            CringeManager.GetCringe<InfiniteMinionCringe>().Enabled = Config.DefaultInfiniteMinions;
            CringeManager.GetCringe<HeldItemViewerCringe>().Enabled = Config.DefaultShowHeldItem;

            ESPCringe boxesCringe = CringeManager.GetCringe<ESPCringe>();
            boxesCringe.DrawAnyESP = Config.DefaultDrawAnyESP;
            boxesCringe.NPCBoxes = Config.DefaultNPCBoxes;
            boxesCringe.ProjectileBoxes = Config.DefaultProjectileBoxes;
            boxesCringe.PlayerBoxes = Config.DefaultPlayerESPBoxes;
            boxesCringe.ItemBoxes = Config.DefaultItemBoxes;
            boxesCringe.PlayerTracers = Config.DefaultPlayerESPTracers;


            if (Config.UseDiscordRPC)
            {
                InitDiscord();
            }
        }

        public static void LoadClient()
        {
#if !DEBUG
            try
            {
#endif
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
                Config.PluginsToEnable = Config.pluginsToEnable;
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
                Buttons = new Button[]
                {
                        new Button()
                        {
                            Label = "View GitHub",
                            Url = "https://github.com/CEO-Chair/TerraAngel/"
                        }
                }
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
}
