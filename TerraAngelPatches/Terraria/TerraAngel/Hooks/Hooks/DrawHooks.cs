using System;
using System.Reflection;
using ReLogic.Threading;
using Terraria.Graphics.Light;

namespace TerraAngel.Hooks.Hooks
{
    public class DrawHooks
    {
        public static void Generate()
        {
            HookUtil.HookGen<Main>("DoDraw", DoDrawHook);
            HookUtil.HookGen<Main>("Update", UpdateHook);
            HookUtil.HookGen(Main.DrawCursor, DrawCursorHook);
            HookUtil.HookGen(Main.DrawThickCursor, DrawThickCursorHook);
            HookUtil.HookGen<Main>("DoDraw_UpdateCameraPosition", UpdateCameraHook);

            HookUtil.HookGen<LightingEngine>("GetColor", LightingHook);
            HookUtil.HookGen<LegacyLighting>("GetColor", LegacyLightingHook);

            HookUtil.HookGen<LightingEngine>("ProcessArea", LightingProcessAreaHook);
            HookUtil.HookGen<LegacyLighting>("ProcessArea", LegacyLightingProcessAreaHook);

            HookUtil.HookGen<LightingEngine>("AddLight", LightingAddLightHook);
            HookUtil.HookGen<LegacyLighting>("AddLight", LegacyLightingAddLightHook);

            HookUtil.HookGen<LightingEngine>("UpdateLightDecay", LightingUpdateLightDecay);

            HookUtil.HookGen(Dust.NewDust, NewDustHook);
            HookUtil.HookGen(Dust.UpdateDust, UpdateDustHook);
            HookUtil.HookGen<Main>("DrawDust", DrawDustHook);

            HookUtil.HookGen(Gore.NewGore, NewGoreHook);
            HookUtil.HookGen<Main>("DrawGore", DrawGoreHook);
            HookUtil.HookGen<Main>("DrawGoreBehind", DrawGoreBehindHook);
            HookUtil.HookGen(Main.MouseText_DrawItemTooltip_GetLinesInfo, GetLinesInfoHook);
            HookUtil.HookGen<Main>("DrawTiles", DrawTilesHook);
            HookUtil.HookGen<Main>("DrawWater", DrawWaterHook);
        }

        public static void DrawWaterHook(Action<Main, bool, int, float> orig, Main self, bool bg, int Style, float Alpha)
        {
            orig(self, bg, Style, Alpha);
        }

        public delegate void GetLinesInfoDef(Item item, ref int yoyoLogo, ref int researchLine, float oldKB, ref int numLines, string[] toolTipLine, bool[] preFixLine, bool[] badPreFixLine);
        public static void GetLinesInfoHook(GetLinesInfoDef orig, Item item, ref int yoyoLogo, ref int researchLine, float oldKB, ref int numLines, string[] toolTipLine, bool[] preFixLine, bool[] badPreFixLine)
        {
            orig(item, ref yoyoLogo, ref researchLine, oldKB, ref numLines, toolTipLine, preFixLine, badPreFixLine);
            /// Keep this as is, never change this. 
            /// "please" - An anonymous user
            if (ClientConfig.Settings.ShowDetailedItemTooltip)
            {
                toolTipLine[0] += $" [{Util.ItemFields[item.type].Name}/{item.type}]";
                if (item.prefix > 0)
                {
                    int firstSpace = toolTipLine[0].IndexOf(' ');
                    firstSpace = firstSpace == -1 ? toolTipLine[0].Length : firstSpace;
                    toolTipLine[0] = toolTipLine[0].Insert(firstSpace, $" [{Util.PrefixFields[item.prefix].Name}/{item.prefix}]");
                }

                if (item.useAmmo > 0)
                {
                    int projectileType = item.shoot;
                    float shootSpeed = item.shootSpeed;
                    bool cs = true;
                    int dm = 0;
                    float kb = 0.0f;
                    Main.LocalPlayer.PickAmmo(item, ref projectileType, ref shootSpeed, ref cs, ref dm, ref kb, out _, true);

                    if (Util.ProjectileFields.ContainsKey(projectileType))
                    {
                        toolTipLine[numLines] = $"[a:[Projectile: {Util.ProjectileFields[projectileType].Name}/{projectileType}]]";
                        numLines++;
                    }
                    else
                    {
                        if (item.shoot > 0 && item.shoot < ProjectileID.Count && Util.ProjectileFields.ContainsKey(item.shoot))
                        {
                            toolTipLine[numLines] = $"[a:[Projectile: {Util.ProjectileFields[item.shoot].Name}/{item.shoot}]]";
                            numLines++;
                        }
                    }
                }
                else
                {
                    if (item.shoot > 0 && item.shoot < ProjectileID.Count && Util.ProjectileFields.ContainsKey(item.shoot) && !Util.AmmoFields.ContainsKey(item.ammo))
                    {
                        toolTipLine[numLines] = $"[a:[Projectile: {Util.ProjectileFields[item.shoot].Name}/{item.shoot}]]";
                        numLines++;
                    }
                }

                if (item.createTile > -1 && item.createTile < TileID.Count && Util.TileFields.ContainsKey(item.createTile))
                {
                    toolTipLine[numLines] = $"[a:[Tile: {Util.TileFields[item.createTile].Name}/{item.createTile}{(item.placeStyle > 0 ? $" {item.placeStyle}" : "")}]]";
                    numLines++;
                }

                if (item.createWall > 0 && item.createWall < WallID.Count && Util.WallFields.ContainsKey(item.createWall))
                {
                    toolTipLine[numLines] = $"[a:[Wall: {Util.WallFields[item.createWall].Name}/{item.createWall}]]";
                    numLines++;
                }

                if (item.ammo > 0 && item.ammo < AmmoID.NailFriendly + 1 && Util.AmmoFields.ContainsKey(item.ammo))
                {
                    toolTipLine[numLines] = $"[a:[Ammo: {Util.AmmoFields[item.ammo].Name}/{item.ammo}]]";
                    numLines++;
                }

                if (item.shootSpeed > 0f)
                {
                    toolTipLine[numLines] = $"[a:[Shoot Speed: {item.shootSpeed:F1}]]";
                    numLines++;
                }

                if (item.buffType > 0 && item.buffType < BuffID.Count)
                {
                    if (item.buffTime > 0)
                    {
                        toolTipLine[numLines] = $"[a:[Buff: {Util.BuffFields[item.buffType].Name}/{item.buffType} for {TimeSpan.FromSeconds(item.buffTime / 60f):mm\\:ss}]]";
                    }
                    else
                    {
                        toolTipLine[numLines] = $"[a:[Buff: {Util.BuffFields[item.buffType].Name}/{item.buffType}]]";
                    }
                    numLines++;
                }
            }
        }

