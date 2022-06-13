using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;

namespace TerraAngel.Client.ClientWindows
{
    public class MainWindow : ClientWindow
    {
        public override Keys ToggleKey => Keys.OemTilde;

        private string testString = "";
        public override void Draw(ImGuiIOPtr io)
        {
            ImGui.Begin("Real window");

            ImGui.Text("This is a real window");
            ImGui.Text($"Framerate: {io.Framerate}");
            ImGui.InputText("fake", ref testString, 1000);
            ImGui.End();
        }
    }
}
