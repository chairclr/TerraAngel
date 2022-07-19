using System;
using System.Threading;
using ReLogic.OS.Base;

namespace ReLogic.OS
{
    internal class FNAClipboard : ReLogic.OS.Base.Clipboard
    {
        protected override string GetClipboard()
        {
            return TryToGetClipboardText();
        }

        protected override void SetClipboard(string text)
        {
            SDL2.SDL.SDL_SetClipboardText(text);
        }

        private string TryToGetClipboardText()
        {
            return SDL2.SDL.SDL_GetClipboardText();
        }
    }
}
