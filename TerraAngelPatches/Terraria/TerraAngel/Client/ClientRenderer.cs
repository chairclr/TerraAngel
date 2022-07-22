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
using TerraAngel.WorldEdits;
using TerraAngel.Cheat;
using System.Threading.Tasks;
using System.Linq;
using TerraAngel;

namespace TerraAngel.Client
{
    public class ClientRenderer : TerraImGuiRenderer
    {
        public List<ClientWindow> ClientWindows = new List<ClientWindow>();
        public List<WorldEdit> WorldEdits = new List<WorldEdit>() { new WorldEditBrush() };

        public int CurrentWorldEditIndex = -1;

        public WorldEdit CurrentWorldEdit
        {
            get
            {
                if (CurrentWorldEditIndex == -1)
                    return null;
                else
                    return WorldEdits[CurrentWorldEditIndex];
            }
        }

        public static int updateCount = 0;

        public bool GlobalUIState = true;


        public ClientRenderer(Game game) : base(game)
        {
            this.Init();
        }

        public void Init()
        {
            base.RebuildFontAtlas();
            AddWindow(new DrawWindow());
            AddWindow(new MainWindow());
            ConsoleSetup.SetConsoleInitialCommands(ClientLoader.ConsoleWindow = (ConsoleWindow)AddWindow(new ConsoleWindow()));
            AddWindow(new StatsWindow());
            AddWindow(new NetMessageWindow());


            Task.Run(() => ImGuiUtil.ItemLoaderThread(this));
            ItemBrowser.Init();

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
                ClientConfig.WriteToFile(ClientLoader.Config);
            }

            RealFNAIme.blocking = ImGui.GetIO().WantCaptureKeyboard || ImGui.GetIO().WantTextInput;

            if (Netplay.Connection.State <= 3 && CringeManager.LoadedTileSections != null)
            {
                CringeManager.LoadedTileSections = null;
            }

            foreach (Plugin.Plugin plugin in Plugin.PluginLoader.LoadedPlugins)
            {
                plugin.Update();
            }
        }

        public void Draw()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            if (InputSystem.IsKeyPressed(ClientLoader.Config.ToggleUIVisibility))
                GlobalUIState = !GlobalUIState;
            for (int i = 0; i < ClientWindows.Count; i++)
            {
                ClientWindow window = ClientWindows[i];
                if ((!window.IsToggleable || GlobalUIState) && window.IsEnabled)
                {
                    window.Draw(io);
                }
                window.Update();

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
