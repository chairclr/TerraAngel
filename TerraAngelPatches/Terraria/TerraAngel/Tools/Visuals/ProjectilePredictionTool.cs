using System;
using TerraAngel.Physics;

namespace TerraAngel.Tools.Visuals;

public class ProjectilePredictionTool : Tool
{
    public override string Name => "Projectile Prediction";

    public override ToolTabs Tab => ToolTabs.VisualTools;

    public bool DrawPlayerProjectilePrediction = false;

    [DefaultConfigValue(nameof(ClientConfig.Config.DefaultDrawActiveProjectilePrediction))]
    public bool DrawActiveProjectilePrediction = false;

    [DefaultConfigValue(nameof(ClientConfig.Config.DefaultDrawFriendlyProjectilePrediction))]
    public bool DrawFriendlyProjectiles = false;

    [DefaultConfigValue(nameof(ClientConfig.Config.DefaultDrawHostileProjectilePrediction))]
    public bool DrawHostileProjectiles = true;

    public ref Color FriendlyDrawColor => ref ClientConfig.Settings.FriendlyProjectilePredictionDrawColor;
    public ref Color HostileDrawColor => ref ClientConfig.Settings.HostileProjectilePredictionDrawColor;
    public ref int MaxStepCount => ref ClientConfig.Settings.ProjectilePredictionMaxStepCount;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.Checkbox("Projectile Prediction", ref DrawActiveProjectilePrediction);

        if (DrawActiveProjectilePrediction)
        {
            if (ImGui.CollapsingHeader("Projectile Prediction Settings"))
            {
                ImGui.Indent();
                ImGui.Checkbox("Draw Friendly", ref DrawFriendlyProjectiles);
                ImGui.Checkbox("Draw Hostile", ref DrawHostileProjectiles);
                ImGuiUtil.ColorEdit4("Friendly Color", ref FriendlyDrawColor);
                ImGuiUtil.ColorEdit4("Hostile Color", ref HostileDrawColor);
                ImGui.SliderInt("Max Step Count", ref MaxStepCount, 100, 10000);
                ImGui.Unindent();
            }
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
                        case ProjectileID.PulseBolt:
                        case ProjectileID.MeteorShot:
                            StraightBouncingPrediction(projectile, drawList);
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
        Vector2 velocity = projectile.velocity;
        float maxDistance = (velocity * projectile.timeLeft).Length();
        RaycastData ray1 = Raycast.Cast(position, velocity.Normalized(), maxDistance);
        RaycastData ray2 = Raycast.Cast(position + projectile.Size, velocity.Normalized(), maxDistance);
        RaycastData ray;

        if (ray1.Distance <= ray2.Distance) ray = ray1;
        else ray = ray2;

        drawList.AddLine(Util.WorldToScreenWorld(projectile.Center), Util.WorldToScreenWorld(projectile.Center + ray.Direction * ray.Distance), projectile.hostile ? HostileDrawColor.PackedValue : FriendlyDrawColor.PackedValue);
    }

    public void StraightBouncingPrediction(Projectile projectile, ImDrawListPtr drawList)
    {
        Vector2 position = projectile.position;
        Vector2 startPosition = position;
        Vector2 velocity = projectile.velocity;
        int penetrate = projectile.penetrate;
        uint col = projectile.hostile ? HostileDrawColor.PackedValue : FriendlyDrawColor.PackedValue;
        bool flag = false;

        int stepCount = Math.Min(projectile.timeLeft, MaxStepCount);
        for (int i = 0; i < stepCount; i++)
        {
            Vector2 tileVelocity = Collision.TileCollision(position, velocity, projectile.width, projectile.height, true, true);
            if (tileVelocity != velocity)
            {
                bool f = false;
                if (tileVelocity.Y != velocity.Y)
                {
                    f = true;
                    velocity.Y = -velocity.Y;
                }
                else if (tileVelocity.X != velocity.X)
                {
                    f = true;
                    velocity.X = -velocity.X;
                }
                if (f)
                {

                    Vector2 bounceLastScreenPos = Util.WorldToScreenWorld(startPosition + (projectile.Size / 2f));
                    Vector2 bounceCurrentScreenPos = Util.WorldToScreenWorld(position + (projectile.Size / 2f));

                    drawList.AddLine(bounceLastScreenPos, bounceCurrentScreenPos, col);

                    startPosition = position;


                    penetrate--;
                    if (penetrate <= 0)
                        flag = true;
                }
            }

            Vector4 slopeVelocity = Collision.SlopeCollision(position, velocity, projectile.width, projectile.height, 0f, fall: true);

            if (position.X != slopeVelocity.X || position.Y != slopeVelocity.Y ||
                velocity.X != slopeVelocity.Z || velocity.Y != slopeVelocity.W)
            {

                bool f = false;
                if (slopeVelocity.W != velocity.Y)
                {
                    f = true;
                    velocity.Y = -velocity.Y;
                }
                else if (slopeVelocity.Z != velocity.X)
                {
                    f = true;
                    velocity.X = -velocity.X;
                }

                if (f)
                {
                    Vector2 bounceLastScreenPos = Util.WorldToScreenWorld(startPosition + (projectile.Size / 2f));
                    Vector2 bounceCurrentScreenPos = Util.WorldToScreenWorld(position + (projectile.Size / 2f));

                    drawList.AddLine(bounceLastScreenPos, bounceCurrentScreenPos, col);

                    startPosition = position;


                    penetrate--;
                    if (penetrate <= 0)
                        flag = true;
                }

            }

            Vector2 tempPos = position;
            tempPos.X = slopeVelocity.X;
            tempPos.Y = slopeVelocity.Y;
            position = tempPos + (position - tempPos);
            velocity.X = slopeVelocity.Z;
            velocity.Y = slopeVelocity.W;

            position += velocity;

            if (position.X < 0 || position.Y < 0 || position.X > Main.maxTilesX * 16f || position.Y > Main.maxTilesY * 16f)
                flag = true;

            if (flag)
                break;

        }

        if (!flag)
        {
            Vector2 lastScreenPos = Util.WorldToScreenWorld(startPosition + (projectile.Size / 2f));
            Vector2 currentScreenPos = Util.WorldToScreenWorld(position + (projectile.Size / 2f));

            drawList.AddLine(lastScreenPos, currentScreenPos, col);
        }
    }

