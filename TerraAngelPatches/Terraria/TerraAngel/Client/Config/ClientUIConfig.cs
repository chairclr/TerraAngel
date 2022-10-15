using System;
using System.Collections.Generic;
using System.Reflection;
using DiscordRPC.Logging;
using Newtonsoft.Json;


namespace TerraAngel.Client.Config
{
    public class ClientUIConfig
    {
        [JsonIgnore]
        public static string[] ColorNames = Enum.GetNames<ImGuiCol>();

        [JsonIgnore]
        private ImGuiStylePtr style => ImGui.GetStyle();

        [JsonIgnore]
        public Dictionary<string, Vector4> StyleColors
        {
            get
            {
                if (ImGui.GetCurrentContext() == IntPtr.Zero)
                    return null;
                ImGuiStylePtr styleCache = style;


                Dictionary<string, Vector4> colors = new Dictionary<string, Vector4>(styleCache.Colors.Count);

                for (int i = 0; i < styleCache.Colors.Count; i++)
                {
                    colors.Add(ColorNames[i], styleCache.Colors[i]);
                }

                return colors;
            }
            set
            {
                if (ImGui.GetCurrentContext() == IntPtr.Zero)
                    return;
                if (value is null)
                    return;

                ImGuiStylePtr styleCache = style;

                for (int i = 0; i < styleCache.Colors.Count; i++)
                {
                    if (value.TryGetValue(ColorNames[i], out Vector4 v))
                    {
                        styleCache.Colors[i] = v;
                    }
                }
            }
        }

        delegate ref float FuncRefFloat();
        delegate ref Vector2 FuncRefVector2();
        delegate ref Vector3 FuncRefVector3();
        delegate ref Vector4 FuncRefVector4();
        delegate ref bool FuncRefBool();

        [JsonIgnore]
        public Dictionary<string, object> StyleData
        {
            get
            {
                if (ImGui.GetCurrentContext() == IntPtr.Zero)
                    return null;
                ImGuiStylePtr styleCache = style;
                PropertyInfo[] properties = typeof(ImGuiStylePtr).GetProperties(BindingFlags.Instance | BindingFlags.Public);

                Dictionary<string, object> data = new Dictionary<string, object>();
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo property = properties[i];
                    if (!property.CanRead)
                        continue;

                    object? o = property.GetValue(styleCache);

                    if (o is null)
                        continue;

                    Type t = property.PropertyType;

                    if (t != typeof(float).MakeByRefType()
                        && t != typeof(Vector2).MakeByRefType()
                        && t != typeof(Vector4).MakeByRefType()
                        && t != typeof(bool).MakeByRefType())
                        continue;

                    data.Add(property.Name, o);
                }

                return data;
            }
            set
            {
                if (ImGui.GetCurrentContext() == IntPtr.Zero)
                    return;
                if (value is null)
                    return;

                ImGuiStylePtr styleCache = style;

                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new ClientConfig.VectorConverter());

                foreach (KeyValuePair<string, object> kvp in value)
                {
                    PropertyInfo? property = typeof(ImGuiStylePtr).GetProperty(kvp.Key, BindingFlags.Instance | BindingFlags.Public);

                    if (property is not null)
                    {
                        Type t = property.PropertyType;
                        MethodInfo? getMethod = property.GetMethod;
                        if (getMethod is null)
                            continue;

                        if (t == typeof(float).MakeByRefType())
                        {
                            float v = 0f;
                            if (kvp.Value.GetType() == typeof(double))
                                v = ((float)((double)kvp.Value));
                            else
                                v = (float)kvp.Value;
                            getMethod.CreateDelegate<FuncRefFloat>(styleCache)() = v;
                        }
                        else if (t == typeof(Vector2).MakeByRefType())
                        {
                            getMethod.CreateDelegate<FuncRefVector2>(styleCache)() = ((Newtonsoft.Json.Linq.JObject)kvp.Value).ToObject<Vector2>(serializer);
                        }
                        else if (t == typeof(Vector3).MakeByRefType())
                        {
                            getMethod.CreateDelegate<FuncRefVector3>(styleCache)() = ((Newtonsoft.Json.Linq.JObject)kvp.Value).ToObject<Vector3>(serializer);
                        }
                        else if (t == typeof(Vector4).MakeByRefType())
                        {
                            getMethod.CreateDelegate<FuncRefVector4>(styleCache)() = ((Newtonsoft.Json.Linq.JObject)kvp.Value).ToObject<Vector4>(serializer);
                        }
                        else if (t == typeof(bool).MakeByRefType())
                        {
                            getMethod.CreateDelegate<FuncRefBool>(styleCache)() = (bool)kvp.Value;
                        }
                    }
                }
            }
        }

        public void Set()
        {
            StyleColors = styleColors;
            StyleData = styleData;
        }

        public void Get()
        {
            styleColors = StyleColors;
            styleData = StyleData;
        }

        public Dictionary<string, Vector4> styleColors;
        public Dictionary<string, object> styleData;

        public ClientUIConfig()
        {

        }
    }
}
