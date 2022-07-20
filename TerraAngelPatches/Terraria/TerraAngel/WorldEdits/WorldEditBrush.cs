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
using Microsoft.Xna.Framework;
using TerraAngel.Utility;
using System.Runtime.CompilerServices;
using TerraAngel.ID;
using TerraAngel.Net;

namespace TerraAngel.WorldEdits 
{
    public class WorldEditBrush : WorldEdit
    {
        public enum TileEditActions
        {
            None,
            Break,
            Place,
            Replace,
        }
        public enum WallEditActions
        {
            None,
            Break,
            Place,
            Replace,
        }
        public enum LiquidEditActions
        {
            None,
            Remove,
            Water,
            Lava,
            Honey
        }


        private static string[] tileActionNames = Util.EnumFancyNames<TileEditActions>();
        private static string[] wallActionNames = Util.EnumFancyNames<WallEditActions>();
        private static string[] liquidActionNames = Util.EnumFancyNames<LiquidEditActions>();

        private bool sqaureFrame = true;
        private bool drawDetailedPreview = true;
        private int brushDiameter = 80;
        private bool teleportToTilesFarAway = true;
        private bool revealMap = true;

        public override bool RunEveryFrame => true;

        public override void DrawPreviewInMap(ImGuiIOPtr io, ImDrawListPtr drawList)
        {
            Vector2 mousePos = Util.ScreenToWorldFullscreenMap(Input.InputSystem.MousePosition);

            if (!drawDetailedPreview)
            {
                Vector2 screenCoords = Input.InputSystem.MousePosition;
                Vector2 screenCoords2 = Util.WorldToScreenFullscreenMap((mousePos + new Vector2(brushDiameter + 16f, 0f)));
                float dist = screenCoords.Distance(screenCoords2);
                drawList.AddCircleFilled(screenCoords.ToNumerics(), dist, ImGui.GetColorU32(new System.Numerics.Vector4(1f, 0f, 0f, 0.5f)));
                return;
            }

            float bd = MathF.Floor(brushDiameter / 16f);
            Vector2 mouseTileCoords = new Vector2(MathF.Floor(mousePos.X / 16f), MathF.Floor(mousePos.Y / 16f));
            for (float x = mouseTileCoords.X - bd; x < mouseTileCoords.X + bd; x++)
            {
                for (float y = mouseTileCoords.Y - bd; y < mouseTileCoords.Y + bd; y++)
                {
                    Vector2 tileCoords = new Vector2(x, y);
                    if (x < 0 || x > Main.maxTilesX ||
                        y < 0 || y > Main.maxTilesY)
                        continue;

                    if (tileCoords.Distance(mouseTileCoords) < bd)
                    {
                        Vector2 worldCoords = tileCoords.ToPoint().ToWorldCoordinates(0, 0);
                        Vector2 worldCoords2 = (tileCoords + Vector2.One).ToPoint().ToWorldCoordinates(0, 0);
                        drawList.AddRectFilled(Util.WorldToScreenFullscreenMap(worldCoords).ToNumerics(), Util.WorldToScreenFullscreenMap(worldCoords2).ToNumerics(), ImGui.GetColorU32(new System.Numerics.Vector4(1f, 0f, 0f, 0.5f)));
                    }

                }
            }
        }
        public override void DrawPreviewInWorld(ImGuiIOPtr io, ImDrawListPtr drawList)
        {
            Vector2 mouseWorld = Util.ScreenToWorld(Input.InputSystem.MousePosition);

            if (!drawDetailedPreview)
            {
                Vector2 screenCoords = Input.InputSystem.MousePosition;
                Vector2 screenCoords2 = Util.WorldToScreen((mouseWorld + new Vector2(brushDiameter + 16f, 0f)));
                float dist = screenCoords.Distance(screenCoords2);
                drawList.AddCircleFilled(screenCoords.ToNumerics(), dist, ImGui.GetColorU32(new System.Numerics.Vector4(1f, 0f, 0f, 0.5f)));
                return;
            }

            float bd = MathF.Floor(brushDiameter / 16f);
            Vector2 mouseTileCoords = new Vector2(MathF.Floor(mouseWorld.X / 16f), MathF.Floor(mouseWorld.Y / 16f));
            for (float x = mouseTileCoords.X - bd; x < mouseTileCoords.X + bd; x++)
            {
                for (float y = mouseTileCoords.Y - bd; y < mouseTileCoords.Y + bd; y++)
                {
                    Vector2 tileCoords = new Vector2(x, y);
                    if (x < 0 || x > Main.maxTilesX ||
                        y < 0 || y > Main.maxTilesY)
                        continue;

                    if ((tileCoords).Distance(mouseTileCoords) < bd)
                    {
                        Vector2 worldCoords = tileCoords.ToPoint().ToWorldCoordinates(0, 0);
                        Vector2 worldCoords2 = (tileCoords + Vector2.One).ToPoint().ToWorldCoordinates(0, 0);
                        drawList.AddRectFilled(Util.WorldToScreen(worldCoords).ToNumerics(), Util.WorldToScreen(worldCoords2).ToNumerics(), ImGui.GetColorU32(new System.Numerics.Vector4(1f, 0f, 0f, 0.5f)));
                    }

                }
            }
        }

