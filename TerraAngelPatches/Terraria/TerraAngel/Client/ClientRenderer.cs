using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using TerraAngel.Client.ClientWindows;
using TerraAngel.WorldEdits;
using static Terraria.WorldBuilding.Searches;

namespace TerraAngel.Client
{
    public class ClientRenderer : TerraImGuiRenderer
    {
        public List<ClientWindow> ClientWindows = new List<ClientWindow>();
        public List<WorldEdit> WorldEdits = new List<WorldEdit>() { new WorldEditBrush(), new WorldEditCopyPaste() };

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

        public bool ShowMetricsWindow = false;
        public bool GlobalUIState = true;


        public ClientRenderer(Game game) : base(game)
        {
            this.Init();
        }

        public void Init()
        {
            TileUtil.Init();
            this.RebuildFontAtlas();
            AddWindow(new DrawWindow());
            AddWindow(new MainWindow());
            ConsoleSetup.SetConsoleInitialCommands(ClientLoader.ConsoleWindow = (ConsoleWindow)AddWindow(new ConsoleWindow()));
            AddWindow(new StatsWindow());
            AddWindow(new NetMessageWindow());
            AddWindow(new PlayerInspectorWindow());
            AddWindow(ClientLoader.ChatWindow = new ChatWindow());
            AddWindow(new StyleEditorWindow());

            ItemBrowser.Init();

            Main.instance.Exiting += (args, o) =>
            {
                ClientConfig.WriteToFile();
            };

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

        protected override void SetupInput()
        {
            base.SetupInput();

            ImGuiStylePtr style = ImGui.GetStyle();
            RangeAccessor<NVector4> colors = style.Colors;

            colors[(int)ImGuiCol.Text] = new NVector4(1.00f, 1.00f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled] = new NVector4(0.50f, 0.50f, 0.50f, 1.00f);
            colors[(int)ImGuiCol.WindowBg] = new NVector4(0.10f, 0.10f, 0.10f, 1.00f);
            colors[(int)ImGuiCol.ChildBg] = new NVector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.PopupBg] = new NVector4(0.19f, 0.19f, 0.19f, 0.92f);
            colors[(int)ImGuiCol.Border] = new NVector4(0.19f, 0.19f, 0.19f, 0.29f);
            colors[(int)ImGuiCol.BorderShadow] = new NVector4(0.00f, 0.00f, 0.00f, 0.24f);
            colors[(int)ImGuiCol.FrameBg] = new NVector4(0.05f, 0.05f, 0.05f, 0.54f);
            colors[(int)ImGuiCol.FrameBgHovered] = new NVector4(0.19f, 0.19f, 0.19f, 0.54f);
            colors[(int)ImGuiCol.FrameBgActive] = new NVector4(0.20f, 0.22f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.TitleBg] = new NVector4(0.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive] = new NVector4(0.06f, 0.06f, 0.06f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new NVector4(0.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.MenuBarBg] = new NVector4(0.14f, 0.14f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarBg] = new NVector4(0.05f, 0.05f, 0.05f, 0.54f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new NVector4(0.34f, 0.34f, 0.34f, 0.54f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new NVector4(0.40f, 0.40f, 0.40f, 0.54f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new NVector4(0.56f, 0.56f, 0.56f, 0.54f);
            colors[(int)ImGuiCol.CheckMark] = new NVector4(0.33f, 0.67f, 0.86f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab] = new NVector4(0.34f, 0.34f, 0.34f, 0.54f);
            colors[(int)ImGuiCol.SliderGrabActive] = new NVector4(0.56f, 0.56f, 0.56f, 0.54f);
            colors[(int)ImGuiCol.Button] = new NVector4(0.05f, 0.05f, 0.05f, 0.54f);
            colors[(int)ImGuiCol.ButtonHovered] = new NVector4(0.19f, 0.19f, 0.19f, 0.54f);
            colors[(int)ImGuiCol.ButtonActive] = new NVector4(0.20f, 0.22f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.Header] = new NVector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.HeaderHovered] = new NVector4(0.00f, 0.00f, 0.00f, 0.36f);
            colors[(int)ImGuiCol.HeaderActive] = new NVector4(0.20f, 0.22f, 0.23f, 0.33f);
            colors[(int)ImGuiCol.Separator] = new NVector4(0.28f, 0.28f, 0.28f, 0.29f);
            colors[(int)ImGuiCol.SeparatorHovered] = new NVector4(0.44f, 0.44f, 0.44f, 0.29f);
            colors[(int)ImGuiCol.SeparatorActive] = new NVector4(0.40f, 0.44f, 0.47f, 1.00f);
            colors[(int)ImGuiCol.ResizeGrip] = new NVector4(0.28f, 0.28f, 0.28f, 0.29f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new NVector4(0.44f, 0.44f, 0.44f, 0.29f);
            colors[(int)ImGuiCol.ResizeGripActive] = new NVector4(0.40f, 0.44f, 0.47f, 1.00f);
            colors[(int)ImGuiCol.Tab] = new NVector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.TabHovered] = new NVector4(0.14f, 0.14f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.TabActive] = new NVector4(0.20f, 0.20f, 0.20f, 0.36f);
            colors[(int)ImGuiCol.TabUnfocused] = new NVector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.TabUnfocusedActive] = new NVector4(0.14f, 0.14f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.DockingPreview] = new NVector4(0.33f, 0.67f, 0.86f, 1.00f);
            colors[(int)ImGuiCol.DockingEmptyBg] = new NVector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotLines] = new NVector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new NVector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram] = new NVector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogramHovered] = new NVector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TableHeaderBg] = new NVector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.TableBorderStrong] = new NVector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.TableBorderLight] = new NVector4(0.28f, 0.28f, 0.28f, 0.29f);
            colors[(int)ImGuiCol.TableRowBg] = new NVector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.TableRowBgAlt] = new NVector4(1.00f, 1.00f, 1.00f, 0.06f);
            colors[(int)ImGuiCol.TextSelectedBg] = new NVector4(0.20f, 0.22f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.DragDropTarget] = new NVector4(0.33f, 0.67f, 0.86f, 1.00f);
            colors[(int)ImGuiCol.NavHighlight] = new NVector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.NavWindowingHighlight] = new NVector4(1.00f, 0.00f, 0.00f, 0.70f);
            colors[(int)ImGuiCol.NavWindowingDimBg] = new NVector4(1.00f, 0.00f, 0.00f, 0.20f);
            colors[(int)ImGuiCol.ModalWindowDimBg] = new NVector4(1.00f, 0.00f, 0.00f, 0.35f);

            ClientConfig.Settings.UIConfig.Set();
        }

        public void Update(GameTime time)
        {

        }
        public void Render(GameTime time)
        {
            PreDraw(time);
            Draw();
            PostDraw();
        }

        public void PreDraw(GameTime time)
        {
            InputSystem.EndUpdateInput();
            InputSystem.UpdateInput();
            base.BeforeLayout(time);
        }

        public void PostDraw()
        {
            updateCount++;
            if (updateCount % 600 == 0)
            {
                ClientConfig.WriteToFile();
            }

            if (Main.netMode == 1 && Netplay.Connection.State <= 3)
            {
                Main.tile.LoadedTileSections = new bool[Main.tile.Width / Main.sectionWidth, Main.tile.Height / Main.sectionHeight];
            }

            if (!ClientConfig.Settings.UseDiscordRPC && ClientLoader.DiscordClient is not null)
            {
                ClientLoader.DiscordClient.Dispose();
                ClientLoader.DiscordClient = null;
            }
            if (ClientConfig.Settings.UseDiscordRPC && ClientLoader.DiscordClient is null)
            {
                ClientLoader.InitDiscord();
            }

            foreach (Plugin.Plugin plugin in Plugin.PluginLoader.LoadedPlugins)
            {
                plugin.Update();
            }

            if (ClientConfig.Settings.ClearChatThroughWorldChanges && Main.gameMenu)
            {
                ClientLoader.ChatWindow?.ChatItems.Clear();
            }

            base.AfterLayout();
        }

        public void Draw()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            if (InputSystem.IsKeyPressed(ClientConfig.Settings.ToggleUIVisibility) && !(InputSystem.IsKeyDown(Keys.LeftControl) || InputSystem.IsKeyDown(Keys.RightControl)))
                GlobalUIState = !GlobalUIState;
            for (int i = 0; i < ClientWindows.Count; i++)
            {
                ClientWindow window = ClientWindows[i];
                if ((!window.IsPartOfGlobalUI || GlobalUIState) && window.IsEnabled)
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

            if (ShowMetricsWindow)
            {
                ImGui.ShowMetricsWindow(ref ShowMetricsWindow);
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
