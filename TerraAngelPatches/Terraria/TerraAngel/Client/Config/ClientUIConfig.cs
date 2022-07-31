using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using NVector2 = System.Numerics.Vector2;
using NVector3 = System.Numerics.Vector3;
using NVector4 = System.Numerics.Vector4;

namespace TerraAngel.Client.Config
{
    public class ClientUIConfig
    {
        [JsonIgnore]
        private ImGuiStylePtr style => ImGui.GetStyle();

        [JsonIgnore]
        public NVector4[] StyleColors
        {
            get
            {
                ImGuiStylePtr styleCache = style;

                NVector4[] colors = new NVector4[styleCache.Colors.Count];

                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = styleCache.Colors[i];
                }

                return colors;
            }
            set
            {
                if (value is null)
                    return;

                ImGuiStylePtr styleCache = style;

                for (int i = 0; i < value.Length; i++)
                {
                    styleCache.Colors[i] = value[i];
                }
            }
        }

        [JsonIgnore]
        public float Alpha { get => style.Alpha; set { if (value != nullValuef) style.Alpha = value; } }
        [JsonIgnore]
        public float WindowRounding { get => style.WindowRounding; set { if (value != nullValuef) style.WindowRounding = value; } }
        [JsonIgnore]
        public float WindowBorderSize { get => style.WindowBorderSize; set { if (value != nullValuef) style.WindowBorderSize = value; } }
        [JsonIgnore]
        public NVector2 WindowPadding { get => style.WindowPadding; set { if (value != nullValuev2) style.WindowPadding = value; } }
        [JsonIgnore]
        public NVector2 WindowTitleAlign { get => style.WindowTitleAlign; set { if (value != nullValuev2) style.WindowTitleAlign = value; } }
        [JsonIgnore]
        public float GrabSize { get => style.GrabMinSize; set { if (value != nullValuef) style.GrabMinSize = value; } }
        [JsonIgnore]
        public float GrabRounding { get => style.GrabRounding; set { if (value != nullValuef) style.GrabRounding = value; } }

        public static float nullValuef = float.MaxValue;
        public static NVector2 nullValuev2 = new NVector2(float.MaxValue);
        public static NVector3 nullValuev3 = new NVector3(float.MaxValue);
        public static NVector4 nullValuev4 = new NVector4(float.MaxValue);

        public void Set()
        {
            StyleColors = styleColors;
            Alpha = alpha;
            WindowRounding = windowRounding;
            WindowBorderSize = windowBorderSize;
            WindowPadding = windowPadding;
            WindowTitleAlign = windowTitleAlign;
            GrabSize = grabSize;
            GrabRounding = grabRounding;
        }

        public void Get()
        {
            styleColors = StyleColors;
            alpha = Alpha;
            windowRounding = WindowRounding;
            windowBorderSize = WindowBorderSize;
            windowPadding = WindowPadding;
            windowTitleAlign = WindowTitleAlign;
            grabSize = GrabSize;
            grabRounding = GrabRounding;
        }

        public NVector4[] styleColors;
        public float alpha;
        public float windowRounding;
        public float windowBorderSize;
        public NVector2 windowPadding;
        public NVector2 windowTitleAlign;
        public float grabSize;
        public float grabRounding;

        public ClientUIConfig()
        {
            alpha = nullValuef;
            windowRounding = nullValuef;
            windowBorderSize = nullValuef;
            windowPadding = nullValuev2;
            windowTitleAlign = nullValuev2;
            grabSize = nullValuef;
            grabRounding = nullValuef;
        }
    }
}
