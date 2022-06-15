using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TerraAngel.Input;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace TerraAngel.UI
{
    public class UIKeySelect : UIElement
    {
        private UIText nameText;
        private UIText keyText;
        private UIPanel backgroundPanel;
        private Func<Keys> valueGet;
        private Action<Keys> valueSet;
        private bool isSelectingKey = false;

        public UIKeySelect(string name, Func<Keys> valueGet, Action<Keys> valueSet, float textScale = .9f)
        {
            this.valueGet = valueGet;
            this.valueSet = valueSet;

            this.Width = new StyleDimension(0, 1f);
            this.Height = new StyleDimension(40, 0f);

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

            keyText = new UIText(valueGet() == Keys.None ? "<Unbound>" : valueGet().ToString(), textScale)
            {
                HAlign = 1f,
                VAlign = 0.5f
            };



            backgroundPanel.Append(nameText);
            backgroundPanel.Append(keyText);

            backgroundPanel.OnClick += (x, _) =>
            {
                SoundEngine.PlaySound(SoundID.MenuTick);

                isSelectingKey = !isSelectingKey;

                if (isSelectingKey)
                {
                    keyText.SetText("<Press Key>");
                    keyText.TextColor = new Color(2, 255, 2);
                }
                else
                {
                    if (this.valueGet() != Keys.None)
                        this.valueSet(Keys.None);
                    keyText.SetText(this.valueGet() == Keys.None ? "<Unbound>" : valueGet().ToString());
                    keyText.TextColor = this.valueGet() == Keys.None ? Color.DimGray : Color.White;
                }
            };

            Append(backgroundPanel);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (isSelectingKey)
            {
                foreach (Keys key in Enum.GetValues<Keys>())
                {
                    if (InputSystem.IsKeyDown(key))
                    {
                        valueSet(key);
                        isSelectingKey = false;
                        SoundEngine.PlaySound(SoundID.MenuTick);

                        keyText.SetText(valueGet() == Keys.None ? "<Unbound>" : valueGet().ToString());

                        keyText.TextColor = this.valueGet() == Keys.None ? Color.DimGray : Color.White;
                        break;
                    }
                }
            }
        }
    }
}
