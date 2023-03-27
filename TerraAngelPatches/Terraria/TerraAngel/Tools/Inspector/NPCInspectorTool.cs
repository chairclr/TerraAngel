using System;
using TerraAngel.Inspector.Tools;
using Terraria.Graphics.Light;

namespace TerraAngel.Tools.Inspector;

public class NPCInspectorTool : InspectorTool
{
    public override string Name => "NPC Inspector";

    private int SelectedNPCIndex = -1;

    private NPC? SelectedNPC => SelectedNPCIndex > -1 ? Main.npc[SelectedNPCIndex] : null;

    private nint BoundNPCDrawTexture = 0;
    private RenderTarget2D? NPCDrawRenderTarget;

    public override void DrawMenuBar(ImGuiIOPtr io)
    {
        DrawNPCSelectMenu(out _);

        if (SelectedNPC is null)
        {
            return;
        }

        if (ImGui.Button($"{Icon.Move}"))
        {
            Main.LocalPlayer.velocity = Vector2.Zero;
            Main.LocalPlayer.Teleport(SelectedNPC.position, TeleportationStyleID.RodOfDiscord);

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
            ImGui.Text($"Teleport to \"{SelectedNPC.FullName.Truncate(30)}\"");
            ImGui.EndTooltip();
        }

        if (ImGui.Button($"{Icon.CircleSlash}"))
        {
            Butcher.ButcherNPC(SelectedNPC, ToolManager.GetTool<ButcherTool>().ButcherDamage);
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text($"Kill \"{SelectedNPC.FullName.Truncate(30)}\"");
            ImGui.EndTooltip();
        }

        //if (ImGui.Button($"{Icon.ScreenFull}"))
        //{
        //    DrawHooks.SpectateOverride = player.whoAmI;
        //}
        //if (ImGui.IsItemHovered())
        //{
        //    ImGui.BeginTooltip();
        //    ImGui.Text($"Spectate \"{player.name.Truncate(30)}\"");
        //    ImGui.EndTooltip();
        //}
    }

    public override void DrawInspector(ImGuiIOPtr io)
    {
        if (SelectedNPC is null)
        {
            return;
        }


        ImGui.Text($"Inspecting NPC[{SelectedNPCIndex}] \"{SelectedNPC.FullNameDefault.Truncate(60)}\"/{InternalRepresentation.GetNPCIDName(SelectedNPC.type)}/{SelectedNPC.type}");
        ImGui.Text($"Health:      {SelectedNPC.life}/{SelectedNPC.lifeMax}");
        ImGui.Text($"Defense:     {SelectedNPC.defense}");
        ImGui.Text($"Position:    {SelectedNPC.position}");
        ImGui.Text($"Speed:       {SelectedNPC.velocity.Length()}");
        ImGui.Text($"Velocity:    {SelectedNPC.velocity}");
        ImGui.Text($"Velocity Dir: ");

        if (SelectedNPC.velocity.Length() > 0f)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            Vector2 center = new Vector2(ImGui.GetItemRectMax().X, ImGui.GetItemRectMin().Y) + new Vector2(ImGui.GetItemRectSize().Y / 2f);
            Matrix3x2 rotationMatrix = Matrix3x2.CreateRotation(SelectedNPC.velocity.SafeNormalize(Vector2.Zero).AngleTo(Vector2.Zero) + MathF.PI / 2f);

            Vector2 head = center + Vector2.Transform(new Vector2(0f, ImGui.GetTextLineHeight() / 2f), rotationMatrix);
            Vector2 tail = center + Vector2.Transform(new Vector2(0f, -ImGui.GetTextLineHeight() / 2f), rotationMatrix);
            Vector2 tri1 = center + Vector2.Transform(new Vector2(0f, ImGui.GetTextLineHeight() / 2f), rotationMatrix);
            Vector2 tri2 = center + Vector2.Transform(new Vector2(ImGui.GetTextLineHeight() / 7f, ImGui.GetTextLineHeight() / 4f), rotationMatrix);
            Vector2 tri3 = center + Vector2.Transform(new Vector2(-ImGui.GetTextLineHeight() / 7f, ImGui.GetTextLineHeight() / 4f), rotationMatrix);

            drawList.AddLine(head, tail, Color.Red.PackedValue);
            drawList.AddTriangle(tri1, tri2, tri3, Color.Red.PackedValue);
            drawList.AddTriangle(tri1, tri2, tri3, Color.Red.PackedValue);
            drawList.AddTriangleFilled(tri1, tri2, tri3, Color.Red.PackedValue);
        }

