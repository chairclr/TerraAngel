using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraAngel.Cheat.Cringes
{
    public class InfiniteMinionCringe : Cringe
    {
        public override string Name => "Infinite minions";

        public override CringeTabs Tab => CringeTabs.MainCringes;

        [DefaultConfigValue(nameof(ClientConfig.Config.DefaultInfiniteMinions))]
        public bool Enabled;

        public override void DrawUI(ImGuiIOPtr io)
        {
            ImGui.Checkbox(Name, ref Enabled);
        }
    }
}
