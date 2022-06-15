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

namespace TerraAngel.Client.ClientWindows
{
    public class MainWindow : ClientWindow
    {
        public override Keys ToggleKey => Config.ClientConfig.Instance.ToggleUIVisibility;

        public override bool DefaultEnabled => true;

        public override bool IsToggleable => true;

        public override string Title => "Main Window";

        public override void Draw(ImGuiIOPtr io)
        {
            ImGui.PushFont(ClientAssets.GetMonospaceFont(18));

            NVector2 windowSize = io.DisplaySize / new NVector2(3f, 2f);
            
            ImGui.SetNextWindowPos(new NVector2(0, io.DisplaySize.Y - windowSize.Y), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.FirstUseEver);

            ImGui.Begin("Main window");

            if (!Main.gameMenu && Main.CanUpdateGameplay)
            {
                DrawInGameWorld(io);
            }
            else
            {
                DrawInMenu(io);
            }


            ImGui.End();

            ImGui.PopFont();
        }

        public void DrawInGameWorld(ImGuiIOPtr io)
        {
            if (ImGui.BeginTabBar("##MainTabBar"))
            {
                if (ImGui.BeginTabItem("Cheats"))
                {
                    ImGui.Checkbox("Anti-Hurt/Godmode", ref GlobalCheatManager.AntiHurt);
                    ImGui.Checkbox("Infinite Minions", ref GlobalCheatManager.InfiniteMinions);
                    ImGui.Checkbox("Infinite Mana", ref GlobalCheatManager.InfiniteMana);
                    ImGui.Checkbox("Freecam", ref GlobalCheatManager.Freecam);
                    ImGui.Checkbox("Noclip", ref GlobalCheatManager.NoClip);
                    if (ImGui.CollapsingHeader("Noclip Settings"))
                    {
                        ImGui.TextUnformatted("Speed"); ImGui.SameLine();
                        ImGui.SliderFloat("##Speed", ref GlobalCheatManager.NoClipSpeed, 1f, 128f);
                        ImGui.TextUnformatted("Frames between sync"); ImGui.SameLine();
                        ImGui.SliderInt("##SyncTime", ref GlobalCheatManager.NoClipPlayerSyncTime, 1, 60);
                    }
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Visuals"))
                {
                    ImGui.Checkbox("Fullbright", ref GlobalCheatManager.FullBright);
                    if (ImGui.CollapsingHeader("Fullbright Settings"))
                    {
                        ImGui.TextUnformatted("Brightness"); ImGui.SameLine();
                        float tmp = GlobalCheatManager.FullBrightBrightness * 100f;
                        if (ImGui.SliderFloat("##Brightness", ref tmp, 1f, 100f))
                        {
                            GlobalCheatManager.FullBrightBrightness = tmp / 100f;
                        }
                    }
                    ImGui.EndTabItem();
                }
            }
        }

        public void DrawInMenu(ImGuiIOPtr io)
        {

        }
    }
}
