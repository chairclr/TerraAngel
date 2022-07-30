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

        public override void DrawPreviewInMap(ImGuiIOPtr io, ImDrawListPtr drawList)
        {
            Vector2 worldMouse = Util.ScreenToWorldFullscreenMap(InputSystem.MousePosition);
            Vector2 tileMouse = (worldMouse / 16f).Round();
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
            Vector2 tileMouse = (worldMouse / 16f).Round();
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
                ImGui.EndTabItem();
                return true;
            }
            return false;
        }

        public override void Edit(Vector2 cursorTilePosition)
        {

        }

        public void Copy(Vector2 startTile, Vector2 endTile)
        {
            float width = endTile.X - startTile.X;
            float height = endTile.Y - startTile.Y;
            CopiedSection = new TileSection(((int)startTile.X), ((int)startTile.Y), ((int)width), ((int)height));
        }
    }
}
