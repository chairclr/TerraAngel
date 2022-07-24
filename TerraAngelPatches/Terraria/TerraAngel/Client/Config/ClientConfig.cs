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

        [UIConfigElement("Default Tracers")]
        public bool DefaultESPTracers = false;

        [UIConfigElement("Default Player Hitboxes")]
        public bool DefaultPlayerESPBoxes = true;

        [UIConfigElement("Default NPC Hitboxes")]
        public bool DefaultNPCBoxes = false;

        [UIConfigElement("Default Projectile Hitboxes")]
        public bool DefaultProjectileBoxes = false;

        [UIConfigElement("Default Show Held Item")]
        public bool DefaultShowHeldItem = false;

        [UIConfigElement("Send Rod of Discord packet when teleporting")]
        public bool TeleportSendRODPacket = true;

        [UIConfigElement("Right click on the map to teleport")]
        public bool RightClickOnMapToTeleport = true;

        [UIConfigElement("Right click on player to view inventory")]
        public bool RightClickOnPlayerToInspect = true;

        [UIConfigElement("Disable Nebula Packet")]
        public bool DisableNebulaLagPacket = true;

        [UIConfigElement("Toggle UI")]
        public Keys ToggleUIVisibility = Keys.OemTilde;

        [UIConfigElement("Toggle stats window being movable")]
        public Keys ToggleStatsWindowMovability = Keys.NumPad5;

        [UIConfigElement("Toggle Net Message Sender UI")]
        public Keys ToggleNetMessageSender = Keys.NumPad6;

        [UIConfigElement("Toggle Noclip")]
        public Keys ToggleNoclip = Keys.F2;

        [UIConfigElement("Toggle Freecam")]
        public Keys ToggleFreecam = Keys.F3;

        [UIConfigElement("Toggle Fullbright")]
        public Keys ToggleFullbright = Keys.F4;

        [UIConfigElement("Teleport to cursor")]
        public Keys TeleportToCursor = Keys.Z;

        public Color TracerColor = new Color(0f, 0f, 1f);
        public Color LocalBoxPlayerColor = new Color(0f, 1f, 0f);
        public Color OtherBoxPlayerColor = new Color(1f, 0f, 0f);
        public Color NPCBoxColor = new Color(1f, 0f, 1f);
        public Color ProjectileBoxColor = new Color(1f, 0f, 1f);


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
            List<UIElement> elements = new List<UIElement>();

            foreach (FieldInfo field in typeof(ClientConfig).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                UIConfigElementAttribute? attribute = field.GetCustomAttribute<UIConfigElementAttribute>();
                if (attribute != null)
                {
                    string name = attribute.Name;

                    if (field.FieldType == typeof(bool))
                    {
                        elements.Add(new UITextCheckbox(name, () => (bool)field.GetValue(this), (x) => field.SetValue(this, x)));
                    }
                    else if (field.FieldType == typeof(Keys))
                    {
                        elements.Add(new UIKeySelect(name, () => (Keys)field.GetValue(this), (x) => field.SetValue(this, x)));
                    }
                }
            }

            return elements;
        }

        private static object FileLock = new object();
        public static void WriteToFile(ClientConfig config)
        {
            lock (FileLock)
            {
                config.pluginsToEnable = config.PluginsToEnable;

                string s = JsonConvert.SerializeObject(config);
                Utility.Util.CreateParentDirectory(ClientLoader.ConfigPath);
                using (FileStream fs = new FileStream(ClientLoader.ConfigPath, FileMode.OpenOrCreate))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(s);
                    fs.SetLength(bytes.Length);
                    fs.Write(bytes);
                    fs.Close();
                }
            }
        }

        public static ClientConfig ReadFromFile()
        {
            if (!File.Exists(ClientLoader.ConfigPath))
            {
                WriteToFile(new ClientConfig());
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
                return JsonConvert.DeserializeObject<ClientConfig>(s);
            }
        }
    }
}
