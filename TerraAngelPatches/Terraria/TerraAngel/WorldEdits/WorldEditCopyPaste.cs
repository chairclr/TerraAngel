﻿using System;
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
using TerraAngel.Input;
using TerraAngel.Utility;

namespace TerraAngel.WorldEdits
{
    public class WorldEditCopyPaste : WorldEdit
    {
        public override bool RunEveryFrame => false;

        public TileSectionRenderer Renderer = new TileSectionRenderer();
        public TileSection? CopiedSection;
        private bool isCopying = false;
        private Vector2 startSelectTile;
        private string[] placeModes = Util.EnumFancyNames<PlaceMode>();
        private int currentPlaceMode = 0;
        private bool destroyTiles = false;

        public override void DrawPreviewInMap(ImGuiIOPtr io, ImDrawListPtr drawList)
        {
            Vector2 worldMouse = Util.ScreenToWorldFullscreenMap(InputSystem.MousePosition);
            Vector2 tileMouse = (worldMouse / 16f).Floor();
            if (InputSystem.IsKeyPressed(ClientLoader.Config.WorldEditSelectKey))
            {
                isCopying = true;

                startSelectTile = tileMouse;
            }

            if (isCopying)
            {
                if (InputSystem.IsKeyDown(ClientLoader.Config.WorldEditSelectKey))
                {
                    drawList.DrawTileRect(startSelectTile, tileMouse, new Color(1f, 0f, 0f, 0.5f).PackedValue);
                }

                if (InputSystem.IsKeyUp(ClientLoader.Config.WorldEditSelectKey))
                {
                    isCopying = false;
                    Copy(startSelectTile, tileMouse);
                }
            }
            else
            {
                if (CopiedSection is not null)
                {
                    Renderer.DrawPrimitiveMap(CopiedSection, tileMouse * 16f, Vector2.Zero, io.DisplaySize.ToXNA());
                }
            }
        }
        public override void DrawPreviewInWorld(ImGuiIOPtr io, ImDrawListPtr drawList)
        {
            Vector2 worldMouse = Util.ScreenToWorld(InputSystem.MousePosition);
            Vector2 tileMouse = (worldMouse / 16f).Floor();
            if (InputSystem.IsKeyPressed(ClientLoader.Config.WorldEditSelectKey))
            {
                isCopying = true;

                startSelectTile = tileMouse;
            }

            if (isCopying)
            {
                if (InputSystem.IsKeyDown(ClientLoader.Config.WorldEditSelectKey))
                {
                    drawList.DrawTileRect(startSelectTile, tileMouse, new Color(1f, 0f, 0f, 0.5f).PackedValue);
                }

                if (InputSystem.IsKeyUp(ClientLoader.Config.WorldEditSelectKey))
                {
                    isCopying = false;
                    Copy(startSelectTile, tileMouse);
                }
            }
            else
            {
                if (CopiedSection is not null)
                {
                    Renderer.DrawDetailed(CopiedSection, Util.WorldToScreen(tileMouse * 16f), Vector2.Zero, io.DisplaySize.ToXNA());
                }
            }
        }

        public override bool DrawUITab(ImGuiIOPtr io)
        {
            if (ImGui.BeginTabItem("Copy/Paste"))
            {
                ImGui.Checkbox("Destroy Tiles", ref destroyTiles);
                ImGui.Text("Place Mode"); ImGui.SameLine(); ImGui.Combo("##PlaceMode", ref currentPlaceMode, placeModes, placeModes.Length);
                ImGui.EndTabItem();
                return true;
            }
            return false;
        }

        public override void Edit(Vector2 cursorTilePosition)
        {
            if (CopiedSection is null)
                return;
            switch ((PlaceMode)currentPlaceMode)
            {
                case PlaceMode.SendTileRect:
                    EditSendTileRect(cursorTilePosition);
                    break;
                case PlaceMode.TileManipulation:
                    EditSendTileManipulation(cursorTilePosition);
                    break;
            }
        }

