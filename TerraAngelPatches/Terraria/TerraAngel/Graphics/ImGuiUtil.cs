using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;
using TerraAngel.Utility;

namespace TerraAngel.Graphics
{
    public class ImGuiUtil
    {
        public static void TextColored(string text, Color color)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, color.PackedValue);
            ImGui.TextUnformatted(text);
            ImGui.PopStyleColor();
        }
        public static void ColorEdit4(string label, ref Color color)
        {
            System.Numerics.Vector4 v4c = color.ToVector4().ToNumerics();
            if (ImGui.ColorEdit4(label, ref v4c))
            {
                color = new Color(v4c.ToXNA());
            }
        }
    }
}
