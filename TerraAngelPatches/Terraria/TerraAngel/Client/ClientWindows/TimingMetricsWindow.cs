using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;
using Terraria.Map;

namespace TerraAngel.Client.ClientWindows;

public class TimingMetricsWindow : ClientWindow
{
    public override Keys ToggleKey => ClientConfig.Settings.ShowTimingMetrics;

    public override string Title => "Timing Metrics";

    public override bool DefaultEnabled => false;

    public override bool IsPartOfGlobalUI => false;

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
            if (ImGui.CollapsingHeader($"{text} ###{timerName}"))
            {
                TimeSpan[] timeSpans = new TimeSpan[time.Times.Count];
                time.Times.CopyTo(timeSpans, 0);
                float[] times = timeSpans.Select(x => (float)x.TotalMilliseconds).ToArray();
                float max = times.Max();
                float min = times.Min();

                if (max > time.LerpValue) time.LerpValue = max;
                max = (time.LerpValue = Utils.Lerp(time.LerpValue, max, 0.1f));

                Vector2 cursorPos = ImGui.GetCursorScreenPos();
                Vector2 regionAvail = ImGui.GetContentRegionAvail();
                float step = regionAvail.X / times.Length;

                float height = 100f - style.ItemSpacing.Y;
                drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(regionAvail.X, height), ImGui.GetColorU32(ImGuiCol.PopupBg));

                for (int j = 1; j < times.Length; j++)
                {
                    float x0 = (j - 1) * step;
                    float x1 = j * step;
                    float v0 = times[j - 1] / max;
                    float v1 = times[j] / max;

                    float f = min / max * height;
                    drawList.AddLine(cursorPos + new Vector2(x0, (1f - v0) * height + f), cursorPos + new Vector2(x1, (1f - v1) * height + f), Color.Red.PackedValue);
                }

                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 100f);
            }
        }

        ImGui.End();
    }
}
