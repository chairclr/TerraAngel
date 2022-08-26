using System;
using TerraAngel.Hooks.Hooks;

namespace TerraAngel.Hooks
{
    public class GameHooks
    {
        public static void Generate()
        {
            HookUtil.HookGen<Main>("Initialize_AlmostEverything", InitHook);
            DrawHooks.Generate();
            ChatHooks.Generate();
            PlayerHooks.Generate();
            MiscHooks.Generate();
        }

        public static void InitHook(Action<Main> orig, Main self)
        {
            orig(self);
            ClientLoader.SetupImGuiRenderer(self);
        }
    }
}
