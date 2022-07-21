using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace TerraAngel.Cheat.Cringes
{
    public class NoClipCringe : Cringe
    {
        public override string Name => "Noclip";

        public override CringeTabs Tab => CringeTabs.MainCheats;

        public float NoClipSpeed = 20.8f;
        public int NoClipPlayerSyncTime = 2;

        public override void DrawUI(ImGuiIOPtr io)
        {
            base.DrawUI(io);

            if (ImGui.CollapsingHeader("Noclip Settings"))
            {
                ImGui.TextUnformatted("Speed"); ImGui.SameLine();
                ImGui.SliderFloat("##Speed", ref NoClipSpeed, 1f, 100f);

                ImGui.TextUnformatted("Frames between sync"); ImGui.SameLine();
                ImGui.SliderInt("##SyncTime", ref NoClipPlayerSyncTime, 1, 60);
            }
        }
    }
}
