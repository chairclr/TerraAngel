using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI.Chat;

namespace TerraAngel.UI.TerrariaUI;

public class UIAutoScaleTextTextPanel<T> : UIPanel
{
    public string Text => _text?.ToString() ?? string.Empty;

    public bool IsLarge { get; private set; }
    public bool DrawPanel { get; set; } = true;
    public float TextScaleMax { get; set; } = 1f;
    public float TextScale { get; set; } = 1f;
    public Vector2 TextSize { get; private set; } = Vector2.Zero;
    public Color TextColor { get; set; } = Color.White;

    private Rectangle oldInnerDimensions;
    private T _text = default;
    private string[] textStrings;
    private Vector2[] drawOffsets;

    public UIAutoScaleTextTextPanel(T text, float textScaleMax = 1f, bool large = false) : base()
    {
        SetText(text, textScaleMax, large);
    }

    public override void Recalculate()
    {
        base.Recalculate();
        SetText(_text, TextScaleMax, IsLarge);
    }

    public void SetText(T text)
    {
        SetText(text, TextScaleMax, IsLarge);
    }

    public virtual void SetText(T text, float textScaleMax, bool large)
    {
        Rectangle innerDimensionsRectangle = GetDimensions().ToRectangle();

        if (text.ToString() != _text?.ToString() || oldInnerDimensions != innerDimensionsRectangle)
        {
            oldInnerDimensions = innerDimensionsRectangle;

            TextScaleMax = textScaleMax;
            DynamicSpriteFont dynamicSpriteFont = large ? FontAssets.DeathText.Value : FontAssets.MouseText.Value;
            Vector2 textSize = ChatManager.GetStringSize(dynamicSpriteFont, text.ToString(), new Vector2(TextScaleMax));

            innerDimensionsRectangle.Inflate(-4, 0);

            Vector2 availableSpace = new Vector2(innerDimensionsRectangle.Width, innerDimensionsRectangle.Height);

            if (textSize.X > availableSpace.X || textSize.Y > availableSpace.Y)
            {
                float scale = (textSize.X / availableSpace.X > textSize.Y / availableSpace.Y) ? availableSpace.X / textSize.X : availableSpace.Y / textSize.Y;
                TextScale = scale;
                textSize = ChatManager.GetStringSize(dynamicSpriteFont, text.ToString(), new Vector2(TextScale));
            }
            else
            {
                TextScale = TextScaleMax;
            }
            innerDimensionsRectangle.Y += 8;
            innerDimensionsRectangle.Height -= 8;
            _text = text;
            TextSize = textSize;
            IsLarge = large;
            textStrings = text.ToString().Split('\n');
            drawOffsets = new Vector2[textStrings.Length];
            for (int i = 0; i < textStrings.Length; i++)
            {
                Vector2 size = ChatManager.GetStringSize(dynamicSpriteFont, textStrings[i], new Vector2(TextScale));
                //size.Y = size.Y * 0.9f;
                float x = (innerDimensionsRectangle.Width - size.X) * 0.5f;
                float y = (-textStrings.Length * size.Y * 0.5f) + i * size.Y + innerDimensionsRectangle.Height * 0.5f;
                drawOffsets[i] = new Vector2(x, y);
            }
        }
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (float.IsNaN(TextScale))
            Recalculate();

        if (DrawPanel)
            base.DrawSelf(spriteBatch);

        Rectangle innerDimensions = GetDimensions().ToRectangle();
        innerDimensions.Inflate(-4, 0);
        innerDimensions.Y += 8;
        innerDimensions.Height -= 8;
        for (int i = 0; i < textStrings.Length; i++)
        {
            Vector2 pos = innerDimensions.TopLeft() + drawOffsets[i];
            if (IsLarge)
                Utils.DrawBorderStringBig(spriteBatch, textStrings[i], pos, TextColor, TextScale, 0f, 0f, -1);
            else
                Utils.DrawBorderString(spriteBatch, textStrings[i], pos, TextColor, TextScale, 0f, 0f, -1);
        }
    }
}