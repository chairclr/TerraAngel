using System;
using ReLogic.Graphics;
using Terraria.UI.Chat;

namespace TerraAngel.Hooks.Hooks
{
    public class ChatHooks
    {
        public static void Generate()
        {
            HookUtil.HookGen<Main>("DoUpdate_Enter_ToggleChat", ToggleChatHook);
            HookUtil.HookGen<Main>("OpenPlayerChat", OpenChatHook);
            HookUtil.HookGen<Main>("ClosePlayerChat", CloseChatHook);
            HookUtil.HookGen<Main>("DrawPlayerChat", DrawPlayerChatHook);
            HookUtil.HookGen<Main>("DoUpdate_HandleChat", DoUpdateChatHook);
            HookUtil.HookGen<Main>("NewText", NewTextHook);
            HookUtil.HookGen<Main>("NewTextMultiline", NewTextMultilineHook);
            HookUtil.HookGen(ChatManager.AddChatText, AddChatTextHook);
        }
        public static void ToggleChatHook(Action orig)
        {
            return;
        }
        public static void OpenChatHook(Action orig)
        {
            return;
        }
        public static void CloseChatHook(Action orig)
        {
            return;
        }
        public static void DoUpdateChatHook(Action orig)
        {
            return;
        }
        public static void DrawPlayerChatHook(Action<Main> orig, Main self)
        {
            return;
        }
        public static void NewTextHook(Action<string, byte, byte, byte> orig, string newText, byte R, byte G, byte B)
        {
            ClientLoader.Chat.WriteLine(newText, new Color(R, G, B, 255));
            return;
        }
        public static void NewTextMultilineHook(Action<string, bool, Color, int> orig, string text, bool force, Color color, int WidthLimit)
        {
            ClientLoader.Chat.WriteLine(text, color);
            return;
        }
        public static bool AddChatTextHook(Func<DynamicSpriteFont, string, Vector2, bool> orig, DynamicSpriteFont font, string text, Vector2 baseScale)
        {
            ClientLoader.Chat.AddText(text);
            return true;
        }
    }
}
