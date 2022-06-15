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
    public class MiscHooks
    {
        public static void Generate()
        {
            HookUtil.HookGen<Main>("SetTitle", SetTitleHook);
            //HookUtil.HookGen(typeof(SDL2.SDL).Assembly.GetType("Microsoft.Xna.Framework.SDL2_FNAPlatform"), "OnIsMouseVisibleChanged", OnIsMouseVisibleChangedHook);
            HookUtil.HookGen<Game>("set_IsMouseVisible", set_IsMouseVisibleHook);
            //HookUtil.HookGen(Collision.EmptyTile, EmptyTileHook);
            //HookUtil.HookGen(Collision.TileCollision, TileCollisionHook);
            //HookUtil.HookGen<Terraria.UI.Gamepad.UILinkPointNavigator>("Update", UILinkPointNavigatorUpdateHook);
        }
        public static void set_IsMouseVisibleHook(Action<Game, bool> orig, Game self, bool visible)
        {
            if (ClientLoader.SetupRenderer && ImGui.GetIO().WantCaptureMouse)
                visible = true;
            orig(self, visible);
        }
        public static void SetTitleHook(Action<Main> orig, Main self)
        {
            
            SDL2.SDL.SDL_SetWindowTitle(self.Window.Handle, "TerraAngel");
            //orig(self);
        }
        // Justification for patching out: its literally useless who tf uses up and down to navigate the menu headass
        public static void UILinkPointNavigatorUpdateHook(Action orig)
        {
            //orig();
        }
        //public static Vector2 TileCollisionHook(Func<Vector2, Vector2, int, int, bool, bool, int, Vector2> orig, Vector2 Position, Vector2 Velocity, int Width, int Height, bool fallThrough = false, bool fall2 = false, int gravDir = 1)
        //{
        //    return orig(Position, Velocity, Width, Height, fallThrough, fall2, gravDir);
        //}
        //public static bool EmptyTileHook(Func<int, int, bool, bool> orig, int i, int j, bool ignoreTiles)
        //{
        //    if (GlobalCheatManager.NoClip)
        //        return true;
        //    return orig(i, j, ignoreTiles);
        //}
        //public static void OnIsMouseVisibleChangedHook(Action<bool> orig, bool visible)
        //{
        //    if (ClientLoader.SetupRenderer && ImGui.GetIO().WantCaptureMouse)
        //    {
        //        Main.instance.IsMouseVisible
        //        visible = true;
        //    }
        //    orig(visible);
        //}
    }
}
