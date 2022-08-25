using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraAngel.Cheat.Cringes
{
    public class HeldItemViewerCringe : Cringe
    {
        public override string Name => "Show held item";

        public override CringeTabs Tab => CringeTabs.VisualUtility;

        [DefaultConfigValue("DefaultShowHeldItem")]
        public override ref bool Enabled => ref base.Enabled;
    }
}
