using ReLogic.OS.Base;

namespace ReLogic.OS.FNA;

internal class FNAClipboard : Clipboard
{
    protected override string GetClipboard()
    {
        return SDL2.SDL.SDL_GetClipboardText();
    }

    protected override void SetClipboard(string text)
    {
        SDL2.SDL.SDL_SetClipboardText(text);
    }
}
