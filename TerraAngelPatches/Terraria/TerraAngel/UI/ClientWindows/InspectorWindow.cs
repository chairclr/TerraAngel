using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Input;
using TerraAngel.Inspector.Tools;

namespace TerraAngel.UI.ClientWindows;

public class InspectorWindow : ClientWindow
{
    public override string Title => "Inspector";

    public override bool DefaultEnabled => false;

    public override bool IsToggleable => true;

    public override bool IsGlobalToggle => false;

    public override Keys ToggleKey => ClientConfig.Settings.ShowInspectorWindow;

    private Type? TypeOfTabToOpen;

    public override void Draw(ImGuiIOPtr io)
    {
        ImGui.PushFont(ClientAssets.GetMonospaceFont(16));
        bool closeWindow = true;
        ImGui.Begin(Title, ref closeWindow, ImGuiWindowFlags.MenuBar);

        if (!closeWindow)
        {
            IsEnabled = false;
            ImGui.End();
            ImGui.PopFont();
            return;
        }

        if (ImGui.BeginTabBar("InspectorTabBar"))
        {
            foreach (Tool tool in ToolManager.GetToolsOfTab(ToolTabs.Inspector))
            {
                Type toolType = tool.GetType();
                if (toolType.IsSubclassOf(typeof(InspectorTool)))
                {
                    ImGuiTabItemFlags flags = ImGuiTabItemFlags.None;

                    if (TypeOfTabToOpen is not null && TypeOfTabToOpen == toolType)
                    {
                        flags |= ImGuiTabItemFlags.SetSelected;
                        TypeOfTabToOpen = null;
                    }

                    if (ImGui.BeginTabItem(tool.Name, flags))
                    {
                        tool.DrawUI(io);
                        ImGui.EndTabItem();
                    }
                }
            }
        }

        ImGui.End();
        ImGui.PopFont();
    }

    public void OpenTab(InspectorTool tool)
    {
        TypeOfTabToOpen = tool.GetType();
    }
}
