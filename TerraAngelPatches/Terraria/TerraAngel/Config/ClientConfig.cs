using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.RulesetToEditorconfig;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using TerraAngel.UI;
using TerraAngel.UI.TerrariaUI;
using Terraria.UI;

namespace TerraAngel.Config;

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
        public bool DefaultAntiHurt = false;

        [UIConfigElement("Default Infinite Mana")]
        public bool DefaultInfiniteMana = false;

        [UIConfigElement("Default Infinite Minions")]
        public bool DefaultInfiniteMinions = false;

        [UIConfigElement("Default ESP Draw Any")]
        public bool DefaultDrawAnyESP = false;

        [UIConfigElement("Default ESP On Map")]
        public bool DefaultMapESP = false;

        [UIConfigElement("Default ESP Tracers")]
        public bool DefaultPlayerESPTracers = false;

        [UIConfigElement("Default ESP Player Hitboxes")]
        public bool DefaultPlayerESPBoxes = true;

        [UIConfigElement("Default ESP NPC Hitboxes")]
        public bool DefaultNPCBoxes = false;

        [UIConfigElement("Default ESP Projectile Hitboxes")]
        public bool DefaultProjectileBoxes = false;

        [UIConfigElement("Default ESP Item Hitboxes")]
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

        [UIConfigElement("Default Disable Gore")]
        public bool DefaultDisableGore = false;

        [UIConfigElement("Default Disable Dust")]
        public bool DefaultDisableDust = false;

        [UIConfigElement("Discord Rich Presence")]
        public bool UseDiscordRPC = true;

        [UIConfigElement("Tell server that you're using a modified client (Experimental)")]
        public bool BroadcastPresence = false;

        [UIConfigElement("Show Detailed Tooltips")]
        public bool ShowDetailedTooltips = true;

        [UIConfigElement("Ignore super laggy visuals (Yorai eye)")]
        public bool IgnoreReLogicBullshit = true;

        [UIConfigElement("Save Console History")]
        public bool PreserveConsoleHistory = true;

        [UIConfigElement("Save Console State")]
        public bool ConsoleSaveInReplMode = true;

        [UIConfigElement("Chat Replicates Vanilla Behavior")]
        public bool ChatVanillaInvetoryBehavior = true;

        [UIConfigElement("Default Disable Tile Framing")]
        public bool DefaultDisableTileFraming = false;

        [UIConfigElement("Default ESP Tile Sections")]
        public bool DefaultTileSections = false;

        [UIConfigElement("Default Full Bright")]
        public bool FullBrightDefaultValue = false;

        [UIConfigElement("Always Enable Journey Menu")]
        public bool ForceEnableCreativeUI = false;

        [UIConfigElement("All Journey Items Available")]
        public bool ForceAllCreativeUnlocks = false;

        [UIConfigElement("Enable Steam")]
        public bool UseSteamSocialAPI = true;

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
        public Keys ToggleFullBright = Keys.F4;

        [UIConfigElement("Teleport to Cursor")]
        public Keys TeleportToCursor = Keys.Z;

        [UIConfigElement("Toggle Style Editor")]
        public Keys ToggleStyleEditor = Keys.NumPad8;

        [UIConfigElement("World Edit Select")]
        public Keys WorldEditSelectKey = Keys.F;

        [UIConfigElement("Show Player Inspector")]
        public Keys ShowInspectorWindow = Keys.NumPad7;

        [UIConfigElement("Show Timing Metrics")]
        public Keys ShowTimingMetrics = Keys.NumPad9;

        [UIConfigElement("Take Map Screenshot")]
        public Keys TakeMapScreenshot = Keys.F9;

        [UIConfigElement("Open Quick Item Browser (Ctrl +")]
        public Keys OpenFastItemBrowser = Keys.I;

        public int ConsoleHistoryLimit = 5000;
        public int ChatHistoryLimit = 3000;
        public int ChatMessageLimit = 600;

        public List<string>? ConsoleHistorySave = new List<string>();

        [JsonIgnore]
        public List<string> ConsoleHistory = new List<string>();


        public int ConsoleUndoStackSize = 3000;

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

        public bool AutoAttackFavorBosses = true;
        public bool AutoAttackTargetHostileNPCs = true;
        public bool AutoAttackRequireLineOfSight = true;
        public bool AutoAttackVelocityPrediction = true;
        public float AutoAttackVelocityPredictionScaling = 0.2269f;
        public float AutoAttackMinTargetRange = 800f;

        public float StatsWindowHoveredTransperency = 0.65f;

        public int LightingBlurPassCount = 4;

        public int MapScreenshotPixelsPerTile = 4;


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
                Plugin.PluginLoader.FindPluginFiles();
                foreach (string enabledPlugin in value)
                {
                    if (Plugin.PluginLoader.AvailablePlugins.ContainsKey(enabledPlugin))
                    {
                        Plugin.PluginLoader.AvailablePlugins[enabledPlugin] = true;
                    }
                }
                Plugin.PluginLoader.UnloadPlugins();
                Plugin.PluginLoader.LoadAndInitializePlugins();
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


    //public class VectorConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        return objectType == typeof(Vector2) || objectType == typeof(Vector3) || objectType == typeof(Vector4);
    //    }
    //
    //    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    //    {
    //        if (objectType == typeof(Vector2)) return serializer.Deserialize<System.Numerics.Vector2>(reader).ToXNA();
    //        if (objectType == typeof(Vector3)) return serializer.Deserialize<System.Numerics.Vector3>(reader).ToXNA();
    //        if (objectType == typeof(Vector4)) return serializer.Deserialize<System.Numerics.Vector4>(reader).ToXNA();
    //        return existingValue;
    //    }
    //
    //    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    //    {
    //        if (value is Vector2 vec2) serializer.Serialize(writer, vec2.ToNumerics());
    //        if (value is Vector3 vec3) serializer.Serialize(writer, vec3.ToNumerics());
    //        if (value is Vector4 vec4) serializer.Serialize(writer, vec4.ToNumerics());
    //    }
    //}

    public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
    {
        Converters = new List<JsonConverter>()
        {
        //    new VectorConverter(),
        },
        Formatting = Formatting.Indented,
    };

    public static void BeforeWrite()
    {
        Settings.pluginsToEnable = Settings.PluginsToEnable;
        Settings.UIConfig.Get();
        if (Settings.PreserveConsoleHistory)
            Settings.ConsoleHistorySave = Settings.ConsoleHistory;
        Settings.LightingBlurPassCount = Lighting.NewEngine.BlurPassCount;
    }

    private static object FileLock = new object();
    public static Task WriteToFile()
    {
        lock (FileLock)
        {
            BeforeWrite();
            string s = JsonConvert.SerializeObject(Settings, SerializerSettings);
            DirectoryUtility.TryCreateParentDirectory(ClientLoader.ConfigPath);
            using (FileStream fs = new FileStream(ClientLoader.ConfigPath, FileMode.OpenOrCreate))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                fs.SetLength(bytes.Length);
                fs.Write(bytes);
                fs.Close();
            }

            Settings.ConsoleHistorySave = null;
        }

        return Task.CompletedTask;
    }
    public static void AfterRead()
    {
        if (Settings.PreserveConsoleHistory && Settings.ConsoleHistorySave is not null)
            Settings.ConsoleHistory = Settings.ConsoleHistorySave;
        else
            Settings.ConsoleHistorySave = null;
    }
    public static void AfterReadLater()
    {
        Settings.UIConfig.Set();
        Lighting.NewEngine.BlurPassCount = Settings.LightingBlurPassCount;
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
            Settings = JsonConvert.DeserializeObject<Config>(s, SerializerSettings) ?? new Config();
            AfterRead();
        }
    }

    private static float SaveTimer;
    public static void Update()
    {
        SaveTimer -= Time.UpdateDeltaTime;
        if (SaveTimer <= 0.0f)
        {
            SaveTimer = 5.0f;
            
            Task.Run(WriteToFile);

            if (ClientLoader.WindowManager is not null)
            {
                Task.Run(ClientLoader.WindowManager.WriteToFile);
            }
        }
    }

    delegate ref float FuncRefFloat();
    delegate ref Vector2 FuncRefVector2();
    delegate ref Vector3 FuncRefVector3();
    delegate ref Vector4 FuncRefVector4();
    delegate ref bool FuncRefBool();
    public static void SetDefaultCringeValues(Tool cringe)
    {
        Type type = cringe.GetType();
        foreach (FieldInfo field in type.GetFields().Where(x => Attribute.IsDefined(x, typeof(DefaultConfigValueAttribute))))
        {
            DefaultConfigValueAttribute? value = (DefaultConfigValueAttribute?)Attribute.GetCustomAttribute(field, typeof(DefaultConfigValueAttribute));
            if (value is null) throw new NullReferenceException("No attribute L");

            FieldInfo? configField = typeof(Config).GetField(value.FieldName, BindingFlags.Public | BindingFlags.Instance);
            if (configField is null) throw new Exception($"Could not find field {value.FieldName}");

            field.SetValue(cringe, configField.GetValue(Settings));
        }

        foreach (PropertyInfo property in type.GetProperties().Where(x => Attribute.IsDefined(x, typeof(DefaultConfigValueAttribute))))
        {
            bool refType = false;
            if (!property.CanWrite)
            {
                if (property.CanRead && property.GetMethod is not null && property.GetMethod.ReturnType.IsByRef)
                {
                    refType = true;
                }
                else
                {
                    throw new Exception($"Property {property.Name} cannot be written to");
                }
            }

            DefaultConfigValueAttribute? value = (DefaultConfigValueAttribute?)Attribute.GetCustomAttribute(property, typeof(DefaultConfigValueAttribute));
            if (value is null) throw new NullReferenceException("No attribute L");

            FieldInfo? configField = typeof(Config).GetField(value.FieldName, BindingFlags.Public | BindingFlags.Instance);
            if (configField is null) throw new Exception($"Could not find field {value.FieldName}");

            if (!refType) property.SetValue(cringe, configField.GetValue(Settings));

            // this is a shitty solution to the problem of dotnet not having an easy way to set ref properties :(
            else
            {
                object? obj = configField.GetValue(Settings);
                Type t = property.PropertyType;
                MethodInfo? getMethod = property.GetMethod;
                if (getMethod is null)
                    continue;

                if (t == typeof(float).MakeByRefType())
                {
                    getMethod.CreateDelegate<FuncRefFloat>(cringe)() = (float)obj;
                }
                else if (t == typeof(Vector2).MakeByRefType())
                {
                    getMethod.CreateDelegate<FuncRefVector2>(cringe)() = (Vector2)obj;
                }
                else if (t == typeof(Vector3).MakeByRefType())
                {
                    getMethod.CreateDelegate<FuncRefVector3>(cringe)() = (Vector3)obj;
                }
                else if (t == typeof(Vector4).MakeByRefType())
                {
                    getMethod.CreateDelegate<FuncRefVector4>(cringe)() = (Vector4)obj;
                }
                else if (t == typeof(bool).MakeByRefType())
                {
                    getMethod.CreateDelegate<FuncRefBool>(cringe)() = (bool)obj;
                }
            }
        }
    }
}
