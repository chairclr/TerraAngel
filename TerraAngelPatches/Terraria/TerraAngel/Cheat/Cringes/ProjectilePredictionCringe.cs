using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TerraAngel.Client.ClientWindows;
using TerraAngel.Graphics;
using TerraAngel.Hooks;
using TerraAngel.Input;
using TerraAngel.Utility;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using NVector2 = System.Numerics.Vector2;


namespace TerraAngel.Cheat.Cringes
{
    public class ProjectilePredictionCringe : Cringe
    {
        public override string Name => "Projectile Prediction";

        public override CringeTabs Tab => CringeTabs.VisualUtility;

        public bool DrawPlayerProjectilePrediction = false;
        public bool DrawActiveProjectilePrediction = true;
        public bool DrawFriendlyProjectiles = false;
        public bool DrawHostileProjectiles = true;

        public ref Color FriendlyDrawColor => ref ClientLoader.Config.FriendlyProjectilePredictionDrawColor;
        public ref Color HostileDrawColor => ref ClientLoader.Config.HostileProjectilePredictionDrawColor;

        public override void DrawUI(ImGuiIOPtr io)
        {
            ImGui.Checkbox("Projectile Prediction", ref DrawActiveProjectilePrediction);

            if (ImGui.CollapsingHeader("Projectile Prediction Settings"))
            {
                ImGui.Checkbox("Draw Friendly", ref DrawFriendlyProjectiles);
                ImGui.Checkbox("Draw Hostile", ref DrawHostileProjectiles);
                ImGuiUtil.ColorEdit4("Friendly Color", ref FriendlyDrawColor);
                ImGuiUtil.ColorEdit4("Hostile Color", ref HostileDrawColor);
            }
        }

