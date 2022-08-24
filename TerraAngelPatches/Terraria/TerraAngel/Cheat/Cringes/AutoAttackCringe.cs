using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using TerraAngel.Client.Config;
using TerraAngel.Graphics;
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

        public ref bool FavorBosses => ref ClientConfig.Settings.AutoAttackFavorBosses;
        public ref bool TargetHostileNPCs => ref ClientConfig.Settings.AutoAttackTargetHostileNPCs;
        public ref bool RequireLineOfSight => ref ClientConfig.Settings.AutoAttackRequireLineOfSight;
        public ref bool VelocityPrediction => ref ClientConfig.Settings.AutoAttackVelocityPrediction;

        public ref float MinAttackRange => ref ClientConfig.Settings.AutoAttackMinTargetRange;
        public ref float VelocityPrectionScaling => ref ClientConfig.Settings.AutoAttackVelocityPredictionScaling;


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
                    ImGui.Checkbox("Velocity Prediction", ref VelocityPrediction);
                    if (VelocityPrediction)
                    {
                        ImGui.SliderFloat("Prediction Scaling", ref VelocityPrectionScaling, 1f, 30f);
                        
                    }

                    ImGui.Unindent(20f);
                }
            }
        }

        public Vector2 TargetPoint = Vector2.Zero;
        public bool wantToShoot = false;
        public bool LockedOnToTarget = false;
        public override void Update()
        {
            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
            if (Enabled && !Main.gameMenu)
            {
                Vector2 correctedPlayerCenter = Main.LocalPlayer.RotatedRelativePoint(Main.LocalPlayer.MountedCenter, reverseRotation: true);
                float minDist = float.MaxValue;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (npc.active)
                    {
                        if (npc.friendly)
                            continue;

                        if (!TargetHostileNPCs)
                            continue;

                        if (npc.immortal || npc.dontTakeDamage)
                            continue;

                        float distToPlayer = correctedPlayerCenter.Distance(npc.Center);

                        if (distToPlayer > MinAttackRange)
                            continue;

                        RaycastData raycast = Raycast.Cast(correctedPlayerCenter, (npc.Center - correctedPlayerCenter).Normalized(), distToPlayer + 1f);
                        if (RequireLineOfSight)
                        {
                            if (raycast.Hit)
                            {
                                continue;
                            }
                        }

                        Vector2 targetPoint = npc.Center;
                        if (VelocityPrediction)
                        {
                            float sp = CalcPlayerShootSpeed();
                            if (sp > 0 && (npc.velocity.X != 0 || npc.velocity.Y != 0))
                            {
                                float ttt = (raycast.Distance / sp) * VelocityPrectionScaling;
                                RaycastData tttCorrection = Raycast.Cast(npc.Center, (npc.velocity * ttt).Normalized(), (npc.velocity * ttt).Length() + 0.1f);
                                targetPoint = tttCorrection.End;
                            }
                        }

                        float d = targetPoint.DistanceSQ(correctedPlayerCenter);
                        if (d < minDist)
                        {
                            TargetPoint = targetPoint;
                            LockedOnToTarget = true;
                            minDist = d;
                        }
                    }
                }
                if (!Main.mapFullscreen && LockedOnToTarget) drawList.AddCircleFilled(Util.WorldToScreen(TargetPoint).ToNumerics(), 5f, Color.Red.PackedValue);
            }
        }

        public float CalcPlayerShootSpeed()
        {
            int projectileType = Main.LocalPlayer.HeldItem.shoot;
            float shootSpeed = Main.LocalPlayer.HeldItem.shootSpeed;

            if (Main.LocalPlayer.HeldItem.useAmmo > 0)
            {
                bool cs = true;
                int dm = 0;
                float kb = 0.0f;
                Main.LocalPlayer.PickAmmo(Main.LocalPlayer.HeldItem, ref projectileType, ref shootSpeed, ref cs, ref dm, ref kb, out _, true);
            }

            return shootSpeed;
        }
    }
}
