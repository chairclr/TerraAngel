using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;


namespace TerraAngel.Cheat.Cringes
{
    public class ESPBoxesCringe : Cringe
    {
        public override string Name => "ESP boxes";

        public override CringeTabs Tab => CringeTabs.VisualUtility;

        public Color LocalPlayerColor = new Color(0f, 1f, 0f);
        public Color OtherPlayerColor = new Color(1f, 0f, 0f);
    }
}
