using Microsoft.Xna.Framework;
using TerraAngel.Hooks;
using TerraAngel.Graphics;
using TerraAngel.Input;
using TerraAngel.Client;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using ImGuiNET;
using TerraAngel.Client.ClientWindows;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using TerraAngel.Client.Config;

namespace TerraAngel.Client
{
    public class ClientRenderer : TerraImGuiRenderer
    {
        public List<ClientWindow> ClientWindows = new List<ClientWindow>();
        public static int updateCount = 0;

        public bool GlobalUIState = true;


        public ClientRenderer(Game game) : base(game)
        {
            this.Init();
        }

        public void Init()
        {
            base.RebuildFontAtlas();
            AddWindow(new MainWindow());
            ConsoleSetup.SetConsoleInitialCommands((ConsoleWindow)AddWindow(new ConsoleWindow()));
            AddWindow(new StatsWindow());

            unsafe
            {
                TextInputEXT.TextInput += (c) =>
                {
                    ImGuiIOPtr io = ImGui.GetIO();
                    if (io.NativePtr != null)
                    {
                        io.AddInputCharacter(c);
                    }
                };
            }
        }

        public void Render(GameTime time)
        {
            base.BeforeLayout(time);
            PreDraw();
            Draw();
            PostDraw();
            base.AfterLayout();
            
        }

        public void PreDraw()
        {
            InputSystem.EndUpdateInput();
            InputSystem.UpdateInput();
        }

        public void PostDraw()
        {

            if (ImGui.GetIO().WantCaptureKeyboard)
            {
                Main.ClosePlayerChat();
            }
            updateCount++;
            if (updateCount % 600 == 0)
            {
                ClientConfig.Instance.WriteToFile();
            }
        }

        public void Draw()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            if (InputSystem.IsKeyPressed(ClientConfig.Instance.ToggleUIVisibility))
                GlobalUIState = !GlobalUIState;
            foreach (ClientWindow window in ClientWindows)
            {
                if ((!window.IsToggleable || GlobalUIState) && window.IsEnabled)
                {
                    window.Draw(io);
                }

                if (window.IsToggleable && InputSystem.IsKeyPressed(window.ToggleKey))
                {
                    window.IsEnabled = !window.IsEnabled;

                    if (window.IsEnabled)
                        window.OnEnable();
                    else
                        window.OnDisable();
                }
            }
        }

        public ClientWindow AddWindow(ClientWindow window)
        {
            ClientWindows.Add(window);
            window.Init();
            window.IsEnabled = window.DefaultEnabled;
            return window;
        }
    }
}
