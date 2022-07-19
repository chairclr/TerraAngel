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
using TerraAngel.Utility;
using TerraAngel.Client.Config;

namespace TerraAngel.Hooks.Hooks
{
    public class DrawHooks
    {
        public static void Generate()
        {
            HookUtil.HookGen<Main>("DoDraw", DoDrawHook);
            HookUtil.HookGen(Main.DrawCursor, DrawCursorHook);
            HookUtil.HookGen(Main.DrawThickCursor, DrawThickCursorHook);
            HookUtil.HookGen<Main>("DoDraw_UpdateCameraPosition", UpdateCameraHook);

            HookUtil.HookGen<Terraria.Graphics.Light.LightingEngine>("GetColor", LightingHook);
            HookUtil.HookGen<Terraria.Graphics.Light.LegacyLighting>("GetColor", LegacyLightingHook);

            //HookUtil.HookGen<Terraria.Graphics.Light.LightingEngine>("ProcessArea", LightingProcessAreaHook);
            //HookUtil.HookGen<Terraria.Graphics.Light.LegacyLighting>("ProcessArea", LegacyLightingProcessAreaHook);

            HookUtil.HookGen(Dust.NewDust, NewDustHook);
            HookUtil.HookGen(Dust.UpdateDust, UpdateDustHook);
            HookUtil.HookGen<Main>("DrawDust", DrawDustHook);

            HookUtil.HookGen(Gore.NewGore, NewGoreHook);
            HookUtil.HookGen<Main>("DrawGore", DrawGoreHook);
            HookUtil.HookGen<Main>("DrawGoreBehind", DrawGoreBehindHook);
        }

        public static void DoDrawHook(Action<Main, GameTime> orig, Main self, GameTime time)
        {
            if (ClientLoader.SetupRenderer)
            {
                Main.blockInput = ImGui.GetIO().WantCaptureKeyboard;
            
                //if (Main.drawingPlayerChat && !Main.blockInput)
                //{
                //    if (ImGui.GetIO().KeyCtrl && Input.InputSystem.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.V))
                //    {
                //        Main.chatText += SDL2.SDL.SDL_GetClipboardText();
                //    }
                //}
            }
            orig(self, time);
            if (ClientLoader.SetupRenderer)
            {
                Main.blockInput = ImGui.GetIO().WantCaptureKeyboard;
            }
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
        private static Vector2 freecamOriginPoint;
        public static void UpdateCameraHook(Action orig)
        {
            if (Input.InputSystem.IsKeyPressed(ClientConfig.Instance.ToggleFreecam))
            {
                GlobalCheatManager.Freecam = !GlobalCheatManager.Freecam;
            }
            if (Input.InputSystem.IsKeyPressed(ClientConfig.Instance.ToggleFullbright))
            {
                GlobalCheatManager.FullBright = !GlobalCheatManager.FullBright;
            }
            if (GlobalCheatManager.Freecam)
            {
                ImGuiIOPtr io = ImGui.GetIO();
                if (io.MouseClicked[1])
                {
                    freecamOriginPoint = Util.ScreenToWorld(Main.MouseScreen);
                }
                if (io.MouseDown[1])
                {
                    Vector2 diff = freecamOriginPoint - Util.ScreenToWorld(Main.MouseScreen);
                    Main.screenPosition = Main.screenPosition + diff;
                }
                return;
            }
            orig();
        }

        private static Vector3 LightingHook(Func<Terraria.Graphics.Light.LightingEngine, int, int, Vector3> orig, Terraria.Graphics.Light.LightingEngine self, int x, int y)
        {
            if (GlobalCheatManager.FullBright)
            {
                return Vector3.One * GlobalCheatManager.FullBrightBrightness;
            }
            return orig(self, x, y);
        }
        private static Vector3 LegacyLightingHook(Func<Terraria.Graphics.Light.LegacyLighting, int, int, Vector3> orig, Terraria.Graphics.Light.LegacyLighting self, int x, int y)
        {
            if (GlobalCheatManager.FullBright)
            {
                return Vector3.One * GlobalCheatManager.FullBrightBrightness;
            }
            return orig(self, x, y);
        }
        //private static void LightingProcessAreaHook(Action<Terraria.Graphics.Light.LightingEngine, Rectangle> orig, Terraria.Graphics.Light.LightingEngine self, Rectangle area)
        //{
        //    if (GlobalCheatManager.FullBright)
        //        return;
        //    orig(self, area);
        //}
        //private static void LegacyLightingProcessAreaHook(Action<Terraria.Graphics.Light.LegacyLighting, Rectangle> orig, Terraria.Graphics.Light.LegacyLighting self, Rectangle area)
        //{
        //    if (GlobalCheatManager.FullBright)
        //        return;
        //    orig(self, area);
        //}

        public static int NewDustHook(Func<Vector2, int, int, int, float, float, int, Color, float, int> orig, Vector2 Position, int Width, int Height, int Type, float SpeedX, float SpeedY, int Alpha, Color newColor, float Scale)
        {
            if (GlobalCheatManager.NoDust)
                return 6000;
            return orig( Position,  Width,  Height,  Type,  SpeedX,  SpeedY,  Alpha,  newColor,  Scale);
        }
        public static void UpdateDustHook(Action orig)
        {
            if (GlobalCheatManager.NoDust)
                return;
            orig();
        }
        public static void DrawDustHook(Action<Main> orig, Main self)
        {
            if (GlobalCheatManager.NoDust)
                return;
            orig(self);
        }

        public static int NewGoreHook(Func<Vector2, Vector2, int, float, int> orig, Vector2 Position, Vector2 Velocity, int Type, float Scale)
        {
            if (GlobalCheatManager.NoGore)
                return 600;
            return orig(Position, Velocity, Type, Scale);
        }
        public static void DrawGoreHook(Action<Main> orig, Main self)
        {
            if (GlobalCheatManager.NoGore)
                return;
            orig(self);
        }
        public static void DrawGoreBehindHook(Action<Main> orig, Main self)
        {
            if (GlobalCheatManager.NoGore)
                return;
            orig(self);
        }
    }
}