        public override void Update()
        {
            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
            if (DrawActiveProjectilePrediction)
            {
                for (int i = 0; i < 1000; i++)
                {
                    Projectile projectile = Main.projectile[i];

                    if (projectile.active)
                    {
                        if (projectile.hostile && !DrawHostileProjectiles)
                            continue;
                        if (projectile.friendly && !DrawFriendlyProjectiles)
                            continue;

                        switch (projectile.type)
                        {
                            case ProjectileID.Bullet:
                            case ProjectileID.BulletHighVelocity:
                            case ProjectileID.BulletDeadeye:
                            case ProjectileID.CrystalBullet:
                            case ProjectileID.CursedBullet:
                            case ProjectileID.ExplosiveBullet:
                            case ProjectileID.IchorBullet:
                            case ProjectileID.NanoBullet:
                            case ProjectileID.PartyBullet:
                            case ProjectileID.SniperBullet:
                            case ProjectileID.VenomBullet:
                            case ProjectileID.MoonlordBullet:
                            case ProjectileID.JestersArrow:
                            case ProjectileID.RocketI:
                            case ProjectileID.RocketII:
                            case ProjectileID.RocketIII:
                            case ProjectileID.RocketIV:
                            case ProjectileID.RocketSkeleton:
                            case ProjectileID.SaucerLaser:
                            case ProjectileID.LaserMachinegunLaser:
                            case ProjectileID.StarCannonStar:
                            case ProjectileID.StarCloakStar:
                            case ProjectileID.Starfury:
                            case ProjectileID.StarWrath:
                            case ProjectileID.TerraBeam:
                            case ProjectileID.NightBeam:
                            case ProjectileID.LightBeam:
                            case ProjectileID.EyeBeam:
                            case ProjectileID.EyeLaser:
                            case ProjectileID.AmethystBolt:
                            case ProjectileID.AmberBolt:
                            case ProjectileID.EmeraldBolt:
                            case ProjectileID.FrostBoltStaff:
                            case ProjectileID.IceBolt:
                            case ProjectileID.SapphireBolt:
                            case ProjectileID.DiamondBolt:
                            case ProjectileID.EnchantedBeam:
                            case ProjectileID.InfernoFriendlyBolt:
                            case ProjectileID.EatersBite:
                            case ProjectileID.VampireKnife:
                            case ProjectileID.Stake:
                            case ProjectileID.ImpFireball:
                            case ProjectileID.BeeArrow:
                            case ProjectileID.InfluxWaver:
                            case ProjectileID.SeedlerThorn:
                            case ProjectileID.SkyFracture:
                            case ProjectileID.DeathLaser:
                            case ProjectileID.CursedFlameFriendly:
                            case ProjectileID.CursedFlameHostile:
                            case ProjectileID.GreenLaser:
                            case ProjectileID.MartianWalkerLaser:
                            case ProjectileID.MinecartMechLaser:
                            case ProjectileID.ScutlixLaser:
                            case ProjectileID.ScutlixLaserFriendly:
                            case ProjectileID.MiniRetinaLaser:
                            case ProjectileID.PinkLaser:
                            case ProjectileID.PurpleLaser:
                            case ProjectileID.RayGunnerLaser:
                            case ProjectileID.ZapinatorLaser:
                            case ProjectileID.UnholyTridentFriendly:
                            case ProjectileID.UnholyTridentHostile:
                            case ProjectileID.TorchGod:
                                StraightPrediction(projectile, drawList);
                                break;
                            default:
                                {
                                    if (projectile.arrow)
                                    {
                                        ArrowPrediction(projectile, drawList);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        public void StraightPrediction(Projectile projectile, ImDrawListPtr drawList)
        {
            Vector2 position = projectile.position;
            Vector2 startPosition = position;
            Vector2 velocity = projectile.velocity;
            for (int i = 0; i < projectile.timeLeft; i++)
            {
                Vector2 tileVelocity = Collision.TileCollision(position, velocity, projectile.width, projectile.height, true, true);
                bool flag = false;
                if (tileVelocity != velocity)
                    flag = true;

                Vector4 slopeVelocity = Collision.SlopeCollision(position, velocity, projectile.width, projectile.height, 0f, fall: true);

                if (position.X != slopeVelocity.X || position.Y != slopeVelocity.Y ||
                    velocity.X != slopeVelocity.Z || velocity.Y != slopeVelocity.W)
                {
                    flag = true;
                }

                Vector2 tempPos = position;
                tempPos.X = slopeVelocity.X;
                tempPos.Y = slopeVelocity.Y;
                position = tempPos + (position - tempPos);
                velocity.X = slopeVelocity.Z;
                velocity.Y = slopeVelocity.W;

                position += velocity;

                if (flag)
                    break;
            }

            NVector2 lastScreenPos = Util.WorldToScreen(startPosition + (projectile.Size / 2f)).ToNumerics();
            NVector2 currentScreenPos = Util.WorldToScreen(position + (projectile.Size / 2f)).ToNumerics();
            drawList.AddLine(lastScreenPos, currentScreenPos, projectile.hostile ? HostileDrawColor.PackedValue : FriendlyDrawColor.PackedValue);
        }

        public void ArrowPrediction(Projectile projectile, ImDrawListPtr drawList)
        {
            float ai0 = projectile.ai[0];
            Vector2 position = projectile.position;
            Vector2 lastPosition = position;
            Vector2 velocity = projectile.velocity;

            int num3 = Math.Min(projectile.width, projectile.height);
            if (num3 < 3) num3 = 3;
            if (num3 > 16) num3 = 16;

            uint col = projectile.hostile ? HostileDrawColor.PackedValue : FriendlyDrawColor.PackedValue;
            NVector2 displaySize = ImGui.GetIO().DisplaySize;
            for (int i = 0; i < projectile.timeLeft; i++)
            {
                ai0 += 1f;
                lastPosition = position;

                if (ai0 >= 15f)
                {
                    ai0 = 15f;
                    velocity.Y += 0.1f;
                }

                Vector2 tileVelocity = Collision.TileCollision(position, velocity, projectile.width, projectile.height, true, true);
                bool flag = false;
                if (tileVelocity != velocity)
                    flag = true;

                Vector4 slopeVelocity = Collision.SlopeCollision(position, velocity, projectile.width, projectile.height, 0f, fall: true);

                if (position.X != slopeVelocity.X || position.Y != slopeVelocity.Y ||
                    velocity.X != slopeVelocity.Z || velocity.Y != slopeVelocity.W)
                {
                    flag = true;
                }

                Vector2 tempPos = position;
                tempPos.X = slopeVelocity.X;
                tempPos.Y = slopeVelocity.Y;
                position = tempPos + (position - tempPos);
                velocity.X = slopeVelocity.Z;
                velocity.Y = slopeVelocity.W;

                position += velocity;
                NVector2 lastScreenPos = Util.WorldToScreen(lastPosition + (projectile.Size / 2f)).ToNumerics();
                NVector2 currentScreenPos = Util.WorldToScreen(position + (projectile.Size / 2f)).ToNumerics();

                // dont draw off screen W
                if (Util.IsRectOnScreen(lastScreenPos, currentScreenPos, displaySize))
                {
                    drawList.AddLine(lastScreenPos, currentScreenPos, col);
                }

                if (flag)
                    break;
            }
        }

    }
}
