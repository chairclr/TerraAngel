using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace TerraAngel.Inspector.Tools;

public abstract class InspectorTool : Tool
{
    public static InspectorWindow InspectorWindow = new InspectorWindow();

    public override ToolTabs Tab => ToolTabs.Inspector;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.PushFont(ClientAssets.GetMonospaceFont(16));

        if (ImGui.BeginMenuBar())
        {
            DrawMenuBar(io);
            ImGui.EndMenuBar();
        }

        DrawInspector(io);

        ImGui.PopFont();
    }

    public virtual void DrawMenuBar(ImGuiIOPtr io)
    {

    }

    public virtual void DrawInspector(ImGuiIOPtr io)
    {

    }

    public override void Update()
    {
        if (InputSystem.Ctrl && ClientConfig.Settings.CtrlRightClickOnObjectToInspect && !Main.mapFullscreen && !Main.gameMenu)
        {
            if (InspectorWindow.TypeOfTabToOpen is null)
            {
                UpdateInGameSelect();
            }
        }
    }

    public virtual void UpdateInGameSelect()
    {

    }
}