    public void ArrowPrediction(Projectile projectile, ImDrawListPtr drawList)
    {
        float ai0 = projectile.ai[0];
        Vector2 position = projectile.position;
        Vector2 lastPosition = position;
        Vector2 velocity = projectile.velocity;

        int someVelocityCap = Math.Min(projectile.width, projectile.height);
        if (someVelocityCap < 3) someVelocityCap = 3;
        if (someVelocityCap > 16) someVelocityCap = 16;

        uint col = projectile.hostile ? HostileDrawColor.PackedValue : FriendlyDrawColor.PackedValue;
        Vector2 displaySize = ImGui.GetIO().DisplaySize;
        int stepCount = Math.Min(projectile.timeLeft, MaxStepCount);
        for (int i = 0; i < stepCount; i++)
        {
            ai0 += 1f;
            lastPosition = position;
            Vector2 lastVelocity = velocity;

            if (ai0 >= 15f)
            {
                ai0 = 15f;
                velocity.Y += 0.1f;
            }

            bool flag = false;

            Vector2 tempPos = position;

            float velocityLenSquared = velocity.LengthSquared();
            if (velocityLenSquared > (someVelocityCap * someVelocityCap))
            {
                float velocityLength = MathF.Sqrt(velocityLenSquared);
                Vector2 velocityDir = velocity / velocityLength;
                Vector2 trueVelocity = Vector2.Zero;

                int updateCount = 0;
                while (velocityLength > 0)
                {
                    updateCount++;
                    if (updateCount > 300)
                    {
                        break;
                    }

                    float currentVelocityCorrection = velocityLength;
                    if (currentVelocityCorrection > someVelocityCap)
                    {
                        currentVelocityCorrection = someVelocityCap;
                    }

                    velocityLength -= currentVelocityCorrection;

                    Vector2 correctedTileVelocity = Collision.TileCollision(tempPos, velocityDir * currentVelocityCorrection, projectile.width, projectile.height, true, true);
                    if (correctedTileVelocity != velocityDir * currentVelocityCorrection)
                        flag = true;
                    tempPos += correctedTileVelocity;
                    velocity = correctedTileVelocity;


                    Vector4 slopeCorrectionVelocity = Collision.SlopeCollision(tempPos, velocity, projectile.width, projectile.height, 0f, fall: true);

                    if (tempPos.X != slopeCorrectionVelocity.X || tempPos.Y != slopeCorrectionVelocity.Y ||
                        velocity.X != slopeCorrectionVelocity.Z || velocity.Y != slopeCorrectionVelocity.W)
                    {
                        flag = true;
                    }

                    tempPos.X = slopeCorrectionVelocity.X;
                    tempPos.Y = slopeCorrectionVelocity.Y;
                    velocity.X = slopeCorrectionVelocity.Z;
                    velocity.Y = slopeCorrectionVelocity.W;


                    trueVelocity += velocity;
                }

                velocity = trueVelocity;



                if (Math.Abs(velocity.X - lastVelocity.X) < 0.0001f)
                {
                    velocity.X = lastVelocity.X;
                }

                if (Math.Abs(velocity.Y - lastVelocity.Y) < 0.0001f)
                {
                    velocity.Y = lastVelocity.Y;
                }

                Vector4 finalSlopCorrection = Collision.SlopeCollision(tempPos, velocity, projectile.width, projectile.height, 0f, fall: true);

                if (tempPos.X != finalSlopCorrection.X || tempPos.Y != finalSlopCorrection.Y ||
                    velocity.X != finalSlopCorrection.Z || velocity.Y != finalSlopCorrection.W)
                {
                    flag = true;
                }

                tempPos.X = finalSlopCorrection.X;
                tempPos.Y = finalSlopCorrection.Y;
                velocity.X = finalSlopCorrection.Z;
                velocity.Y = finalSlopCorrection.W;
            }
            else
            {
                Vector2 tileVelocity = Collision.TileCollision(tempPos, velocity, projectile.width, projectile.height, true, true);
                if (tileVelocity != velocity)
                    flag = true;

                Vector4 slopeVelocity = Collision.SlopeCollision(tempPos, velocity, projectile.width, projectile.height, 0f, fall: true);

                if (tempPos.X != slopeVelocity.X || tempPos.Y != slopeVelocity.Y ||
                    velocity.X != slopeVelocity.Z || velocity.Y != slopeVelocity.W)
                {
                    flag = true;
                }

                tempPos.X = slopeVelocity.X;
                tempPos.Y = slopeVelocity.Y;
                velocity.X = slopeVelocity.Z;
                velocity.Y = slopeVelocity.W;
            }

            if (position.X < 0 || position.Y < 0 || position.X > Main.maxTilesX * 16f || position.Y > Main.maxTilesY * 16f)
                flag = true;

            position += velocity;
            Vector2 lastScreenPos = Util.WorldToScreenWorld(lastPosition + (projectile.Size / 2f));
            Vector2 currentScreenPos = Util.WorldToScreenWorld(position + (projectile.Size / 2f));

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
