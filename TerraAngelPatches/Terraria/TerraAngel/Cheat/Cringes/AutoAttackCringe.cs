using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;
using TerraAngel.Client.Config;
using TerraAngel.Utility;
using Terraria;
using Terraria.ID;
using static Terraria.GameContent.Skies.CreditsRoll.Actions;

namespace TerraAngel.Cheat.Cringes
{
    public class AutoAttackCringe : Cringe
    {

        public override string Name => "Auto-Attack";
        public override CringeTabs Tab => CringeTabs.AutomationCringes;

        public ref bool TargetBosses => ref ClientConfig.Settings.AutoAttackTargetBosses;
        public ref bool FavorBosses => ref ClientConfig.Settings.AutoAttackFavorBosses;
        public ref bool TargetHostileNPCs => ref ClientConfig.Settings.AutoAttackTargetHostileNPCs;
        public ref bool RequireLineOfSight => ref ClientConfig.Settings.AutoAttackRequireLineOfSight;

        public ref float MinAttackRange => ref ClientConfig.Settings.AutoAttackMinTargetRange;



        public override void DrawUI(ImGuiIOPtr io)
        {
            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
            base.DrawUI(io);

            if (Enabled)
            {
                if (ImGui.CollapsingHeader("Auto-Attack Settings"))
                {
                    ImGui.Indent(20f);
                    ImGui.SliderFloat("Minimum Target Range", ref MinAttackRange, 1f, 2000f);
                    if (ImGui.IsItemFocused())
                    {
                        if (Main.mapFullscreen)
                        {
                            drawList.AddCircleFilled(Util.WorldToScreenFullscreenMap(Main.LocalPlayer.Center).ToNumerics(), Util.WorldToScreenFullscreenMap(Main.LocalPlayer.Center).Distance(Util.WorldToScreenFullscreenMap(Main.LocalPlayer.Center + new Vector2(MinAttackRange, 0f))), Color.Red.WithAlpha(0.5f).PackedValue);
                        }
                        else
                        {
                            drawList.AddCircleFilled(Util.WorldToScreen(Main.LocalPlayer.Center).ToNumerics(), MinAttackRange, Color.Red.WithAlpha(0.5f).PackedValue);
                        }
                    }

                    ImGui.Checkbox("Require Line of Sight", ref RequireLineOfSight);
                    ImGui.Checkbox("[wip] Target Hostile NPCs", ref TargetHostileNPCs);
                    ImGui.Checkbox("[wip] Target Bosses", ref TargetBosses);
                    ImGui.Checkbox("[wip] Favor Bosses", ref FavorBosses);

                    ImGui.Unindent(20f);
                }
            }
        }
        public override void Update()
        {
            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

            if (Enabled)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (npc.active)
                    {
                        if (npc.friendly)
                            continue;

                        if (npc.immortal || npc.dontTakeDamage)
                            continue;

                        Vector2 correctedPlayerCenter = Main.LocalPlayer.RotatedRelativePoint(Main.LocalPlayer.MountedCenter, reverseRotation: true);
                        float distToPlayer = correctedPlayerCenter.Distance(npc.Center);

                        if (distToPlayer > MinAttackRange)
                            continue;

                        if (RequireLineOfSight)
                        {

                            RaycastData raycast = Raycast.Cast(correctedPlayerCenter, (npc.Center - correctedPlayerCenter).Normalized(), distToPlayer + 1f);

                            drawList.AddLine(Util.WorldToScreen(correctedPlayerCenter).ToNumerics(), Util.WorldToScreen(raycast.IntersectionPoint).ToNumerics(), Color.Red.PackedValue);

                            if (raycast.Intersects)
                            {
                                continue;
                            }
                        }

                        drawList.AddCircleFilled(Util.WorldToScreen(npc.Center).ToNumerics(), 5f, Color.Red.PackedValue);
                        break;
                    }
                }
            }
        }

        public bool PlayerUpdate()
        {
            if (Enabled)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (npc.active)
                    {
                        if (npc.friendly)
                            continue;

                        if (npc.immortal || npc.dontTakeDamage)
                            continue;

                        Vector2 correctedPlayerCenter = Main.LocalPlayer.RotatedRelativePoint(Main.LocalPlayer.MountedCenter, reverseRotation: true);
                        float distToPlayer = correctedPlayerCenter.Distance(npc.Center);

                        if (distToPlayer > MinAttackRange)
                            continue;

                        if (RequireLineOfSight)
                        {

                            RaycastData raycast = Raycast.Cast(correctedPlayerCenter, (npc.Center - correctedPlayerCenter).Normalized(), distToPlayer + 1f);

                            //drawList.AddLine(Util.WorldToScreen(correctedPlayerCenter).ToNumerics(), Util.WorldToScreen(raycast.IntersectionPoint).ToNumerics(), Color.Red.PackedValue);

                            if (raycast.Intersects)
                            {
                                continue;
                            }
                        }


                        int mx = Main.mouseX;
                        int my = Main.mouseY;

                        Main.mouseX = (int)npc.Center.X - (int)Main.screenPosition.X;
                        Main.mouseY = (int)npc.Center.Y - (int)Main.screenPosition.Y;

                        Main.LocalPlayer.controlUseItem = true;
                        bool arc = Main.LocalPlayer.HeldItem.autoReuse;
                        Main.LocalPlayer.HeldItem.autoReuse = true;
                        Main.LocalPlayer.ItemCheck(Main.LocalPlayer.whoAmI);
                        Main.LocalPlayer.HeldItem.autoReuse = arc;
                        NetMessage.SendData(MessageID.PlayerControls, number: Main.myPlayer);
                        Main.mouseX = mx;
                        Main.mouseY = my;

                        return true;
                        //drawList.AddCircleFilled(Util.WorldToScreen(npc.Center).ToNumerics(), 5f, Color.Red.PackedValue);
                        break;
                    }
                }
            }
            return false;
        }
    }
}
