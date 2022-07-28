using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using Terraria;
using ImGuiNET;
using ReLogic.OS;
using Terraria.DataStructures;
using Terraria.ID;
using TerraAngel.Cheat;
using TerraAngel.Hooks;
using TerraAngel;
using Terraria.UI.Chat;
using ReLogic.Graphics;

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
            orig();
        }
        public static void OpenChatHook(Action orig)
        {
            return;
            orig();
        }
        public static void CloseChatHook(Action orig)
        {
            return;
            orig();
        }
        public static void DoUpdateChatHook(Action orig)
        {
            return;
            orig();
        }
        public static void DrawPlayerChatHook(Action<Main> orig, Main self)
        {
            return;
            orig(self);
        }
        public static void NewTextHook(Action<string, byte, byte, byte> orig, string newText, byte R, byte G, byte B)
        {
            ClientLoader.Chat.WriteLine(newText, new Color(R, G, B, 255));
            return;
            orig(newText, R, G, B);
        }
        public static void NewTextMultilineHook(Action<string, bool, Color, int> orig, string text, bool force, Color color, int WidthLimit)
        {
            ClientLoader.Chat.WriteLine(text, color);
            return;
            orig(text, force, color, WidthLimit);
        }
        public static bool AddChatTextHook(Func<DynamicSpriteFont, string, Vector2, bool> orig, DynamicSpriteFont font, string text, Vector2 baseScale)
        {
            ClientLoader.Chat.AddText(text);
            return orig(font, text, baseScale);
        }
    }
}