        private int currentTileAction = 0;
        private int currentWallAction = 0;
        private int currentLiquidAction = 0;

        public override bool DrawUITab(ImGuiIOPtr io)
        {
            if (ImGui.BeginTabItem("Brush"))
            {
                ImGui.Checkbox("Draw detailed preview", ref drawDetailedPreview);
                ImGui.Checkbox("Square tile frame", ref sqaureFrame);
                ImGui.Checkbox("Attempt to bypass TShock", ref teleportToTilesFarAway);
                ImGui.Checkbox("Reveal Map", ref revealMap);
                if (ImGui.SliderInt("Brush Diameter", ref brushDiameter, 16, 800))
                {
                    if (brushDiameter > 600)
                        drawDetailedPreview = false;
                    else
                        drawDetailedPreview = true;
                }
                ImGui.Text("Tile"); ImGui.SameLine();
                ImGui.Combo("##WorldEditTileActions",   ref currentTileAction,   tileActionNames,   tileActionNames.Length);
                ImGui.Text("Wall"); ImGui.SameLine();
                ImGui.Combo("##WorldEditWallActions",   ref currentWallAction,   wallActionNames,   wallActionNames.Length);
                ImGui.Text("Liquid"); ImGui.SameLine();
                ImGui.Combo("##WorldEditLiquidActions", ref currentLiquidAction, liquidActionNames, liquidActionNames.Length);

                ImGui.EndTabItem();
                return true;
            }
            return false;
        }

        private int currentPlayerCreateTile;
        private int currentPlayerCreateWall;
        private bool needsResetPlayerPosition = false;
        
        public override void Edit(Vector2 mouseTileCoords)
        {
            lastTeleportPosition = Main.LocalPlayer.position;
            currentPlayerCreateTile = Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].createTile;
            currentPlayerCreateWall = Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].createWall;
            needsResetPlayerPosition = false;

            float bd = MathF.Floor(brushDiameter / 16f);
            for (float x = mouseTileCoords.X - bd; x < mouseTileCoords.X + bd; x++)
            {
                for (float y = mouseTileCoords.Y - bd; y < mouseTileCoords.Y + bd; y++)
                {
                    Vector2 tileCoords = new Vector2(x, y);
                    if (x < 0 || x > Main.maxTilesX ||
                        y < 0 || y > Main.maxTilesY)
                        continue;

                    if (tileCoords.Distance(mouseTileCoords) < bd)
                    {
                        Kernel((int)x, (int)y);
                    }
                }
            }

