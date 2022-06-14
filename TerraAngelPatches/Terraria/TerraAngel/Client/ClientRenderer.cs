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

namespace TerraAngel.Client
{
    public class ClientRenderer : TerraImGuiRenderer
    {
        public TerraInput InputSystem = new TerraInput();
        public List<ClientWindow> ClientWindows = new List<ClientWindow>();

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
            InputSystem.UpdateInput();
        }

        public void PostDraw()
        {
            InputSystem.EndUpdateInput();

            if (ImGui.GetIO().WantCaptureKeyboard)
            {
                Main.ClosePlayerChat();
            }
        }

        public void Draw()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            if (InputSystem.IsKeyPressed(Keys.OemTilde))
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
