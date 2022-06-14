using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using TerraAngel.UI;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.ID;

namespace TerraAngel.Client.Config
{
    public class ClientConfig
    {
        public static bool Fake1 = false;
        public static bool Fake2 = false;
        public static bool Fake3 = false;
        public static bool Fake4 = false;

        public static List<UIElement> GetUITexts()
        {
            return new List<UIElement>() { 
                new UITextCheckbox("Fake",   () => Fake1, (v) => Fake1 = v), 
                new UITextCheckbox("Fake 2", () => Fake2, (v) => Fake2 = v),
                new UITextCheckbox("Fake 3", () => Fake3, (v) => Fake3 = v),
                new UITextCheckbox("Fake 4", () => Fake4, (v) => Fake4 = v),};
        }
    }
}
