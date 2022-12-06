using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace TerraAngel.UI;

public static class UIUtil
{
    public static Color BGColor = new Color(0.10f, 0.10f, 0.10f);
    public static Color BGColor2 = Color.Lerp(Color.Black, BGColor, 0.52f);
    public static Color ButtonColor = Color.Lerp(new Color(0.19f, 0.19f, 0.19f), BGColor, 0.54f);
    public static Color ButtonHoveredColor = Color.Lerp(new Color(0.5f, 0.5f, 0.5f), BGColor, 0.54f);
    public static Color ButtonPressedColor = new Color(0.20f, 0.22f, 0.23f);
    public static Color ScrollbarBGColor = Color.Lerp(new Color(0.5f, 0.5f, 0.5f), BGColor, 0.54f);
    public static Color ScrollbarColor = Color.Lerp(new Color(0.34f, 0.34f, 0.34f), BGColor, 0.54f);

    public static T WithFadedMouseOver<T>(this T elem, Color origColor = default, Color hoverdColor = default, Color pressedColor = default) where T : UIPanel
    {
        if (origColor == default)
            origColor = ButtonColor * 0.98f;

        if (hoverdColor == default)
            hoverdColor = ButtonHoveredColor * 0.98f;

        if (pressedColor == default)
            pressedColor = ButtonPressedColor * 0.98f;

        elem.OnMouseOver += (evt, _) =>
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            elem.BackgroundColor = hoverdColor;
        };
        elem.OnMouseOut += (evt, _) =>
        {
            elem.BackgroundColor = origColor;
        };
        elem.OnLeftMouseDown += (evt, _) =>
        {
            elem.BackgroundColor = pressedColor;
        };
        elem.OnLeftMouseUp += (evt, _) =>
        {
            elem.BackgroundColor = origColor;
        };
        return elem;
    }

    public static T WithPadding<T>(this T elem, float pixels) where T : UIElement
    {
        elem.SetPadding(pixels);
        return elem;
    }

    public static T WithPadding<T>(this T elem, string name, int id, Vector2? anchor = null, Vector2? offset = null) where T : UIElement
    {
        elem.SetSnapPoint(name, id, anchor, offset);
        return elem;
    }

    public static T WithView<T>(this T elem, float viewSize, float maxViewSize) where T : UIScrollbar
    {
        elem.SetView(viewSize, maxViewSize);
        return elem;
    }
}
