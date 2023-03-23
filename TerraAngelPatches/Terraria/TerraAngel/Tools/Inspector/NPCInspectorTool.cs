using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TerraAngel.Inspector.Tools;
using Terraria.GameContent;

namespace TerraAngel.Tools.Inspector;

public class NPCInspectorTool : InspectorTool
{
    public override string Name => "NPC Inspector";

    private int SelectedNPCIndex = -1;

    private NPC? SelectedNPC => SelectedNPCIndex > -1 ? Main.npc[SelectedNPCIndex] : null;

    public override void DrawMenuBar(ImGuiIOPtr io)
    {
        DrawNPCSelectMenu(out _);

        if (SelectedNPC is not null)
        {
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
    }

    public override void DrawInspector(ImGuiIOPtr io)
    {
        if (SelectedNPC is null)
        {
            return;
        }

        string coolNPCName = "None";

        if (Util.NPCFields.TryGetValue(SelectedNPC.type, out FieldInfo? npcField))
        {
            coolNPCName = npcField!.Name;
        }

        ImGui.Text($"Inspecting NPC \"{SelectedNPC.FullNameDefault.Truncate(60)}\"/{coolNPCName}/{SelectedNPC.type}");
        ImGui.Text($"Health:   {SelectedNPC.life.ToString().PadLeft(5),-7}/{SelectedNPC.lifeMax,5}");
        ImGui.Text($"Defense:  {SelectedNPC.defense,5}");
        ImGui.Text($"Velocity: {SelectedNPC.velocity.Length(),5}");
        for (int i = 0; i < NPC.maxAI; i++)
        {
            ImGui.Text($"AI[{i}]:  {SelectedNPC.ai[i],5:F4}");
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

                        string coolNPCName = "None";

                        if (!Util.NPCFields.TryGetValue(Main.npc[j].type, out FieldInfo? npcField))
                        {
                            coolNPCName = npcField!.Name;
                        }

                        if (ImGui.MenuItem($"NPC \"{Main.npc[j].FullNameDefault.Truncate(30)}\"/{coolNPCName}/{Main.npc[j].type}"))
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
}
