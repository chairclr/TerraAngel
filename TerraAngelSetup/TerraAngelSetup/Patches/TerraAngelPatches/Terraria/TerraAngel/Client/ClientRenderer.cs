using Microsoft.Xna.Framework;
using TerraAngel.Hooks;
using TerraAngel.Graphics;
using TerraAngel.Input;
using TerraAngel.Cheat;
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

        public ClientRenderer(Game game) : base(game)
        {
            base.SetupContext();
            base.SetupGraphics(game);
            base.SetupInput();
            this.Init();
        }

        public void Init()
        {
            base.RebuildFontAtlas();
            ClientWindows.Add(new MainWindow());

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
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.SamplerStateForCursor, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            //Main.DrawCursor(Main.DrawThickCursor());
            //Main.spriteBatch.End();
            
        }

        public void PreDraw()
        {
            InputSystem.UpdateInput();
        }

        public void PostDraw()
        {
            InputSystem.EndUpdateInput();

            if (ImGui.GetIO().WantCaptureMouse && !Main.instance.IsMouseVisible)
            {
                Main.instance.IsMouseVisible = true;
            }
            if (ImGui.GetIO().WantCaptureKeyboard)
            {
                Main.ClosePlayerChat();
            }
        }

        public void Draw()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            foreach (ClientWindow window in ClientWindows)
            {
                if (window.IsEnabled)
                {
                    window.Draw(io);
                }

                if (InputSystem.IsKeyPressed(window.ToggleKey))
                {
                    window.IsEnabled = !window.IsEnabled;
                }
            }
        }
    }
}
