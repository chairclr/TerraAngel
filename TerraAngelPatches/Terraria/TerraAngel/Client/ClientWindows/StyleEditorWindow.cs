using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using TerraAngel.Hooks;
using Microsoft.Xna.Framework.Input;
using NVector2 = System.Numerics.Vector2;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TerraAngel.Cheat;
using TerraAngel.Graphics;
using TerraAngel.WorldEdits;
using TerraAngel;
using TerraAngel.Cheat.Cringes;
using TerraAngel.Utility;

namespace TerraAngel.Client.ClientWindows
{
    public class StyleEditorWindow : ClientWindow
    {
        public override bool DefaultEnabled => false;
        public override bool IsPartOfGlobalUI => true;
        public override bool IsToggleable => true;
        public override string Title => "Style Editor";
        public override Keys ToggleKey => ClientLoader.Config.ToggleStyleEditor;

        public static string[] ColorNames = Util.EnumFancyNames<ImGuiCol>();

        public override void Draw(ImGuiIOPtr io)
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            RangeAccessor<System.Numerics.Vector4> colors = style.Colors;

            ImGui.PushFont(ClientAssets.GetMonospaceFont(16f));

            bool open = IsEnabled;
            ImGui.Begin("Style Editor", ref open);
            IsEnabled = open;


            if (ImGui.CollapsingHeader("Colors"))
            {
                for (int i = 0; i < colors.Count; i++)
                {
                    ImGui.ColorEdit4(ColorNames[i], ref colors[i]);
                }
            }

            if (ImGui.CollapsingHeader("Window"))
            {
                ImGui.SliderFloat("Window Rounding", ref style.WindowRounding, 0f, 8f);
                ImGui.SliderFloat("Window Border Size", ref style.WindowBorderSize, 0f, 4f);
                ImGui.SliderFloat2("Window Padding", ref style.WindowPadding, 0f, 20f);
                ImGui.SliderFloat2("Window Title Align", ref style.WindowTitleAlign, 0f, 1f);
            }

            if (ImGui.CollapsingHeader("Misc"))
            {
                ImGui.SliderFloat("Alpha", ref style.Alpha, 0f, 1f);
                ImGui.SliderFloat("Grab Size", ref style.GrabMinSize, 0f, 8f);
                ImGui.SliderFloat("Grab Rounding", ref style.GrabRounding, 0f, 8f);
            }

            ImGui.End();

            ImGui.PopFont();
        }
    }
}
