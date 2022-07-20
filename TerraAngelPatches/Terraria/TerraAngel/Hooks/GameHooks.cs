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
using TerraAngel.Hooks.Hooks;
using TerraAngel;

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
