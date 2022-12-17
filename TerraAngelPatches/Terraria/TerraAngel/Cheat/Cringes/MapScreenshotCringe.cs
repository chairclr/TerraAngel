using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CommunityToolkit.HighPerformance;
using ReLogic.OS;
using SDL2;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using Steamworks;
using Color = Microsoft.Xna.Framework.Color;

namespace TerraAngel.Cheat.Cringes;
public class MapScreenshotCringe : Cringe
{
    public override string Name => "Map Screenshot";

    public override CringeTabs Tab => CringeTabs.MiscCringes;

    public bool TakingScreenshot = false;
    public bool SelectingArea = false;

    public Vector2i ScreenshotOrigin = Vector2i.Zero;

    public ref int PixelsPerTile => ref ClientConfig.Settings.MapScreenshotPixelsPerTile;

    public override void DrawUI(ImGuiIOPtr io)
    {
        ImGui.SliderInt("Pixels Per Tile", ref PixelsPerTile, 1, 32);
    }

    public override void Update()
    {
        if (Main.mapFullscreen)
        {
            if (InputSystem.IsKeyPressed(ClientConfig.Settings.TakeMapScreenshot))
            {
                TakingScreenshot = true;
                SelectingArea = false;
            }

            if (TakingScreenshot)
            {
                if (InputSystem.LeftMousePressed)
                {
                    SelectingArea = true;

                    ScreenshotOrigin = (Util.ScreenToWorldFullscreenMap(InputSystem.MousePosition) / 16f);
                }

                if (SelectingArea)
                {
                    ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
                    Vector2 mousePos = (Util.ScreenToWorldFullscreenMap(InputSystem.MousePosition) / 16f);
                    Vector2i screenshotOrigin = ScreenshotOrigin;

                    drawList.DrawTileRect((Vector2i)screenshotOrigin, (Vector2i)mousePos, new Color(1f, 0f, 0f, 0.7f).PackedValue);

                    if (InputSystem.LeftMouseReleased)
                    {
                        if (OperatingSystem.IsWindows())
                        {
                            TakeMapScreenshot(screenshotOrigin, mousePos);

                            TakingScreenshot = false;
                            SelectingArea = false;
                        }
                    }
                }
            }
        }
    }

    [SupportedOSPlatform("Windows")]
    public unsafe void TakeMapScreenshot(Vector2i start, Vector2i end)
    {
        if (start.X == end.X || start.Y == end.Y) return;
        if (end.X < start.X)
        {
            int temp = start.X;
            start.X = end.X;
            end.X = temp;
        }
        if (end.Y < start.Y)
        {
            int temp = start.Y;
            start.Y = end.Y;
            end.Y = temp;
        }

        Vector2i size = end - start;

        int ppt = PixelsPerTile;

        Task.Run(
            () =>
            {
                try
                {
                    using System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(size.X * ppt, size.Y * ppt);
                    System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(new NRectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    
                    Span2D<Color> bitmapSpan = new Span2D<Color>((void*)bitmapData.Scan0, bitmapData.Height, bitmapData.Width, 0);

                    ClientLoader.Console.WriteLine("Taking map screenshot");
                    for (int y = 0; y < size.Y; y++)
                    {
                        for (int x = 0; x < size.X; x++)
                        {
                            int tileX = x + start.X;
                            int tileY = y + start.Y;
                            Terraria.Map.MapTile mapTile = Main.Map[tileX, tileY];
                            Color col = Terraria.Map.MapHelper.GetMapTileXnaColor(ref mapTile);
                            byte r = col.R;
                            col.R = col.B;
                            col.B = r;

                            for (int yp = 0; yp < ppt; yp++)
                            {
                                for (int xp = 0; xp < ppt; xp++)
                                {
                                    bitmapSpan[y * ppt + yp, x * ppt + xp].PackedValue = col.PackedValue;
                                }
                            }
                        }
                    }
                    bitmap.UnlockBits(bitmapData);

                    SetClipboardBitmap(bitmap);
                    ClientLoader.Console.WriteLine("Copied map screenshot to clipboard");

                }
                catch (Exception ex)
                {
                    ClientLoader.Console.WriteError($"{ex}");
                }
            });
    }

    [SupportedOSPlatform("Windows")]
    private unsafe void SetClipboardBitmap(System.Drawing.Bitmap bitmap)
    {
        SDL.SDL_SysWMinfo wmInfo = new SDL.SDL_SysWMinfo();
        SDL.SDL_VERSION(out wmInfo.version);
        SDL.SDL_GetWindowWMInfo(Main.instance.Window.Handle, ref wmInfo);
        nint hwnd = wmInfo.info.win.window;

        if (!NativeClipboard.OpenClipboard(hwnd))
        {
            return;
        }

        try
        {
            if (!NativeClipboard.EmptyClipboard())
            {
                return;
            }

            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(new NRectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            nint hbitmap = NativeClipboard.CreateBitmap(bitmap.Width, bitmap.Height, 1, 32, bitmapData.Scan0);

            bitmap.UnlockBits(bitmapData);

            NativeClipboard.SetClipboardData((uint)NativeClipboard.CLIPFORMAT.CF_BITMAP, hbitmap);
        }
        finally
        {
            NativeClipboard.CloseClipboard();
        }
    }

    private static class NativeClipboard
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool OpenClipboard(nint hWndNewOwner);

        [DllImport("user32.dll")]
        public static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        public static extern nint SetClipboardData(uint uFormat, nint hMem);

        [DllImport("gdi32.dll")]
        public static extern nint CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, nint lpvBits);

        public enum CLIPFORMAT : uint
        {
            CF_TEXT = 1,
            CF_BITMAP = 2,
            CF_METAFILEPICT = 3,
            CF_SYLK = 4,
            CF_DIF = 5,
            CF_TIFF = 6,
            CF_OEMTEXT = 7,
            CF_DIB = 8,
            CF_PALETTE = 9,
            CF_PENDATA = 10,
            CF_RIFF = 11,
            CF_WAVE = 12,
            CF_UNICODETEXT = 13,
            CF_ENHMETAFILE = 14,
            CF_HDROP = 15,
            CF_LOCALE = 16,
            CF_MAX = 17,
            CF_OWNERDISPLAY = 0x80,
            CF_DSPTEXT = 0x81,
            CF_DSPBITMAP = 0x82,
            CF_DSPMETAFILEPICT = 0x83,
            CF_DSPENHMETAFILE = 0x8E,
        }
    }
}