            if (needsResetPlayerPosition)
                SpecialNetMessage.SendData(MessageID.PlayerControls, null, Main.myPlayer, Main.LocalPlayer.position.X, Main.LocalPlayer.position.Y, (float)Main.LocalPlayer.selectedItem);
        }

        private Vector2 lastTeleportPosition;
        public void Kernel(int x, int y)
        {
            if (!WorldGen.InWorld(x, y))
                return;

            Tile tile = Main.tile[x, y];

            if (tile == null)
                return;


            switch ((TileEditActions)currentTileAction)
            {
                case TileEditActions.Break:
                    KillTile(tile, x, y);
                    break;
                case TileEditActions.Place:
                    PlaceTile(tile, currentPlayerCreateTile, x, y, false);
                    break;
                case TileEditActions.Replace:
                    PlaceTile(tile, currentPlayerCreateTile, x, y, true);
                    break;
            }

            switch ((WallEditActions)currentWallAction)
            {
                case WallEditActions.Break:
                    KillWall(tile, x, y);
                    break;
                case WallEditActions.Place:
                    PlaceWall(tile, currentPlayerCreateWall, x, y, false);
                    break;
                case WallEditActions.Replace:
                    PlaceWall(tile, currentPlayerCreateWall, x, y, true);
                    break;
            }

            switch ((LiquidEditActions)currentLiquidAction)
            {
                case LiquidEditActions.Remove:
                    KillLiquid(tile, x, y);
                    break;
                case LiquidEditActions.Water:
                    PlaceLiquid(tile, x, y, Tile.Liquid_Water);
                    break;
                case LiquidEditActions.Lava:
                    PlaceLiquid(tile, x, y, Tile.Liquid_Lava);
                    break;
                case LiquidEditActions.Honey:
                    PlaceLiquid(tile, x, y, Tile.Liquid_Honey);
                    break;
            }

            if (revealMap)
            {
                Main.Map.Update(x, y, 255);
                Main.refreshMap = true;
            }
        }

        public void KillTile(Tile tile, int x, int y)
        {
            if (WorldGen.CanKillTile(x, y))
            {
                tile.active(false);
                tile.type = 0;
                NetMessage.SendData(MessageID.TileManipulation, number: TileManipulationID.KillTileNoItem, number2: x, number3: y); 
                if (sqaureFrame)
                    WorldGen.SquareTileFrame(x, y);
            }
        }
        public void PlaceTile(Tile tile, int otherType, int x, int y, bool replace)
        {
            if (!tile.active() || (replace && tile.type != otherType))
            {
                if (otherType == -1)
                    return;
                if (MathF.Abs(x * 16f - lastTeleportPosition.X) > 26f * 16f || MathF.Abs(y * 16f - lastTeleportPosition.Y) > 26f * 16f)
                {
                    needsResetPlayerPosition = true;
                    lastTeleportPosition = new Vector2(x * 16f, y * 16f);
                    SpecialNetMessage.SendData(MessageID.PlayerControls, null, Main.myPlayer, x * 16f, y * 16f, (float)Main.LocalPlayer.selectedItem);
                }
                tile.active(true);
                tile.type = (ushort)otherType;

                NetMessage.SendData(MessageID.TileManipulation, number: TileManipulationID.PlaceTile, number2: x, number3: y, number4: otherType);

                if (sqaureFrame)
                    WorldGen.SquareTileFrame(x, y);
            }
        }

        public void KillWall(Tile tile, int x, int y)
        {
            if (tile.wall != 0)
            {
                tile.wall = 0;
                NetMessage.SendData(MessageID.TileManipulation, number: TileManipulationID.KillWall, number2: x, number3: y);
                if (sqaureFrame)
                    WorldGen.SquareWallFrame(x, y);
            }
        }
        public void PlaceWall(Tile tile, int otherType, int x, int y, bool replace)
        {
            if (tile.wall == 0 || (replace && tile.wall != otherType))
            {
                if (MathF.Abs(x * 16f - lastTeleportPosition.X) > 26f * 16f || MathF.Abs(y * 16f - lastTeleportPosition.Y) > 26f * 16f)
                {
                    needsResetPlayerPosition = true;
                    lastTeleportPosition = new Vector2(x * 16f, y * 16f);
                    SpecialNetMessage.SendData(MessageID.PlayerControls, null, Main.myPlayer, x * 16f, y * 16f, (float)Main.LocalPlayer.selectedItem);
                }

                tile.wall = (ushort)otherType;

                if (replace)
                    NetMessage.SendData(MessageID.TileManipulation, number: TileManipulationID.ReplaceWall, number2: x, number3: y, number4: otherType);
                else
                    NetMessage.SendData(MessageID.TileManipulation, number: TileManipulationID.PlaceWall, number2: x, number3: y, number4: otherType);

                if (sqaureFrame)
                    WorldGen.SquareWallFrame(x, y);
            }
        }

        public void KillLiquid(Tile tile, int x, int y)
        {
            if (tile.liquid != 0)
            {
                if (tile.liquidType() == Tile.Liquid_Water || tile.liquidType() == Tile.Liquid_Honey)
                {
                    SpecialNetMessage.SendInventorySlot(0, ItemID.SuperAbsorbantSponge);
                }
                else if (tile.liquidType() == Tile.Liquid_Lava)
                {
                    SpecialNetMessage.SendInventorySlot(0, ItemID.LavaAbsorbantSponge);
                }
                SpecialNetMessage.SendPlayerControl(new Vector2(x * 16f, y * 16f), 0);

                lastTeleportPosition = new Vector2(x * 16f, y * 16f);
                needsResetPlayerPosition = true;
                WorldGen.EmptyLiquid(x, y);

                SpecialNetMessage.SendInventorySlot(0, Main.LocalPlayer.inventory[0].netID, Main.LocalPlayer.inventory[0].stack, Main.LocalPlayer.inventory[0].prefix);
            }
        }

        public void PlaceLiquid(Tile tile, int x, int y, int liquid)
        {
            if (tile.liquidType() != liquid || tile.liquid != 255)
            {
                switch (liquid)
                {
                    case Tile.Liquid_Water:
                        SpecialNetMessage.SendInventorySlot(0, ItemID.WaterBucket);
                        break;
                    case Tile.Liquid_Lava:
                        SpecialNetMessage.SendInventorySlot(0, ItemID.LavaBucket);
                        break;
                    case Tile.Liquid_Honey:
                        SpecialNetMessage.SendInventorySlot(0, ItemID.HoneyBucket);
                        break;
                }

                SpecialNetMessage.SendPlayerControl(new Vector2(x * 16f, y * 16f), 0);

                lastTeleportPosition = new Vector2(x * 16f, y * 16f);
                needsResetPlayerPosition = true;
                WorldGen.PlaceLiquid(x, y, (byte)liquid, 255);

                SpecialNetMessage.SendInventorySlot(0, Main.LocalPlayer.inventory[0].netID, Main.LocalPlayer.inventory[0].stack, Main.LocalPlayer.inventory[0].prefix);
            }
        }
    }
}