        private void EditSendTileRect(Vector2 originTile)
        {
            if (CopiedSection is null)
                return;

            int ox = (int)MathF.Round(originTile.X);
            int oy = (int)MathF.Round(originTile.Y);
            Task.Run(
                () =>
                {
                    Main.rand = new Terraria.Utilities.UnifiedRandom();
                    // pass one, for solid tiles
                    for (int x = 0; x < CopiedSection.Width; x++)
                    {
                        for (int y = CopiedSection.Height - 1; y > -1; y--)
                        {
                            if (!WorldGen.InWorld(ox + x, oy + y))
                                continue;

                            Tile? tile = Main.tile[ox + x, oy + y];
                            Tile? copiedTile = CopiedSection.Tiles?[x, y];

                            if (tile is null || copiedTile is null)
                                continue;

                            if (!(Main.tileSolid[copiedTile.type] &&
                                copiedTile.type != TileID.GolfTee &&
                                copiedTile.type != TileID.GolfHole &&
                                copiedTile.type != TileID.GolfCupFlag))
                                continue;

                            bool isCopiedTileEmpty = !(copiedTile.active() || copiedTile.wall > 0);
                            if (isCopiedTileEmpty && !destroyTiles)
                                continue;



                            tile.CopyFrom(copiedTile);
                        }
                    }

                    // pass two, for non solid tiles
                    for (int x = 0; x < CopiedSection.Width; x++)
                    {
                        for (int y = CopiedSection.Height - 1; y > -1; y--)
                        {
                            if (!WorldGen.InWorld(ox + x, oy + y))
                                continue;

                            Tile? tile = Main.tile[ox + x, oy + y];
                            Tile? copiedTile = CopiedSection.Tiles?[x, y];

                            if (tile is null || copiedTile is null)
                                continue;

                            if ((Main.tileSolid[copiedTile.type] &&
                                copiedTile.type != TileID.GolfTee &&
                                copiedTile.type != TileID.GolfHole &&
                                copiedTile.type != TileID.GolfCupFlag))
                                continue;

                            bool isCopiedTileEmpty = !(copiedTile.active() || copiedTile.wall > 0);
                            if (isCopiedTileEmpty && !destroyTiles)
                                continue;

                            tile.CopyFrom(copiedTile);
                        }
                    }

                    // pass three, for framing and syncing
                    for (int x = 0; x < CopiedSection.Width; x++)
                    {
                        for (int y = CopiedSection.Height - 1; y > -1; y--)
                        {
                            if (!WorldGen.InWorld(ox + x, oy + y))
                                continue;

                            Tile? tile = Main.tile[ox + x, oy + y];
                            Tile? copiedTile = CopiedSection.Tiles?[x, y];

                            if (tile is null || copiedTile is null)
                                continue;

                            WorldGen.SquareTileFrame(ox + x, oy + y);
                            WorldGen.SquareWallFrame(ox + x, oy + y);

                            NetMessage.SendTileSquare(Main.myPlayer, ox + x, oy + y);
                        }
                    }
                });
            
        }

        private void EditSendTileManipulation(Vector2 originTile)
        {

        }

        public void Copy(Vector2 startTile, Vector2 endTile)
        {
            float width = endTile.X - startTile.X;
            float height = endTile.Y - startTile.Y;

            for (int x = ((int)startTile.X); x < ((int)endTile.X); x++)
            {
                for (int y = ((int)startTile.Y); y < ((int)endTile.Y); y++)
                {
                    WorldGen.SquareTileFrame(x, y);
                    WorldGen.SquareWallFrame(x, y);
                }
            }

            CopiedSection = new TileSection(((int)startTile.X), ((int)startTile.Y), ((int)width), ((int)height));
        }

        enum PlaceMode
        {
            SendTileRect,
            TileManipulation
        }
    }
}