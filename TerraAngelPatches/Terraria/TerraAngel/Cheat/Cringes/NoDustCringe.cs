using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraAngel.Cheat.Cringes
{
    public class NoDustCringe : Cringe
    {
        public override string Name => "No dust";

        public override CringeTabs Tab => CringeTabs.VisualUtility;

        [DefaultConfigValue("DefaultNoDust")]
        public override ref bool Enabled => ref base.Enabled;
    }
}
