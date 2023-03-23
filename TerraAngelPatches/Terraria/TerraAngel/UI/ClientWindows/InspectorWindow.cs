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
                if (tool.GetType().IsSubclassOf(typeof(InspectorTool)))
                {
                    if (ImGui.BeginTabItem(tool.Name))
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
}
