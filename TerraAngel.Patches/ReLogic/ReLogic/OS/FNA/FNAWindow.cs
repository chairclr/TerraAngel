using Microsoft.Xna.Framework;

namespace ReLogic.OS.FNA;

internal class FNAWindow : IWindowService
{
    public float GetScaling()
    {
        return 1f;
    }

    public void SetQuickEditEnabled(bool enabled)
    {

    }

    public void SetUnicodeTitle(GameWindow window, string title)
    {
        SDL2.SDL.SDL_SetWindowTitle(window.Handle, title);
    }

    public void StartFlashingIcon(GameWindow window)
    {
        SDL2.SDL.SDL_FlashWindow(window.Handle, SDL2.SDL.SDL_FlashOperation.SDL_FLASH_BRIEFLY);
    }

    public void StopFlashingIcon(GameWindow window)
    {
        SDL2.SDL.SDL_FlashWindow(window.Handle, SDL2.SDL.SDL_FlashOperation.SDL_FLASH_CANCEL);
    }
}
