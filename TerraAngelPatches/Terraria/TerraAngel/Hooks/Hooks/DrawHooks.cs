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
using TerraAngel.Utility;
using TerraAngel.Client.Config;
using TerraAngel;
using TerraAngel.Cheat.Cringes;

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
            HookUtil.HookGen(Main.MouseText_DrawItemTooltip_GetLinesInfo, GetLinesInfoHook);
        }

        public delegate void GetLinesInfoDef(Item item, ref int yoyoLogo, ref int researchLine, float oldKB, ref int numLines, string[] toolTipLine, bool[] preFixLine, bool[] badPreFixLine);
        public static void GetLinesInfoHook(GetLinesInfoDef orig, Item item, ref int yoyoLogo, ref int researchLine, float oldKB, ref int numLines, string[] toolTipLine, bool[] preFixLine, bool[] badPreFixLine)
        {
            orig(item, ref yoyoLogo, ref researchLine, oldKB, ref numLines, toolTipLine, preFixLine, badPreFixLine);
            /// Keep this as is, never change this. 
            /// "please" - An anonymous user
            if (ClientLoader.Config.ShowIDsInTooltips)
            {
                toolTipLine[0] += $" [ItemID.{Util.itemIds[item.type].Name}/{item.type}]";
                if (item.prefix > 0)
                {
                    int firstSpace = toolTipLine[0].IndexOf(' ');
                    firstSpace = firstSpace == - 1 ? toolTipLine[0].Length : firstSpace;
                    toolTipLine[0] = toolTipLine[0].Insert(firstSpace, $" [PrefixID.{Util.prefixIds[item.prefix - 1].Name}/{item.prefix}]");
                }
            }
        }
        public static void DoDrawHook(Action<Main, GameTime> orig, Main self, GameTime time)
        {
            orig(self, time);
            ClientLoader.MainRenderer?.Render(time);
        }
        public static void DrawCursorHook(Action<Vector2, bool> orig, Vector2 bonus, bool smart)
        {
            if (ClientLoader.WantCaptureMouse || ClientLoader.WantCaptureKeyboard)
            {
                return;
            }
            orig(bonus, smart);
        }
        public static Vector2 DrawThickCursorHook(Func<bool, Vector2> orig, bool smart)
        {
            if (ClientLoader.WantCaptureMouse || ClientLoader.WantCaptureKeyboard)
            {
                return Vector2.Zero;
            }
            return orig(smart);
        }
        private static Vector2 freecamOriginPoint;
        public static void UpdateCameraHook(Action orig)
        {
            FullBrightCringe fullBright = CringeManager.GetCringe<FullBrightCringe>();
            if (Input.InputSystem.IsKeyPressed(ClientLoader.Config.ToggleFullbright))
            {
                fullBright.Enabled = !fullBright.Enabled;
            }

            FreecamCringe freecam = CringeManager.GetCringe<FreecamCringe>();
            if (Input.InputSystem.IsKeyPressed(ClientLoader.Config.ToggleFreecam))
            {
                freecam.Enabled = !freecam.Enabled;
            }
            if (freecam.Enabled)
            {
                ImGuiIOPtr io = ImGui.GetIO();
                if (io.MouseClicked[1])
                {
                    freecamOriginPoint = Util.ScreenToWorld(Input.InputSystem.MousePosition);
                }
                if (io.MouseDown[1])
                {
                    Vector2 diff = freecamOriginPoint - Util.ScreenToWorld(Input.InputSystem.MousePosition);
                    Main.screenPosition = Main.screenPosition + diff;
                }
                return;
            }
            orig();
        }

        private static Vector3 LightingHook(Func<Terraria.Graphics.Light.LightingEngine, int, int, Vector3> orig, Terraria.Graphics.Light.LightingEngine self, int x, int y)
        {
            FullBrightCringe fullBright = CringeManager.GetCringe<FullBrightCringe>();
            if (fullBright.Enabled)
            {
                return Vector3.One * fullBright.Brightness;
            }
            return orig(self, x, y);
        }
        private static Vector3 LegacyLightingHook(Func<Terraria.Graphics.Light.LegacyLighting, int, int, Vector3> orig, Terraria.Graphics.Light.LegacyLighting self, int x, int y)
        {
            FullBrightCringe fullBright = CringeManager.GetCringe<FullBrightCringe>();
            if (fullBright.Enabled)
            {
                return Vector3.One * fullBright.Brightness;
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
            if (CringeManager.GetCringe<NoDustCringe>().Enabled)
                return 6000;
            return orig( Position,  Width,  Height,  Type,  SpeedX,  SpeedY,  Alpha,  newColor,  Scale);
        }
        public static void UpdateDustHook(Action orig)
        {
            if (CringeManager.GetCringe<NoDustCringe>().Enabled)
                    return;
            orig();
        }
        public static void DrawDustHook(Action<Main> orig, Main self)
        {
            if (CringeManager.GetCringe<NoDustCringe>().Enabled)
                    return;
            orig(self);
        }

        public static int NewGoreHook(Func<Vector2, Vector2, int, float, int> orig, Vector2 Position, Vector2 Velocity, int Type, float Scale)
        {
            if (CringeManager.GetCringe<NoDustCringe>().Enabled)
                    return 600;
            return orig(Position, Velocity, Type, Scale);
        }
        public static void DrawGoreHook(Action<Main> orig, Main self)
        {
            if (CringeManager.GetCringe<NoGoreCringe>().Enabled)
                    return;
            orig(self);
        }
        public static void DrawGoreBehindHook(Action<Main> orig, Main self)
        {
            if (CringeManager.GetCringe<NoGoreCringe>().Enabled)
                    return;
            orig(self);
        }
    }
}
