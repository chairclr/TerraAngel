using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraAngel.Cheat.Cringes
{
    public class AntiHurtCringe : Cringe
    {
        public override string Name => "Anti-hurt/Godmode";

        public override CringeTabs Tab => CringeTabs.MainCringes;

        [DefaultConfigValue("DefaultAntiHurt")]
        public override ref bool Enabled => ref base.Enabled;

        public int FramesSinceLastLifePacket = 0;
    }
}