        for (int i = 0; i < NPC.maxAI; i++)
        {
            ImGui.Text($"AI[{i}]:    {SelectedNPC.ai[i]}");
        }

        if (NPCDrawRenderTarget is not null && BoundNPCDrawTexture > 0 && !Main.gameMenu)
        {
            ImGui.Image(BoundNPCDrawTexture, NPCDrawRenderTarget.Size());
        }
    }

    private void DrawNPCSelectMenu(out bool showTooltip)
    {
        showTooltip = true;

        if (ImGui.BeginMenu("NPCs"))
        {
            for (int i = 0; i < 200; i++)
            {
                bool anyActiveNPCs = false;
                for (int j = i; j < Math.Min(i + 20, 200); j++)
                {
                    if (Main.npc[j].active)
                        anyActiveNPCs = true;
                }

                bool endedDisableEarly = false;
                ImGui.BeginDisabled(!anyActiveNPCs);
                if (ImGui.BeginMenu($"NPCs {i}-{Math.Min(i + 20, 200)}"))
                {
                    endedDisableEarly = true;
                    showTooltip = false;
                    ImGui.EndDisabled();
                    for (int j = i; j < Math.Min(i + 20, 200); j++)
                    {
                        if (!Main.npc[j].active) ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.Text] * new Vector4(1f, 1f, 1f, 0.4f));

                        if (ImGui.MenuItem($"NPC \"{Main.npc[j].FullNameDefault.Truncate(30)}\"/{InternalRepresentation.GetNPCIDName(Main.npc[j].type)}/{Main.npc[j].type}"))
                        {
                            SelectedNPCIndex = j;
                        }

                        if (!Main.npc[j].active) ImGui.PopStyleColor();
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

    public void PreDraw()
    {
        if (SelectedNPC is null)
        {
            return;
        }

        if (Main.gameMenu)
        {
            return;
        }

        if (NPCDrawRenderTarget is null ||
            NPCDrawRenderTarget.Width < SelectedNPC.width * 4 ||
            NPCDrawRenderTarget.Height < SelectedNPC.height * 4)
        {
            InvalidateDrawTexture();
        }

        try
        {
            ILightingEngine engine = Lighting._activeEngine;
            Lighting._activeEngine = Lighting.FullbrightEngine;
            Main.graphics.GraphicsDevice.SetRenderTarget(NPCDrawRenderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            Main.instance.DrawNPCDirect(Main.spriteBatch, SelectedNPC, SelectedNPC.behindTiles, SelectedNPC.position - SelectedNPC.Size);
            Main.spriteBatch.End();
            Lighting._activeEngine = engine;
        }
        catch (Exception ex)
        {
            ClientLoader.Console.WriteError(ex.ToString());
        }
    }

    private void InvalidateDrawTexture()
    {
        if (SelectedNPC is not null)
        {
            if (NPCDrawRenderTarget is not null)
            {
                ClientLoader.MainRenderer!.UnbindTexture(BoundNPCDrawTexture);
                NPCDrawRenderTarget.Dispose();
                NPCDrawRenderTarget = null;
            }

            NPCDrawRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, SelectedNPC.width * 4, SelectedNPC.height * 4);
            BoundNPCDrawTexture = ClientLoader.MainRenderer!.BindTexture(NPCDrawRenderTarget);
        }
    }

    public override void UpdateInGameSelect()
    {
        for (int i = 0; i < 200; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active)
            {
                Rectangle selectRect = new Rectangle((int)npc.Bottom.X - npc.frame.Width / 2, (int)npc.Bottom.Y - npc.frame.Height, npc.frame.Width, npc.frame.Height);
                if (npc.type >= 87 && npc.type <= 92)
                {
                    selectRect = new Rectangle((int)(npc.position.X + npc.width * 0.5 - 32.0), (int)(npc.position.Y + npc.height * 0.5 - 32.0), 64, 64);
                }

                if (InputSystem.RightMousePressed && selectRect.Contains(Util.ScreenToWorldWorld(InputSystem.MousePosition).ToPoint()))
                {
                    SelectedNPCIndex = i;
                    InspectorWindow.OpenTab(this);
                    break;
                }
            }
        }
    }
}
