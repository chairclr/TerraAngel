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
    }
}
