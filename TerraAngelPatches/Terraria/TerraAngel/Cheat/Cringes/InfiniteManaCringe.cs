using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraAngel.Cheat.Cringes
{
    public class InfiniteManaCringe : Cringe
    {
        public override string Name => "Infinite mana";

        public override CringeTabs Tab => CringeTabs.MainCringes;

        [DefaultConfigValue("DefaultInfiniteMana")]
        public override ref bool Enabled => ref base.Enabled;
    }
}
