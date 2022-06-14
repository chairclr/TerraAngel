using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;

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
    }
}
