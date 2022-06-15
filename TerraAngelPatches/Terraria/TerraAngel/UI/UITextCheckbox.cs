using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.ID;

namespace TerraAngel.UI
{
    public class UITextCheckbox : UIElement
    {
        private UIText nameText;
        private UIText otherText;
        private UIPanel backgroundPanel;
        private Func<bool> valueGet;
        private Action<bool> valueSet;

        public UITextCheckbox(string name, Func<bool> valueGet, Action<bool> valueSet, float textScale = 1.2f) 
        {
            this.valueGet = valueGet;
            this.valueSet = valueSet;

            this.Width = new StyleDimension(0, 1f);
            this.Height = new StyleDimension(50, 0f);

            backgroundPanel = new UIPanel()
            {
                Width = { Percent = 1f },
                Height = { Percent = 1f },
                BorderColor = Color.Black,
                BackgroundColor = UIUtil.BGColor2
            };


            nameText = new UIText(name, textScale)
            {
                HAlign = 0f,
                VAlign = 0.5f
            };

            otherText = new UIText(valueGet().ToString(), textScale)
            {
                HAlign = 1f,
                VAlign = 0.5f
            };

            backgroundPanel.OnClick += (e, _) =>
            {
                valueSet(!valueGet());
                this.otherText.SetText(valueGet().ToString());
                SoundEngine.PlaySound(SoundID.MenuTick);
            };

            backgroundPanel.Append(nameText);
            backgroundPanel.Append(otherText);
            Append(backgroundPanel);

        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            this.nameText.TextColor = this.otherText.TextColor = valueGet() ? Color.Green : Color.Red;

            base.Draw(spriteBatch);
        }
    }
}