        public static void DrawTilesHook(Action<Main, bool, bool, bool, int> orig, Main self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride)
        {
            //float lsp = Main.screenPosition.Y;
            //Main.screenPosition.Y = fixedScreenY;
            orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);
            //Main.screenPosition.Y = lsp;
        }

        public static void DoDrawHook(Action<Main, GameTime> orig, Main self, GameTime time)
        {
            lightModificationCache = CringeManager.GetCringe<LightingModifierCringe>();
            orig(self, time);
            float ry = Main.screenPosition.Y;
            Main.screenPosition.Y = Main.realScreenY;
            ClientLoader.MainRenderer?.Render(time);
            Main.screenPosition.Y = ry;
        }
        public static void UpdateHook(Action<Main, GameTime> orig, Main self, GameTime time)
        {
            orig(self, time);
            ClientLoader.MainRenderer?.Update(time);
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
        private static int fixedScreenY;
        public static int SpectateOverride = -1;
        public static void UpdateCameraHook(Action orig)
        {
            if (!Main.gameMenu)
            {
                FreecamCringe freecam = CringeManager.GetCringe<FreecamCringe>();
                if (freecam.Enabled)
                {
                    ImGuiIOPtr io = ImGui.GetIO();
                    if (io.MouseClicked[1])
                    {
                        freecamOriginPoint = Util.ScreenToWorld(InputSystem.MousePosition);
                    }
                    if (io.MouseDown[1])
                    {
                        Vector2 diff = freecamOriginPoint - Util.ScreenToWorld(InputSystem.MousePosition);
                        Main.screenPosition = Main.screenPosition + diff;
                    }
                    Main.realScreenY = Main.screenPosition.Y;
                    return;
                }
            }

            if (Main.gameMenu || Main.LocalPlayer.controlUp ||
                Main.LocalPlayer.controlLeft ||
                Main.LocalPlayer.controlDown ||
                Main.LocalPlayer.controlRight ||
                Main.LocalPlayer.controlJump ||
                Main.LocalPlayer.controlUseTile ||
                Main.LocalPlayer.controlThrow ||
                Main.LocalPlayer.controlHook ||
                Main.LocalPlayer.controlMount)
                SpectateOverride = -1;

            int temp = Main.myPlayer;
            if (!Main.gameMenu)
            {
                if (SpectateOverride > -1)
                    Main.myPlayer = SpectateOverride;
            }
            orig();
            if (!Main.gameMenu)
            {
                Main.myPlayer = temp;
            }
        }

        private static LightingModifierCringe? lightModificationCache;
        private static Vector3 LightingHook(Func<LightingEngine, int, int, Vector3> orig, LightingEngine self, int x, int y)
        {
            if (lightModificationCache?.FullBright ?? false)
            {
                return Vector3.One * lightModificationCache.Brightness;
            }
            return orig(self, x, y);
        }
        private static Vector3 LegacyLightingHook(Func<LegacyLighting, int, int, Vector3> orig, LegacyLighting self, int x, int y)
        {
            if (lightModificationCache?.FullBright ?? false)
            {
                return Vector3.One * lightModificationCache.Brightness;
            }
            return orig(self, x, y);
        }
        private static void LightingProcessAreaHook(Action<LightingEngine, Rectangle> orig, LightingEngine self, Rectangle area)
        {
            if (lightModificationCache?.FullBright ?? false)
            {
                Main.renderCount = (Main.renderCount + 1) % 4;
                Rectangle value = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);
                value.Inflate(-40, -40);
                area = Rectangle.Intersect(area, value);
                Main.mapMinX = area.Left;
                Main.mapMinY = area.Top;
                Main.mapMaxX = area.Right;
                Main.mapMaxY = area.Bottom;

                /// this hurts to use :(
                FastParallel.For(area.Left, area.Right, delegate (int start, int end, object context)
                {
                    for (int i = start; i < end; i++)
                    {
                        for (int j = area.Top; j < area.Bottom; j++)
                        {
                            Main.Map.Update(i, j, 255);
                        }
                    }
                });

                return;
            }
            orig(self, area);
        }
        private static void LegacyLightingProcessAreaHook(Action<LegacyLighting, Rectangle> orig, LegacyLighting self, Rectangle area)
        {
            if (lightModificationCache?.FullBright ?? false)
            {
                Main.renderCount = (Main.renderCount + 1) % 4;
                Rectangle value = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);
                value.Inflate(-40, -40);
                area = Rectangle.Intersect(area, value);
                Main.mapMinX = area.Left;
                Main.mapMinY = area.Top;
                Main.mapMaxX = area.Right;
                Main.mapMaxY = area.Bottom;

                FastParallel.For(area.Left, area.Right, delegate (int start, int end, object context)
                {
                    for (int i = start; i < end; i++)
                    {
                        for (int j = area.Top; j < area.Bottom; j++)
                        {
                            Main.Map.Update(i, j, 255);
                        }
                    }
                });

                Main.updateMap = true;

                return;
            }
            orig(self, area);
        }
        private static void LightingAddLightHook(Action<LightingEngine, int, int, Vector3> orig, LightingEngine self, int x, int y, Vector3 color)
        {
            if (lightModificationCache?.FullBright ?? false)
                return;
            orig(self, x, y, color);
        }
        private static void LegacyLightingAddLightHook(Action<LegacyLighting, int, int, Vector3> orig, LegacyLighting self, int x, int y, Vector3 color)
        {
            if (lightModificationCache?.FullBright ?? false)
                return;
            orig(self, x, y, color);
        }

        private static void LightingUpdateLightDecay(Action<LightingEngine> orig, LightingEngine self)
        {
            orig(self);

            if (lightModificationCache?.PartialBright ?? false)
            {
                LightMap? workingLightMap = (LightMap?)typeof(LightingEngine).GetField("_workingLightMap", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(self);
                if (workingLightMap == null)
                    return;
                workingLightMap.LightDecayThroughAir *= lightModificationCache.ExtraAirBrightness + 1f;
                workingLightMap.LightDecayThroughSolid *= lightModificationCache.ExtraSolidBrightness + 1f;
            }
        }

        private static OptimizationCringe? optimizationCache;
        public static int NewDustHook(Func<Vector2, int, int, int, float, float, int, Color, float, int> orig, Vector2 Position, int Width, int Height, int Type, float SpeedX, float SpeedY, int Alpha, Color newColor, float Scale)
        {
            if (optimizationCache?.DisableDust ?? false)
                return 6000;
            return orig(Position, Width, Height, Type, SpeedX, SpeedY, Alpha, newColor, Scale);
        }
        public static void UpdateDustHook(Action orig)
        {
            optimizationCache = CringeManager.GetCringe<OptimizationCringe>();
            if (optimizationCache?.DisableDust ?? false)
                return;
            orig();
        }
        public static void DrawDustHook(Action<Main> orig, Main self)
        {
            if (optimizationCache?.DisableDust ?? false)
                return;
            orig(self);
        }

        public static int NewGoreHook(Func<Vector2, Vector2, int, float, int> orig, Vector2 Position, Vector2 Velocity, int Type, float Scale)
        {
            if (optimizationCache?.DisableGore ?? false)
                return 600;
            return orig(Position, Velocity, Type, Scale);
        }
        public static void DrawGoreHook(Action<Main> orig, Main self)
        {
            if (optimizationCache?.DisableGore ?? false)
                return;
            orig(self);
        }
        public static void DrawGoreBehindHook(Action<Main> orig, Main self)
        {
            if (optimizationCache?.DisableGore ?? false)
                return;
            orig(self);
        }
    }
}
