using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraAngel.Cheat.Cringes
{
    public class DisableFramingCringe : Cringe
    {
        public override string Name => "Disable tile framing";

        public override CringeTabs Tab => CringeTabs.VisualUtility;

        [DefaultConfigValue("DefaultDisableTileFraming")]
        public override ref bool Enabled => ref base.Enabled;
    }
}
