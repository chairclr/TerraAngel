using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace TerraAngel.UI.TerrariaUI;

public class UITextSliderFloat : UIPanel
{
    private float value = 0f;
    private float min = default;
    private float max = default;
    private readonly Func<float> getValue;
    private readonly Func<string> getString;
    private readonly Action<float> setValue;
    private UIColoredSlider slider;
    private UIText text;

    public UITextSliderFloat(float min, float max, Func<float> getValue, Action<float> setValue, Func<string> getString)
    {
        this.min = min;
        this.max = max;
        this.getValue = getValue;
        this.getString = getString;
        this.setValue = setValue;

        value = (getValue() - this.min) / this.max;

        slider = new UIColoredSlider(Terraria.Localization.LocalizedText.Empty, () => value, x => { value = x; setValue(Util.Lerp(this.min, this.max, value)); }, () => { }, x => Color.White, Color.White)
        {
            VAlign = -1f,
            HAlign = 1f,
        };
        text = new UIText(getString())
        {
            VAlign = 0.5f,
            HAlign = 0f,
        };

        Append(slider);
        Append(text);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        text.SetText(getString());

        base.Draw(spriteBatch);
    }
}
public class UITextSliderInt : UIPanel
{
    private float value = 0f;
    private int min = default;
    private int max = default;
    private readonly Func<int> getValue;
    private readonly Func<string> getString;
    private readonly Action<int> setValue;
    private UIColoredSlider slider;
    private UIText text;

    public UITextSliderInt(int min, int max, Func<int> getValue, Action<int> setValue, Func<string> getString)
    {
        this.min = min;
        this.max = max;
        this.getValue = getValue;
        this.getString = getString;
        this.setValue = setValue;

        value = (getValue() - (float)this.min) / (float)this.max;

        slider = new UIColoredSlider(Terraria.Localization.LocalizedText.Empty, () => value, x => { value = x; setValue((int)MathF.Round(Util.Lerp((float)this.min, (float)this.max, value))); }, () => { }, x => Color.White, Color.White)
        {
            VAlign = -0.9f,
            HAlign = 1f,
        };

        text = new UIText(getString())
        {
            VAlign = 0.5f,
            HAlign = 0f,
        };

        Append(slider);
        Append(text);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        text.SetText(getString());

        base.Draw(spriteBatch);
    }
}