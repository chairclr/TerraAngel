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

namespace TerraAngel.Client.ClientWindows
{
    public class MainWindow : ClientWindow
    {
        public override Keys ToggleKey => ClientLoader.Config.ToggleUIVisibility;

        public override bool DefaultEnabled => true;

        public override bool IsToggleable => true;

        public override string Title => "Main Window";

        public override void Draw(ImGuiIOPtr io)
        {
            ImGui.PushFont(ClientAssets.GetMonospaceFont(16));

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
                    if (ImGui.BeginTabBar("CheatBar"))
                    {
                        if (ImGui.BeginTabItem("Main Cheats"))
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
                        if (ImGui.BeginTabItem("Butcher"))
                        {
                            if (ImGui.Button("Butcher All Hostile NPCs"))
                            {
                                Butcher.ButcherAllHostileNPCs(GlobalCheatManager.ButcherDamage);
                            }
                            ImGui.Checkbox("Auto-Butcher Hostiles", ref GlobalCheatManager.AutoButcherHostileNPCs);
                            if (ImGui.Button("Butcher All Friendly NPCs"))
                            {
                                Butcher.ButcherAllFriendlyNPCs(GlobalCheatManager.ButcherDamage);
                            }
                            if (ImGui.Button("Butcher All Players"))
                            {
                                Butcher.ButcherAllPlayers(GlobalCheatManager.ButcherDamage);
                            }
                            ImGui.SliderInt("Butcher Damage", ref GlobalCheatManager.ButcherDamage, 1, (int)short.MaxValue);

                            ImGui.EndTabItem();
                        }
                        if (ImGui.BeginTabItem("Items"))
                        {
                            if (ImGui.BeginTabBar("ItemBar"))
                            {
                                if (ImGui.BeginTabItem("Item Browser"))
                                {
                                    ItemBrowser.DrawBrowser();
                                    ImGui.EndTabItem();
                                }
                                ImGui.EndTabBar();
                            }
                            ImGui.EndTabItem();
                        }
                        ImGui.EndTabBar();
                    }
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Visuals"))
                {
                    if (ImGui.BeginTabBar("VisualBar"))
                    {
                        if (ImGui.BeginTabItem("Lighting/Dust"))
                        {
                            ImGui.Checkbox("Fullbright", ref GlobalCheatManager.FullBright);

                            ImGui.TextUnformatted("Brightness"); ImGui.SameLine();
                            float tmp = GlobalCheatManager.FullBrightBrightness * 100f;
                            if (ImGui.SliderFloat("##Brightness", ref tmp, 1f, 100f))
                            {
                                GlobalCheatManager.FullBrightBrightness = tmp / 100f;
                            }

                            ImGui.Checkbox("No Dust", ref GlobalCheatManager.NoDust);
                            ImGui.Checkbox("No Gore", ref GlobalCheatManager.NoGore);

                            ImGui.EndTabItem();
                        }
                        if (ImGui.BeginTabItem("Utility"))
                        {
                            ImGui.Checkbox("ESP Boxes", ref GlobalCheatManager.ESPBoxes);
                            ImGui.Checkbox("ESP Tracers", ref GlobalCheatManager.ESPTracers);
                            if (ImGui.CollapsingHeader("ESP Settings"))
                            {
                                ImGuiUtil.ColorEdit4("Local player box color", ref GlobalCheatManager.ESPBoxColorLocalPlayer);
                                ImGuiUtil.ColorEdit4("Other player box color", ref GlobalCheatManager.ESPBoxColorOthers);
                                ImGuiUtil.ColorEdit4("Tracer color", ref GlobalCheatManager.ESPTracerColor);
                            }
                            if (ImGui.Button("Reveal Map"))
                            {
                                int xlen = Main.Map.MaxWidth;
                                int ylen = Main.Map.MaxHeight;
                                Task.Run(async () =>
                                {
                                    ClientLoader.Console.WriteLine("Revealing map");
                                    lock (Main.tile)
                                    {
                                        for (int x = 0; x < xlen; x++)
                                        {
                                            for (int y = 0; y < ylen; y++)
                                            {
                                                if (Main.tile[x, y] != null && (x - 1 < 0 || Main.tile[x - 1, y] != null) && (x + 1 > xlen || Main.tile[x + 1, y] != null) && (y - 1 < 0 || Main.tile[x, y - 1] != null) && (y + 1 > ylen || Main.tile[x, y + 1] != null))
                                                {
                                                    Main.Map.Update(x, y, 255);
                                                }
                                            }
                                            if (x % Main.maxTilesX == Main.maxTilesX / 2)
                                            {
                                                ClientLoader.Console.WriteLine("50% revealed");
                                            }
                                        }
                                    }
                                    ClientLoader.Console.WriteLine("Map Revealed");
                                    Main.refreshMap = true;
                                });
                            }
                            ImGui.Checkbox("Show Tile Sections", ref GlobalCheatManager.ShowTileSectionBorders);
                            ImGui.EndTabItem();
                        }
                        ImGui.EndTabBar();
                    }
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("World Edit"))
                {
                    if (ImGui.BeginTabBar("WorldEditBar"))
                    {
                        for (int i = 0; i < ClientLoader.MainRenderer.WorldEdits.Count; i++)
                        {
                            WorldEdit worldEdit = ClientLoader.MainRenderer.WorldEdits[i];
                            if (worldEdit.DrawUITab(io))
                            {
                                ClientLoader.MainRenderer.CurrentWorldEditIndex = i;
                            }
                        }
                        ImGui.EndTabBar();
                    }
                    ImGui.EndTabItem();
                }
                else
                {
                    ClientLoader.MainRenderer.CurrentWorldEditIndex = -1;
                }
                
                if (ImGui.BeginTabItem("Misc"))
                {
                    if (ImGui.Checkbox("Nebula Spam", ref GlobalCheatManager.NebulaSpam))
                    {
                        if (GlobalCheatManager.NebulaSpam && GlobalCheatManager.NebulaSpamPower > 30)
                        {
                            GlobalCheatManager.NoDust = true;
                        }
                    }
                    if (ImGui.CollapsingHeader("Nebula Settings"))
                    {
                        if (ImGui.SliderInt("Nebula Spam Power", ref GlobalCheatManager.NebulaSpamPower, 1, 500))
                        {
                            if (GlobalCheatManager.NebulaSpam && GlobalCheatManager.NebulaSpamPower > 30)
                            {
                                GlobalCheatManager.NoDust = true;
                            }
                        }
                    }
                    ImGui.EndTabItem();
                }
            }
        }

        private int framesToShowUUIDFor = 0;
        public void DrawInMenu(ImGuiIOPtr io)
        {
            if (ImGui.BeginTabBar("##MainTabBar"))
            {
                if (ImGui.BeginTabItem("Cheats"))
                {
                    ImGui.Button($"{ClientAssets.IconFont.Refresh} Client UUID"); ImGui.SameLine();
                    if (ImGui.Button("Click to reveal"))
                    {
                        framesToShowUUIDFor = 600;
                    } ImGui.SameLine();
                    if (ImGui.Button("Click to copy"))
                    {
                        ImGui.SetClipboardText(Main.clientUUID);
                    }

                    if (framesToShowUUIDFor > 0)
                    {
                        framesToShowUUIDFor--;
                        ImGui.Text(Main.clientUUID);
                    }
                    ImGui.EndTabItem();
                }
                
            }
        }
    }
}
