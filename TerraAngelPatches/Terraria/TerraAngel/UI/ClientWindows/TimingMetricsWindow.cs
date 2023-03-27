using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace TerraAngel.UI.ClientWindows;

public class TimingMetricsWindow : ClientWindow
{
    public override Keys ToggleKey => ClientConfig.Settings.ShowTimingMetrics;

    public override string Title => "Timing Metrics";

    public override bool DefaultEnabled => false;

    public override bool IsGlobalToggle => false;

    public override bool IsToggleable => true;

    public override void Draw(ImGuiIOPtr io)
    {
        ImGui.PushFont(ClientAssets.GetMonospaceFont(16f));

        bool _enabled = true;
        ImGui.Begin(Title, ref _enabled);
        IsEnabled = _enabled;
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        ImGuiStylePtr style = ImGui.GetStyle();

        KeyValuePair<string, TimeMetrics.TimeLog>[] timeLogs = TimeMetrics.TimeLogs.ToArray();

        for (int i = 0; i < timeLogs.Length; i++)
        {
            string timerName = timeLogs[i].Key;
            TimeMetrics.TimeLog time = timeLogs[i].Value;

            float averageTime = (float)time.Average.TotalMilliseconds;
            string text = $"{timerName}: {averageTime:F3}ms";

            Vector2 cursorPosBefore = ImGui.GetCursorScreenPos();
            ImGui.SetCursorScreenPos(cursorPosBefore + new Vector2(ImGui.CalcTextSize(text).X + 30f, 0f));

            if (ImGui.Button($"{(time.Paused ? Icon.Play : Icon.DebugPause)}###pauseb{timerName}"))
            {
                time.Paused = !time.Paused;
            }

            ImGui.SameLine();

            if (ImGui.Button($"{(Icon.ClearAll)}###clearb{timerName}"))
            {
                time.Times.Clear();
                time.Sum = TimeSpan.Zero;
            }

            ImGui.SetCursorScreenPos(cursorPosBefore);

            if (ImGui.CollapsingHeader($"{text}###{timerName}"))
            {
                TimeSpan[] timespansArray = new TimeSpan[time.Times.Count];
                time.Times.CopyTo(timespansArray, 0);
                float[] times = timespansArray.Select(x => (float)x.TotalMilliseconds).ToArray();

                ImGuiUtil.DrawGraph($"graph{timerName}", 100f, times, 0f, time.MaxTimeCount, 0f, 30f, Color.Red, false, true);

                ImGuiUtil.GraphData data = ImGuiUtil.DrawGraphData[$"graph{timerName}"];
                ImGui.SliderFloat2($"##min{timerName}", ref data.YMinMax, 0f, 30f);
            }

            Vector2 cursorPosAfter = ImGui.GetCursorScreenPos();
            ImGui.SetCursorScreenPos(cursorPosBefore + new Vector2(ImGui.CalcTextSize(text).X + 30f, 0f));

            ImGui.Button($"{(time.Paused ? Icon.Play : Icon.DebugPause)}###pausebf{timerName}");
            ImGui.SameLine();
            ImGui.Button($"{(Icon.ClearAll)}###clearbf{timerName}");

            ImGui.SetCursorScreenPos(cursorPosAfter);

        }

        ImGui.End();
    }
}
