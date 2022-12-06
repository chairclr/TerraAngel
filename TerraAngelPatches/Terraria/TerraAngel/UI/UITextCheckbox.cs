using System;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace TerraAngel.UI;

public class UITextCheckbox : UIPanel
{
    private UIText nameText;
    private UIText otherText;
    private Func<bool> valueGet;
    private Action<bool> valueSet;
    public bool Colorize = true;

    public UITextCheckbox(string name, Func<bool> valueGet, Action<bool> valueSet, float textScale = .9f)
    {
        this.valueGet = valueGet;
        this.valueSet = valueSet;

        this.Width = new StyleDimension(0, 1f);
        this.Height = new StyleDimension(40, 0f);

        BorderColor = Color.Black;
        BackgroundColor = UIUtil.BGColor2;

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

        OnLeftClick += (e, _) =>
        {
            valueSet(!valueGet());
            this.otherText.SetText(valueGet().ToString());
            SoundEngine.PlaySound(SoundID.MenuTick);
        };

        Append(nameText);
        Append(otherText);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (Colorize) this.nameText.TextColor = this.otherText.TextColor = valueGet() ? Color.Green : Color.Red;

        base.Draw(spriteBatch);
    }
}
