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

namespace TerraAngel
{
    public class ClientLoader
    {
        public static bool SetupRenderer = false;
        public static ClientRenderer? MainRenderer;
        internal static ConsoleWindow? ConsoleWindow;
        public static ConfigUI ConfigUI = new ConfigUI();
        public static PluginUI PluginUI = new PluginUI();
        public static ClientConfig Config = new ClientConfig();

        public static string SavePath => Path.Combine(Main.SavePath, "TerraAngel");
        public static string ConfigPath => Path.Combine(SavePath, "clientConfig.json");
        public static string PluginsPath => Path.Combine(SavePath, "Plugins");

        public static void Hookgen_Early()
        {
            GameHooks.Generate();

            Config = ClientConfig.ReadFromFile();

            GlobalCheatManager.AntiHurt = Config.DefaultAntiHurt;
            GlobalCheatManager.ESPTracers = Config.DefaultESPTracers;
            GlobalCheatManager.ESPBoxes = Config.DefaultESPBoxes;
            GlobalCheatManager.InfiniteMinions = Config.DefaultInfiniteMinions;
            GlobalCheatManager.InfiniteMana = Config.DefaultInfiniteMana;
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

        public static class Console
        {
            public static void WriteLine(string message) => ConsoleWindow?.WriteLine(message);
            public static void WriteLine(string message, Color color) => ConsoleWindow?.WriteLine(message, color);
            public static void WriteError(string message) => ConsoleWindow?.WriteError(message);
            public static void AddCommand(string name, Action<ConsoleWindow.CmdStr> action, string description = "No Description Given") => ConsoleWindow?.AddCommand(name, action, description);
            public static void ClearConsole() => ConsoleWindow?.ClearConsole();
        }
    }
}
