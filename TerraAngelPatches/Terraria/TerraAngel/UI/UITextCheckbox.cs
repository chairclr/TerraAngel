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
    public unsafe class UITextCheckbox : UIText
    {
        private Func<bool> valueGet;
        private Action<bool> valueSet;

        public UITextCheckbox(string text, Func<bool> valueGet, Action<bool> valueSet, float textScale = 1.2f, bool large = false) : base(text, textScale, large)
        {
            this.valueGet = valueGet;
            this.valueSet = valueSet;
        }


        public override void OnInitialize()
        {
            base.OnInitialize();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            this.TextColor = valueGet() ? Color.Green : Color.Red;
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void MouseUp(UIMouseEvent evt)
        {
            base.MouseUp(evt);
            valueSet(!valueGet());
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }
}
