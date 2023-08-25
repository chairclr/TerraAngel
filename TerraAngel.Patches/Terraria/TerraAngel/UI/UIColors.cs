using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;

namespace TerraAngel.UI;

internal static class UIColors
{
    public static Color BackgroundColor => new Color(0.1f, 0.1f, 0.1f);

    public static Color ButtonColor => Color.Lerp(new Color(0.19f, 0.19f, 0.19f), BackgroundColor, 0.54f);

    public static Color ButtonHoveredColor => Color.Lerp(new Color(0.5f, 0.5f, 0.5f), ButtonColor, 0.54f);

    public static Color ButtonPressedColor = new Color(0.20f, 0.22f, 0.23f);

    public static T WithMouseEffects<T>(this T elem, Color? hoveredColor = null, Color? pressedColor = null) where T : UIPanel
    {
        Color originalColor = elem.BackgroundColor;

        hoveredColor ??= ButtonHoveredColor * 0.98f;

        pressedColor ??= ButtonPressedColor * 0.98f;

        elem.OnMouseOver += (_, _) =>
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            elem.BackgroundColor = hoveredColor.Value;
        };

        elem.OnMouseOut += (_, _) =>
        {
            elem.BackgroundColor = originalColor;
        };

        elem.OnLeftMouseDown += (_, _) =>
        {
            elem.BackgroundColor = pressedColor.Value;
        };

        elem.OnLeftMouseUp += (_, _) =>
        {
            if (elem.IsMouseHovering)
            {
                elem.BackgroundColor = hoveredColor.Value;
            }
            else
            {
                elem.BackgroundColor = originalColor;
            }
        };

        return elem;
    }
}
