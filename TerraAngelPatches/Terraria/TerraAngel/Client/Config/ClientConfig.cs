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

namespace TerraAngel.Client.Config
{
    public class ClientConfig
    {
        public static ClientConfig Instance;
        public ClientConfig()
        {
            if (Instance == null)
            {
                Instance = this;
                Instance.ReadFromFile();

                GlobalCheatManager.AntiHurt = Instance.DefaultAntiHurt;
                GlobalCheatManager.InfiniteMana = Instance.DefaultInfiniteMana;
                GlobalCheatManager.InfiniteMinions = Instance.DefaultInfiniteMinions;
                GlobalCheatManager.ESPBoxes = Instance.DefaultESPBoxes;
                GlobalCheatManager.ESPTracers = Instance.DefaultESPTracers;
            }
        }

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
        [UIConfigElement("Default Hitboxes")]
        public bool DefaultESPBoxes = true;
        [UIConfigElement("Send Rod of Discord packet when teleporting")]
        public bool TeleportSendRODPacket = true;
        [UIConfigElement("Right click on the map to teleport")]
        public bool RightClickOnMapToTeleport = true;
        [UIConfigElement("Toggle UI")]
        public Keys ToggleUIVisibility = Keys.OemTilde;
        [UIConfigElement("Toggle stats window being movable")]
        public Keys ToggleStatsWindowMovability = Keys.NumPad5;
        [UIConfigElement("Toggle Noclip")]
        public Keys ToggleNoclip = Keys.F2;
        [UIConfigElement("Toggle Freecam")]
        public Keys ToggleFreecam = Keys.F3;
        [UIConfigElement("Toggle Fullbright")]
        public Keys ToggleFullbright = Keys.F4;
        [UIConfigElement("Teleport to cursor")]
        public Keys TeleportToCursor = Keys.Z;

        public static List<UIElement> GetUITexts()
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
                        elements.Add(new UITextCheckbox(name, () => (bool)field.GetValue(Instance), (x) => field.SetValue(Instance, x)));
                    }
                    else if (field.FieldType == typeof(Keys))
                    {
                        elements.Add(new UIKeySelect(name, () => (Keys)field.GetValue(Instance), (x) => field.SetValue(Instance, x)));
                    }
                }
            }

            return elements;
        }

        private static object FileLock = new object();
        private static string fileName = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\clientConfig.json";
        public void WriteToFile()
        {
            lock (FileLock)
            {
                string s = JsonConvert.SerializeObject(Instance);
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(s);
                    fs.SetLength(bytes.Length);
                    fs.Write(bytes);
                    fs.Close();
                }
            }
        }

        public void ReadFromFile()
        {
            if (!File.Exists(fileName))
            {
                WriteToFile();
            }
            lock (FileLock)
            {
                string s = "";
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer);
                    s = Encoding.UTF8.GetString(buffer);
                    fs.Close();
                }
                Instance = JsonConvert.DeserializeObject<ClientConfig>(s);
            }
        }
    }
}
