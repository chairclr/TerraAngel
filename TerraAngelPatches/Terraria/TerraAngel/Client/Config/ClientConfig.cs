using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using TerraAngel.UI;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.ID;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using TerraAngel.Cheat;
using System.Runtime.Serialization;
using TerraAngel;

namespace TerraAngel.Client.Config
{
    public class ClientConfig
    {
        [AttributeUsage(AttributeTargets.Field, Inherited = false)]
        public class UIConfigElementAttribute : Attribute
        {
            public readonly string Name;

            public UIConfigElementAttribute(string name)
            {
                Name = name;
            }
        }

        public static Config Settings = new Config();

        public class Config
        {
            [UIConfigElement("Show Stats Window")]
            public bool ShowStatsWindow = true;

            [UIConfigElement("Show Console Window")]
            public bool ShowConsoleWindow = true;

            [UIConfigElement("Default Anti-Hurt")]
            public bool DefaultAntiHurt = true;

            [UIConfigElement("Default Infinite Mana")]
            public bool DefaultInfiniteMana = true;

            [UIConfigElement("Default Infinite Minions")]
            public bool DefaultInfiniteMinions = true;

            [UIConfigElement("Default Draw Any ESP")]
            public bool DefaultDrawAnyESP = true;

            [UIConfigElement("Default Tracers")]
            public bool DefaultPlayerESPTracers = false;

            [UIConfigElement("Default Player Hitboxes")]
            public bool DefaultPlayerESPBoxes = true;

            [UIConfigElement("Default NPC Hitboxes")]
            public bool DefaultNPCBoxes = false;

            [UIConfigElement("Default Projectile Hitboxes")]
            public bool DefaultProjectileBoxes = false;

            [UIConfigElement("Default Item Hitboxes")]
            public bool DefaultItemBoxes = false;

            [UIConfigElement("Default Show Held Item")]
            public bool DefaultShowHeldItem = false;

            [UIConfigElement("Default Infinite Reach")]
            public bool DefaultInfiniteReach = false;

            [UIConfigElement("Send Rod of Discord packet when teleporting")]
            public bool TeleportSendRODPacket = true;

            [UIConfigElement("Right click on the map to teleport")]
            public bool RightClickOnMapToTeleport = true;

            [UIConfigElement("Right click on player to view inventory")]
            public bool RightClickOnPlayerToInspect = true;

            [UIConfigElement("Disable Nebula Packet")]
            public bool DisableNebulaLagPacket = true;

            [UIConfigElement("Console Auto Scroll")]
            public bool ConsoleAutoScroll = true;

            [UIConfigElement("Clear chat on world changes")]
            public bool ClearChatThroughWorldChanges = false;

            [UIConfigElement("Clear chat input on close")]
            public bool ClearChatInputOnClose = false;

            [UIConfigElement("Default Projectile Prediction")]
            public bool DefaultDrawActiveProjectilePrediction = true;

            [UIConfigElement("Default Projectile Prediction Draw Friendly")]
            public bool DefaultDrawFriendlyProjectilePrediction = false;

            [UIConfigElement("Default Projectile Prediction Draw Hostile")]
            public bool DefaultDrawHostileProjectilePrediction = true;

            [UIConfigElement("Discord Rich Presence")]
            public bool UseDiscordRPC = true;

            [UIConfigElement("Tell server that you're using a modified client (Experimental)")]
            public bool BroadcastPresence = false;

            [UIConfigElement("Show Detailed Item Tooltip")]
            public bool ShowDetailedItemTooltip = true;

            [UIConfigElement("Toggle UI")]
            public Keys ToggleUIVisibility = Keys.OemTilde;

            [UIConfigElement("Toggle All ESP")]
            public Keys ToggleDrawAnyESP = Keys.End;

            [UIConfigElement("Toggle stats window being movable")]
            public Keys ToggleStatsWindowMovability = Keys.NumPad5;

            [UIConfigElement("Toggle Net Debugger UI")]
            public Keys ToggleNetDebugger = Keys.NumPad6;

            [UIConfigElement("Toggle Noclip")]
            public Keys ToggleNoclip = Keys.F2;

            [UIConfigElement("Toggle Freecam")]
            public Keys ToggleFreecam = Keys.F3;

            [UIConfigElement("Toggle Fullbright")]
            public Keys ToggleFullbright = Keys.F4;

            [UIConfigElement("Teleport to Cursor")]
            public Keys TeleportToCursor = Keys.Z;

            [UIConfigElement("Toggle Style Editor")]
            public Keys ToggleStyleEditor = Keys.NumPad8;

            [UIConfigElement("World Edit Select")]
            public Keys WorldEditSelectKey = Keys.F;

            [UIConfigElement("Save Console History")]
            public bool PreserveConsoleHistory = true;
            [UIConfigElement("Save Console State")]
            public bool ConsoleSaveInReplMode = true;
            public int ConsoleHistoryLimit = 5000;
            public int ChatHistoryLimit = 3000;

            public List<string>? ConsoleHistorySave = new List<string>();

            [JsonIgnore]
            public List<string> ConsoleHistory = new List<string>();

