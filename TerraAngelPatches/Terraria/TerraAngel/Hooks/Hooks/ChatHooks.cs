using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using Terraria;
using TerraAngel.Loader;
using ImGuiNET;
using ReLogic.OS;
using Terraria.DataStructures;
using Terraria.ID;
using TerraAngel.Cheat;
using TerraAngel.Hooks;

namespace TerraAngel.Hooks.Hooks
{
    public class ChatHooks
    {
        public static void Generate()
        {
            HookUtil.HookGen<Main>("DoUpdate_Enter_ToggleChat", OpenChatHook);
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
