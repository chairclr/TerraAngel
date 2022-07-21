using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using TerraAngel.Input;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace TerraAngel.UI
{
    public class ResolutionUI : UIState, IHaveBackButtonCommand
    {
        private UIElement element;

        public void HandleBackButtonUsage()
        {
            Terraria.Main.menuMode = MenuID.VideoSettings;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        public override void OnInitialize()
        {
            element = new UIElement
            {
                Width = { Percent = 0.8f },
                MaxWidth = new StyleDimension(600, 0),
                Top = { Pixels = 220 },
                Height = { Pixels = -220, Percent = 1f },
                HAlign = 0.5f
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (InputSystem.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                HandleBackButtonUsage();
        }
    }
}
