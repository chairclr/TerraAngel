using System;
using System.Drawing;
using Microsoft.Xna.Framework.Input;

namespace TerraAngel.UI.ClientWindows;

public class StatsWindow : ClientWindow
{
    public override Keys ToggleKey => Keys.None;

    public override bool IsToggleable => false;

    public override bool IsEnabled { get => ClientConfig.Settings.ShowStatsWindow; }

    public override string Title => "Stat Window";

    public override bool IsGlobalToggle => false;

    public override void Draw(ImGuiIOPtr io)
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        ImDrawListPtr drawList = ImGui.GetForegroundDrawList();

        ImGui.PushFont(ClientAssets.GetMonospaceFont(16f));

        drawList.AddRectFilled(Vector2.Zero, new Vector2(io.DisplaySize.X, 16f + style.ItemSpacing.Y * 2f), ImGui.GetColorU32(ImGuiCol.WindowBg, 0.5f));
        drawList.AddRect(Vector2.Zero, new Vector2(io.DisplaySize.X, 16f + style.ItemSpacing.Y * 2f), ImGui.GetColorU32(ImGuiCol.WindowBg, 0.9f));

        string versionText = ClientLoader.TerraAngelVersion is null ? "" : $"v{ClientLoader.TerraAngelVersion} ";
        string fpsText = $"{Math.Round(1f / TimeMetrics.FramerateDeltaTimeSlices.Average.TotalSeconds),2:F0}";
        string upsText = $"{Math.Round(1f / TimeMetrics.UpdateDeltaTimeSlices.Average.TotalSeconds),2:F0}";

        drawList.AddText(Vector2.Zero + style.ItemSpacing, ImGui.GetColorU32(ImGuiCol.Text), $"{versionText}{fpsText} FPS {upsText} UPS");

        ImGui.PopFont();
    }
}