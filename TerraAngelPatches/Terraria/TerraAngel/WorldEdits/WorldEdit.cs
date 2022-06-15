using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TerraAngel.WorldEdits
{
    public abstract class WorldEdit
    {
        public abstract bool RunEveryFrame { get; }

        public abstract bool DrawUITab(ImGuiIOPtr io);

        public abstract void DrawPreviewInWorld(ImGuiIOPtr io, ImDrawListPtr drawList);

        public abstract void DrawPreviewInMap(ImGuiIOPtr io, ImDrawListPtr drawList);

        public abstract void Edit(Vector2 cursorTilePosition);
    }
}
