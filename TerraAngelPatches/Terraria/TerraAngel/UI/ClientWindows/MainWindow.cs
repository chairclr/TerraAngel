using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TerraAngel.ID;
using TerraAngel.WorldEdits;
using Terraria.TerraAngel.Tools.Developer;

namespace TerraAngel.UI.ClientWindows;

public class MainWindow : ClientWindow
{
    public override bool DefaultEnabled => true;

    public override bool IsToggleable => false;

    public override string Title => "Main Window";

    public override bool IsGlobalToggle => true;

    public override void Draw(ImGuiIOPtr io)
    {
        ImGui.PushFont(ClientAssets.GetMonospaceFont(16f));

        Vector2 windowSize = io.DisplaySize / new Vector2(3f, 2f);

        ImGui.SetNextWindowPos(new Vector2(0, io.DisplaySize.Y - windowSize.Y), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(windowSize, ImGuiCond.FirstUseEver);

        ImGui.Begin("Main window");

        if (!Main.gameMenu && Main.CanUpdateGameplay)
        {
            DrawInGameWorld(io);
        }
        else
        {
            DrawInMenu(io);
        }


        ImGui.End();

        ImGui.PopFont();
    }

    public void DrawInGameWorld(ImGuiIOPtr io)
    {
        if (ImGui.BeginTabBar("##MainTabBar"))
        {
            if (ImGui.BeginTabItem("Tools"))
            {
                if (ImGui.BeginTabBar("ToolBar"))
                {
                    if (ImGui.BeginTabItem("Main Tools"))
                    {
                        foreach (Tool cringe in ToolManager.GetToolsOfTab(ToolTabs.MainTools))
                        {
                            cringe.DrawUI(io);
                        }
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Items"))
                    {
                        if (ImGui.BeginTabBar("ItemBar"))
                        {
                            if (ImGui.BeginTabItem("Item Browser"))
                            {
                                ToolManager.GetTool<ItemBrowserTool>().DrawUI(io);
                                ImGui.EndTabItem();
                            }
                            ImGui.EndTabBar();
                        }
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Automation"))
                    {
                        foreach (Tool cringe in ToolManager.GetToolsOfTab(ToolTabs.AutomationTools))
                        {
                            cringe.DrawUI(io);
                        }
                        ImGui.EndTabItem();
                    }

                    foreach (Tool cringe in ToolManager.GetToolsOfTab(ToolTabs.NewTab))
                    {
                        if (ImGui.BeginTabItem(cringe.Name))
                        {
                            cringe.DrawUI(io);
                            ImGui.EndTabItem();
                        }
                    }

                    ImGui.EndTabBar();
                }
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Visuals"))
            {
                if (ImGui.BeginTabBar("VisualBar"))
                {
                    if (ImGui.BeginTabItem("Utility"))
                    {
                        foreach (Tool cringe in ToolManager.GetToolsOfTab(ToolTabs.VisualTools))
                        {
                            cringe.DrawUI(io);
                        }

                        if (ImGui.Button("Reveal Map"))
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    Stopwatch watch = Stopwatch.StartNew();
                                    int xlen = Main.Map.MaxWidth;
                                    int ylen = Main.Map.MaxHeight;
                                    for (int x = 0; x < xlen; x++)
                                    {
                                        for (int y = 0; y < ylen; y++)
                                        {
                                            if (Main.netMode == 0 || Main.tile.IsTileInLoadedSection(x, y))
                                            {
                                                Main.Map.Update(x, y, 255);
                                            }
                                        }
                                    }
                                    watch.Stop();
                                    ClientLoader.Console.WriteLine($"Map took {watch.Elapsed.Milliseconds}ms");
                                    Main.refreshMap = true;
                                }
                                catch (Exception e)
                                {
                                    ClientLoader.Console.WriteError($"{e}");
                                }
                            });
                        }
                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("ESP"))
                    {
                        foreach (Tool cringe in ToolManager.GetToolsOfTab(ToolTabs.ESPTools))
                        {
                            cringe.DrawUI(io);
                        }

                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("Lighting"))
                    {
                        foreach (Tool cringe in ToolManager.GetToolsOfTab(ToolTabs.LightingTools))
                        {
                            cringe.DrawUI(io);
                        }
                        ImGui.EndTabItem();
                    }
                    ImGui.EndTabBar();
                }
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("World Edit"))
            {
                if (ImGui.BeginTabBar("WorldEditBar"))
                {
                    for (int i = 0; i < ClientLoader.MainRenderer!.WorldEdits.Count; i++)
                    {
                        WorldEdit worldEdit = ClientLoader.MainRenderer.WorldEdits[i];
                        if (worldEdit.DrawUITab(io))
                        {
                            ClientLoader.MainRenderer.CurrentWorldEditIndex = i;
                        }
                    }
                    ImGui.EndTabBar();
                }
                ImGui.EndTabItem();
            }
            else
            {
                ClientLoader.MainRenderer!.CurrentWorldEditIndex = -1;
            }

            if (ImGui.BeginTabItem("Misc"))
            {
                foreach (Tool cringe in ToolManager.GetToolsOfTab(ToolTabs.MiscTools))
                {
                    cringe.DrawUI(io);
                }

                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
    }

    private int framesToShowUUIDFor = 0;
    public void DrawInMenu(ImGuiIOPtr io)
    {
        if (ImGui.BeginTabBar("##MainTabBar"))
        {
            if (ImGui.BeginTabItem("Tools"))
            {
                ImGui.Button($"{Icon.Refresh} Client UUID"); ImGui.SameLine();
                if (ImGui.Button("Click to reveal"))
                {
                    framesToShowUUIDFor = 600;
                }
                ImGui.SameLine();
                if (ImGui.Button("Click to copy"))
                {
                    ImGui.SetClipboardText(Main.clientUUID);
                }

                if (framesToShowUUIDFor > 0)
                {
                    framesToShowUUIDFor--;
                    ImGui.Text(Main.clientUUID);
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    public override void Update()
    {
        base.Update();
    }
}
