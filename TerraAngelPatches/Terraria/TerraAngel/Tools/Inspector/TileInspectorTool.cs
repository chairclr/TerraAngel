using System;
using TerraAngel.Inspector.Tools;

namespace TerraAngel.Tools.Inspector;

public class TileInspectorTool : InspectorTool
{
    public override string Name => "Tile Inspector";

    private Vector2i SelectedTilePosition = new Vector2i(-1, -1);

    private Tile? SelectedTile => SelectedTilePosition.X < 0 || SelectedTilePosition.Y < 0 || SelectedTilePosition.X >= Main.tile.Width || SelectedTilePosition.Y >= Main.tile.Height ? null : Main.tile[SelectedTilePosition];

    public override void DrawMenuBar(ImGuiIOPtr io)
    {
        ImGui.InputInt2("###TileSelectInput", ref SelectedTilePosition.X);

        SelectedTilePosition.X = Math.Clamp(SelectedTilePosition.X, 0, (int)(Main.tile.Width - 1));
        SelectedTilePosition.Y = Math.Clamp(SelectedTilePosition.Y, 0, (int)(Main.tile.Height - 1));

        if (SelectedTile.HasValue)
        {
            if (ImGui.Button($"{Icon.Move}"))
            {
                Main.LocalPlayer.velocity = Vector2.Zero;
                Main.LocalPlayer.Teleport(SelectedTilePosition * 16f, TeleportationStyleID.RodOfDiscord);

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
                ImGui.Text($"Teleport to the selected tile");
                ImGui.EndTooltip();
            }
        }
    }

    public override void DrawInspector(ImGuiIOPtr io)
    {
        if (!SelectedTile.HasValue)
        {
            return;
        }

        ImGui.Text($"Tile Active:      {SelectedTile.Value.active()}");
        ImGui.Text($"Tile inActive:    {SelectedTile.Value.inActive()}");
        ImGui.Text($"Tile nactive:     {SelectedTile.Value.nactive()}");
        ImGui.Text($"Tile Type:        {InternalRepresentation.GetTileIDName(SelectedTile.Value.type)}/{SelectedTile.Value.type}");

        ImGui.Text($"Tile FrameX:     ");
        ImGui.SameLine();
        int x = SelectedTile.Value.frameX;
        ImGui.SetNextItemWidth(8f * 16f);
        ImGui.InputInt("###tfx", ref x);
        SelectedTile.Value.frameX = (short)Math.Clamp(x, short.MinValue, short.MaxValue);
        ImGui.Text($"Tile FrameY:     ");
        ImGui.SameLine();
        int y = SelectedTile.Value.frameY;
        ImGui.SetNextItemWidth(8f * 16f);
        ImGui.InputInt("###tfy", ref y);
        SelectedTile.Value.frameY = (short)Math.Clamp(y, short.MinValue, short.MaxValue);

        ImGui.Text($"Tile Fame Number: {SelectedTile.Value.frameNumber()}");
        ImGui.Text($"Tile Slope:       {SelectedTile.Value.slope()}");
        ImGui.Text($"Tile Halfbrick:   {SelectedTile.Value.halfBrick()}");
        ImGui.Text($"Tile Paint:       {InternalRepresentation.GetPaintIDName(SelectedTile.Value.color())}/{SelectedTile.Value.color()}");
        if (SelectedTile.Value.fullbrightBlock() && SelectedTile.Value.invisibleBlock())
        {
            ImGui.Text($"Tile Coating:     Fullbright/Invisible");
        }
        else if (SelectedTile.Value.fullbrightBlock())
        {
            ImGui.Text($"Tile Coating:     Fullbright");
        }
        else if (SelectedTile.Value.invisibleBlock())
        {
            ImGui.Text($"Tile Coating:     Invisible");
        }
        ImGui.Text($"Tile Coating:     None");
        ImGui.NewLine();
        ImGui.Text($"Wall Type:        {InternalRepresentation.GetWallIDName(SelectedTile.Value.wall)}/{SelectedTile.Value.wall}");
        ImGui.Text($"Wall Paint:       {InternalRepresentation.GetPaintIDName(SelectedTile.Value.wallColor())}/{SelectedTile.Value.wallColor()}");
        if (SelectedTile.Value.fullbrightWall() && SelectedTile.Value.invisibleWall())
        {
            ImGui.Text($"Wall Coating:     Fullbright/Invisible");
        }
        else if (SelectedTile.Value.fullbrightWall())
        {
            ImGui.Text($"Wall Coating:     Fullbright");
        }
        else if (SelectedTile.Value.invisibleWall())
        {
            ImGui.Text($"Wall Coating:     Invisible");
        }
        ImGui.Text($"Wall Coating:     None");
        ImGui.Text($"Wall FrameX:      {SelectedTile.Value.wallFrameX()}");
        ImGui.Text($"Wall FrameY:      {SelectedTile.Value.wallFrameY()}");
        ImGui.Text($"Wall Fame Number: {SelectedTile.Value.wallFrameNumber()}");
        ImGui.NewLine();
        ImGui.Text($"Liquid Type:      {SelectedTile.Value.liquidType() switch
        {
            Tile.Liquid_Water => "Water",
            Tile.Liquid_Honey => "Honey",
            Tile.Liquid_Lava => "Lava",
            Tile.Liquid_Shimmer => "Shimmer",
            _ => "Invalid"
        }}/{SelectedTile.Value.liquid}");
        ImGui.Text($"Liquid Amount:    {SelectedTile.Value.liquid}");

        ImGui.SetNextItemWidth(8f * 16f);
        ImGui.InputInt("###stsx1", ref STSSizeX);
        ImGui.SameLine();
        ImGui.Text("x");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(8f * 16f);
        ImGui.InputInt("###stsy2", ref STSSizeY);

        if (ImGui.Button("STS"))
        {
            NetMessage.SendTileSquare(Main.myPlayer, SelectedTilePosition.X, SelectedTilePosition.Y, STSSizeX, STSSizeY);
        }

        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

        drawList.AddRect(Util.WorldToScreenDynamic(SelectedTilePosition * 16f), Util.WorldToScreenDynamic((SelectedTilePosition + new Vector2(STSSizeX, STSSizeY)) * 16f), Color.Yellow.PackedValue, 0f, ImDrawFlags.None, 2f);
    }

    private int STSSizeX = 1;
    private int STSSizeY = 1;

    public override void UpdateInGameSelect()
    {
        if (InputSystem.RightMousePressed)
        {
            Vector2i pos = Util.ScreenToWorldWorld(InputSystem.MousePosition) / 16f;
            if (Main.tile.InWorld(pos))
            {
                SelectedTilePosition = pos;
                InspectorWindow.OpenTab(this);
            }
        }
    }
}
