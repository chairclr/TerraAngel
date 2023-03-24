using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TerraAngel.Inspector.Tools;
using Terraria.GameContent;

namespace TerraAngel.Tools.Inspector;

public class ProjectileInspector : InspectorTool
{
    private static readonly Dictionary<int, nint> BoundProjectileTextures = new Dictionary<int, nint>();

    public override string Name => "Projectile Inspector";

    private int SelectedProjectileIndex = -1;

    private Projectile? SelectedProjectile => SelectedProjectileIndex > -1 ? Main.projectile[SelectedProjectileIndex] : null;

    public override void DrawMenuBar(ImGuiIOPtr io)
    {
        DrawProjectileSelectMenu(out _);

        if (SelectedProjectile is null)
        {
            return;
        }

        if (ImGui.Button($"{Icon.Move}"))
        {
            Main.LocalPlayer.velocity = Vector2.Zero;
            Main.LocalPlayer.Teleport(SelectedProjectile.position, TeleportationStyleID.RodOfDiscord);

            NetMessage.SendData(MessageID.PlayerControls, number: Main.myPlayer);

            if (ClientConfig.Settings.TeleportSendRODPacket)
            {
                NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null,
                    0,
                    Main.LocalPlayer.whoAmI,
                    Main.LocalPlayer.position.X,
                    Main.LocalPlayer.position.Y,
                    TeleportationStyleID.RodOfDiscord);
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text($"Teleport to \"{SelectedProjectile.Name.Truncate(30)}\"");
            ImGui.EndTooltip();
        }

        if (ImGui.Button($"{Icon.CircleSlash}"))
        {
            ClientLoader.Console.WriteError("Not implemented yet");
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text($"Kill \"{SelectedProjectile.Name.Truncate(30)}\"");
            ImGui.EndTooltip();
        }
    }

    public override void DrawInspector(ImGuiIOPtr io)
    {
        if (SelectedProjectile is null)
        {
            return;
        }

        string coolProjectileName = "None";

        if (Util.ProjectileFields.TryGetValue(SelectedProjectile.type, out FieldInfo? projectileField))
        {
            coolProjectileName = projectileField!.Name;
        }

        ImGui.Text($"Inspecting Projectile[{SelectedProjectileIndex}] \"{SelectedProjectile.Name.Truncate(60)}\"/{coolProjectileName}/{SelectedProjectile.type}");
        ImGui.Text($"Damage:    {SelectedProjectile.damage}");
        ImGui.Text($"Hostile:   {SelectedProjectile.hostile}");
        ImGui.Text($"Velocity:  {SelectedProjectile.velocity.Length():F4}");
        ImGui.Text($"Time Left: {SelectedProjectile.timeLeft}");
        ImGui.Text($"AI Style:  {SelectedProjectile.aiStyle}");
        for (int i = 0; i < Projectile.maxAI; i++)
        {
            ImGui.Text($"AI[{i}]:     {SelectedProjectile.ai[i]:F4}");
        }

        if (Main.netMode == 1 && SelectedProjectile.active)
        {
            ImGui.Text($"Owned By:  {SelectedProjectile.owner switch
            {
                >= 255 => "None/Server",
                >= 0 => $"{Main.player[SelectedProjectile.owner].name}",
                _ => "None/Server",
            }}/{SelectedProjectile.owner}");
        }

        if (SelectedProjectile.type > 0 && SelectedProjectile.type < TextureAssets.Projectile.Length)
        {
            Main.instance.LoadProjectile(SelectedProjectile.type);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            Texture2D tex = TextureAssets.Projectile[SelectedProjectile.type].Value;

            if (!BoundProjectileTextures.TryGetValue(SelectedProjectile.type, out nint texId))
            {
                texId = ClientLoader.MainRenderer!.BindTexture(tex);
                BoundProjectileTextures.Add(SelectedProjectile.type, texId);
            }

            Rectangle frame = tex.Frame(1, Main.projFrames[SelectedProjectile.type], 0, SelectedProjectile.frame);

            float scale = 1f;
            if (frame.Width > 256 || frame.Height > 256)
                scale = 256f / Math.Max(frame.Width, frame.Height);

            Vector2 uv1 = frame.TopLeft() / tex.Size();
            Vector2 uv2 = frame.BottomRight() / tex.Size();

            Vector2 frameSize = frame.Size();
            Vector2 center = ImGui.GetCursorScreenPos() + new Vector2(Math.Max(frame.Width, frame.Height) * scale) / 2f;
            Matrix3x2 rotationMat = Matrix3x2.CreateRotation(SelectedProjectile.rotation);

            Vector2[] positions = new Vector2[4]
            {
                center + Vector2.Transform(new Vector2(-frameSize.X / 2f, -frameSize.Y / 2f), rotationMat) * scale, // top left
                center + Vector2.Transform(new Vector2(+frameSize.X / 2f, -frameSize.Y / 2f), rotationMat) * scale, // top right
                center + Vector2.Transform(new Vector2(+frameSize.X / 2f, +frameSize.Y / 2f), rotationMat) * scale, // bottom right
                center + Vector2.Transform(new Vector2(-frameSize.X / 2f, +frameSize.Y / 2f), rotationMat) * scale, // bottom left
            };

            if (SelectedProjectile.direction == -1)
            {
                uv1.X = 1.0f - uv1.X;
                uv2.X = 1.0f - uv2.X;
            }

            drawList.AddImageQuad(texId, positions[0], positions[1], positions[2], positions[3], new Vector2(uv1.X, uv1.Y), new Vector2(uv2.X, uv1.Y), new Vector2(uv2.X, uv2.Y), new Vector2(uv1.X, uv2.Y));
        }
    }

    private void DrawProjectileSelectMenu(out bool showTooltip)
    {
        showTooltip = true;

        if (ImGui.BeginMenu("Projectiles"))
        {
            for (int i = 0; i < 1000; i++)
            {
                bool anyActiveProjectiles = false;
                for (int j = i; j < Math.Min(i + 40, 1000); j++)
                {
                    if (Main.projectile[j].active)
                        anyActiveProjectiles = true;
                }

                bool endedDisableEarly = false;
                ImGui.BeginDisabled(!anyActiveProjectiles);
                if (ImGui.BeginMenu($"Projectiles {i}-{Math.Min(i + 40, 1000)}"))
                {
                    endedDisableEarly = true;
                    showTooltip = false;
                    ImGui.EndDisabled();
                    for (int j = i; j < Math.Min(i + 40, 1000); j++)
                    {
                        if (!Main.projectile[j].active) ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.Text] * new Vector4(1f, 1f, 1f, 0.4f));

                        string coolProjcetileName = "None";

                        if (Util.ProjectileFields.TryGetValue(Main.projectile[j].type, out FieldInfo? npcField))
                        {
                            coolProjcetileName = npcField!.Name;
                        }

                        if (ImGui.MenuItem($"Projectile \"{Main.projectile[j].Name.Truncate(30)}\"/{coolProjcetileName}/{Main.projectile[j].type}"))
                        {
                            SelectedProjectileIndex = j;
                        }

                        if (!Main.projectile[j].active) ImGui.PopStyleColor();
                    }
                    ImGui.EndMenu();
                }
                if (!endedDisableEarly)
                    ImGui.EndDisabled();
                i += 20;
            }
            ImGui.EndMenu();
        }
    }
}
