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
        public enum WorldEditActions
        {
            Break,
            Place,
            Replace,
            WallBreak,
            WallPlace,
            WallReplace,
        }
        private static string[] actionNames = Util.EnumFancyNames<WorldEditActions>();

        private bool sqaureFrame = true;
        private bool drawDetailedPreview = true;
        private int brushDiameter = 80;
        private bool teleportToTilesFarAway = true;

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
            Vector2 mousePos = Util.ScreenToWorld(Input.InputSystem.MousePosition);

            if (!drawDetailedPreview)
            {
                Vector2 screenCoords = Input.InputSystem.MousePosition;
                Vector2 screenCoords2 = Util.WorldToScreen((mousePos + new Vector2(brushDiameter + 16f, 0f)));
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

                    if ((tileCoords).Distance(mouseTileCoords) < bd)
                    {
                        Vector2 worldCoords = tileCoords.ToPoint().ToWorldCoordinates(0, 0);
                        Vector2 worldCoords2 = (tileCoords + Vector2.One).ToPoint().ToWorldCoordinates(0, 0);
                        drawList.AddRectFilled(Util.WorldToScreen(worldCoords).ToNumerics(), Util.WorldToScreen(worldCoords2).ToNumerics(), ImGui.GetColorU32(new System.Numerics.Vector4(1f, 0f, 0f, 0.5f)));
                    }

                }
            }
        }

        private int currentAction = 0;

        public override bool DrawUITab(ImGuiIOPtr io)
        {
            if (ImGui.BeginTabItem("Brush"))
            {
                ImGui.Checkbox("Draw detailed preview", ref drawDetailedPreview);
                ImGui.Checkbox("Square tile frame", ref sqaureFrame);
                ImGui.Checkbox("Attempt to bypass TShock", ref teleportToTilesFarAway);
                if (ImGui.SliderInt("Brush Diameter", ref brushDiameter, 16, 800))
                {
                    if (brushDiameter > 600)
                        drawDetailedPreview = false;
                    else
                        drawDetailedPreview = true;
                }
                ImGui.Text("Action"); ImGui.SameLine();
                ImGui.Combo("##WorldEditActions", ref currentAction, actionNames, actionNames.Length);

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

        public void Kernel(int x, int y)
        {
            if (!WorldGen.InWorld(x, y))
                return;

            Tile tile = Main.tile[x, y];

            if (tile == null)
                return;

            switch ((WorldEditActions)currentAction)
            {
                case WorldEditActions.Break:
                    KillTile(tile, x, y);
                    break;
                case WorldEditActions.Place:
                    PlaceTile(tile, currentPlayerCreateTile, x, y, false);
                    break;
                case WorldEditActions.Replace:
                    PlaceTile(tile, currentPlayerCreateTile, x, y, true);
                    break;
                case WorldEditActions.WallBreak:
                    KillWall(tile, x, y);
                    break;
                case WorldEditActions.WallPlace:
                    PlaceWall(tile, currentPlayerCreateWall, x, y, false);
                    break;
                case WorldEditActions.WallReplace:
                    PlaceWall(tile, currentPlayerCreateWall, x, y, true);
                    break;
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

        private Vector2 lastTeleportPosition;
        public void PlaceTile(Tile tile, int otherType, int x, int y, bool replace)
        {


            if (!tile.active() || (replace && tile.type != otherType))
            {
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
    }
}