            public float FullBrightBrightness = 0.7f;
            public Color TracerColor = new Color(0f, 0f, 1f);
            public Color LocalBoxPlayerColor = new Color(0f, 1f, 0f);
            public Color OtherBoxPlayerColor = new Color(1f, 0f, 0f);
            public Color OtherTerraAngelUserColor = new Color(1f, 0.5f, 0.3f);
            public Color NPCBoxColor = new Color(1f, 0f, 1f);
            public Color NPCNetOffsetBoxColor = new Color(0f, 0f, 0f);
            public Color ProjectileBoxColor = new Color(1f, 0f, 1f);
            public Color ItemBoxColor = new Color(0.9f, 0.2f, 0.6f);
            public Color FriendlyProjectilePredictionDrawColor = new Color(0f, 1f, 0f);
            public Color HostileProjectilePredictionDrawColor = new Color(1f, 0f, 0f);
            public int ProjectilePredictionMaxStepCount = 1500;

            public float ChatWindowTransperencyActive = 0.5f;
            public float ChatWindowTransperencyInactive = 0.0f;
            public bool ChatAutoScroll = true;
            public int framesForMessageToBeVisible = 600;
            public bool ChatVanillaInvetoryBehavior = true;

            public bool AutoFishAcceptItems = true;
            public bool AutoFishAcceptAllItems = true;
            public bool AutoFishAcceptQuestFish = true;
            public bool AutoFishAcceptCrates = true;
            public bool AutoFishAcceptNormal = true;
            public bool AutoFishAcceptCommon = true;
            public bool AutoFishAcceptUncommon = true;
            public bool AutoFishAcceptRare = true;
            public bool AutoFishAcceptVeryRare = true;
            public bool AutoFishAcceptLegendary = true;
            public bool AutoFishAcceptNPCs = true;
            public int AutoFishFrameCountRandomizationMin = 10;
            public int AutoFishFrameCountRandomizationMax = 50;

            public float StatsWindowHoveredTransperency = 0.65f;
            public bool ConsoleInReplMode = false;

            public ClientUIConfig UIConfig = new ClientUIConfig();

            [JsonIgnore]
            public List<string> PluginsToEnable
            {
                get
                {
                    List<string> enabled = new List<string>();

                    foreach (KeyValuePair<string, bool> availablePlugin in Plugin.PluginLoader.AvailablePlugins)
                    {
                        if (availablePlugin.Value)
                            enabled.Add(availablePlugin.Key);
                    }

                    return enabled;
                }
                set
                {
                    Plugin.PluginLoader.VerifyEnabled();
                    foreach (string enabledPlugin in value)
                    {
                        if (Plugin.PluginLoader.AvailablePlugins.ContainsKey(enabledPlugin))
                        {
                            Plugin.PluginLoader.AvailablePlugins[enabledPlugin] = true;
                        }
                    }
                    Plugin.PluginLoader.UnloadPlugins();
                    Plugin.PluginLoader.LoadPlugins();
                }
            }

            [JsonProperty("PluginsToEnable")]
            public List<string> pluginsToEnable;

            public List<UIElement> GetUITexts()
            {
                SortedList<string, UIElement> elements = new SortedList<string, UIElement>();

                List<FieldInfo> fields = typeof(Config).GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();

                for (int i = 0; i < fields.Count; i++)
                {
                    FieldInfo field = fields[i];
                    UIConfigElementAttribute? attribute = field.GetCustomAttribute<UIConfigElementAttribute>();
                    if (attribute != null)
                    {
                        string name = attribute.Name;

                        if (field.FieldType == typeof(bool))
                        {
                            elements.Add(name, new UITextCheckbox(name, () => (bool)(field.GetValue(this) ?? false), (x) => field.SetValue(this, x)));
                        }
                        else if (field.FieldType == typeof(Keys))
                        {
                            elements.Add("\uFFFF" + name, new UIKeySelect(name, () => (Keys)(field.GetValue(this) ?? Keys.None), (x) => field.SetValue(this, x)));
                        }
                    }
                }


                return elements.Values.ToList();
            }
        }

        private static object FileLock = new object();
        public static void WriteToFile()
        {
            lock (FileLock)
            {
                Settings.pluginsToEnable = Settings.PluginsToEnable;
                Settings.UIConfig.Get();
                if (Settings.PreserveConsoleHistory)
                    Settings.ConsoleHistorySave = Settings.ConsoleHistory;

                string s = JsonConvert.SerializeObject(Settings, new JsonSerializerSettings() { Formatting = Formatting.Indented });
                Utility.Util.CreateParentDirectory(ClientLoader.ConfigPath);
                using (FileStream fs = new FileStream(ClientLoader.ConfigPath, FileMode.OpenOrCreate))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(s);
                    fs.SetLength(bytes.Length);
                    fs.Write(bytes);
                    fs.Close();
                }

                Settings.ConsoleHistorySave = null;
            }
        }

        public static void ReadFromFile()
        {
            if (!File.Exists(ClientLoader.ConfigPath))
            {
                WriteToFile();
            }
            lock (FileLock)
            {
                string s = "";
                using (FileStream fs = new FileStream(ClientLoader.ConfigPath, FileMode.OpenOrCreate))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer);
                    s = Encoding.UTF8.GetString(buffer);
                    fs.Close();
                }
                Settings = JsonConvert.DeserializeObject<Config>(s) ?? new Config();

                if (Settings.PreserveConsoleHistory && Settings.ConsoleHistorySave is not null)
                    Settings.ConsoleHistory = Settings.ConsoleHistorySave;
                else
                    Settings.ConsoleHistorySave = null;
            }
        }
    }
}
