using ImGuiNET;
using Microsoft.Xna.Framework.Input;

namespace TerraAngel.Client
{
    public abstract class ClientWindow
    {
        public bool IsEnabled { get; set; }

        public abstract Keys ToggleKey { get; }

        public abstract void Draw(ImGuiIOPtr io);
    }
}
