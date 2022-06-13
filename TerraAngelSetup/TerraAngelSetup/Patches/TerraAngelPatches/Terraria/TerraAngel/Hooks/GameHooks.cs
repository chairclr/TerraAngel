using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using Terraria;
using TerraAngel.Loader;
using ImGuiNET;

namespace TerraAngel.Hooks
{
    public class GameHooks
    {
        public static void Generate()
        {
            HookUtil.HookGen<Main>("Initialize_AlmostEverything", InitHook);
            HookUtil.HookGen<Main>("DoDraw", DoDrawHook);
            HookUtil.HookGen(Main.DrawCursor, DrawCursorHook);
            HookUtil.HookGen(Main.DrawThickCursor, DrawThickCursorHook);
            HookUtil.HookGen<Main>("DoUpdate_Enter_ToggleChat", OpenChatHook);
        }

        public static void InitHook(Action<Main> orig, Main self)
        {
            orig(self);
            ClientLoader.SetupImGuiRenderer(self);
        }
        public static void DoDrawHook(Action<Main, GameTime> orig, Main self, GameTime time)
        {
            if (ClientLoader.SetupRenderer)
            {
                Main.blockInput = ImGui.GetIO().WantCaptureKeyboard;
            }
            orig(self, time);
            if (ClientLoader.SetupRenderer)
            {
                ClientLoader.MainRenderer.Render(time);
            }
        }
        public static void DrawCursorHook(Action<Vector2, bool> orig, Vector2 bonus, bool smart)
        {
            if (ClientLoader.SetupRenderer && ImGui.GetIO().WantCaptureMouse)
            {
                return;
            }
            orig(bonus, smart);
        }
        public static Vector2 DrawThickCursorHook(Func<bool, Vector2> orig, bool smart)
        {
            if (ClientLoader.SetupRenderer && ImGui.GetIO().WantCaptureMouse)
            {
                return Vector2.Zero;
            }
            return orig(smart);
        }
        public static void OpenChatHook(Action orig)
        {
            if (ClientLoader.SetupRenderer && ImGui.GetIO().WantCaptureMouse)
            {
                return;
            }
            orig();
        }
    }
}
