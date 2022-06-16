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

        public bool ShowStatsWindow = true;
        public bool ShowConsoleWindow = true;
        public bool DefaultAntiHurt = true;
        public bool DefaultInfiniteMana = true;
        public bool DefaultInfiniteMinions = true;
        public bool DefaultESPTracers = false;
        public bool DefaultESPBoxes = true;
        public bool TeleportSendRODPacket = true;
        public bool RightClickOnMapToTeleport = true;
        public Keys ToggleUIVisibility = Keys.OemTilde;
        public Keys ToggleStatsWindowMovability = Keys.NumPad5;
        public Keys ToggleNoclip = Keys.F2;
        public Keys ToggleFreecam = Keys.F3;
        public Keys ToggleFullbright = Keys.F4;
        public Keys TeleportToCursor = Keys.Z;

        public static List<UIElement> GetUITexts()
        {
            return new List<UIElement>() 
            { 
                new UITextCheckbox("Show Stats Window",                     () => Instance.ShowStatsWindow,               (v) => Instance.ShowStatsWindow               = v), 
                new UITextCheckbox("Show Console Window",                   () => Instance.ShowConsoleWindow,             (v) => Instance.ShowConsoleWindow             = v),

                new UITextCheckbox("Default Anti-Hurt",                     () => Instance.DefaultAntiHurt,               (v) => Instance.DefaultAntiHurt               = v),
                new UITextCheckbox("Default Infinite Mana",                 () => Instance.DefaultInfiniteMana,           (v) => Instance.DefaultInfiniteMana           = v),
                new UITextCheckbox("Default Infinite Minions",              () => Instance.DefaultInfiniteMinions,        (v) => Instance.DefaultInfiniteMinions        = v),

                new UITextCheckbox("Default ESp Boxes",                     () => Instance.DefaultESPBoxes,               (v) => Instance.DefaultESPBoxes               = v),
                new UITextCheckbox("Default ESp Tracers",                   () => Instance.DefaultESPTracers,             (v) => Instance.DefaultESPTracers             = v),

                new UITextCheckbox("Right Click on Map for Teleport",       () => Instance.RightClickOnMapToTeleport,     (v) => Instance.RightClickOnMapToTeleport     = v),
                new UITextCheckbox("Send Teleport Packet",                  () => Instance.TeleportSendRODPacket,         (v) => Instance.TeleportSendRODPacket         = v),

                new UIKeySelect(   "Toggle UI",                             () => Instance.ToggleUIVisibility,            (v) => Instance.ToggleUIVisibility            = v),
                new UIKeySelect(   "Move Stats window",                     () => Instance.ToggleStatsWindowMovability,   (v) => Instance.ToggleStatsWindowMovability   = v),

                new UIKeySelect(   "Teleport to Cursor",                    () => Instance.TeleportToCursor,              (v) => Instance.TeleportToCursor              = v),

                new UIKeySelect(   "Toggle Noclip",                         () => Instance.ToggleNoclip,                  (v) => Instance.ToggleNoclip                  = v),
                new UIKeySelect(   "Toggle Freecam",                        () => Instance.ToggleFreecam,                 (v) => Instance.ToggleFreecam                 = v),
                new UIKeySelect(   "Toggle Fullbright",                     () => Instance.ToggleFullbright,              (v) => Instance.ToggleFullbright              = v),
            };
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
