using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SDL2;
using Terraria.GameInput;
using Terraria.UI;

namespace Terraria;
public class WindowManager
{
    public enum WindowState
    {
        Windowed,
        BorderlessFullscreen,
        Fullscreen
    }

    [JsonProperty("Width")]
    private int width = 800;
    [JsonProperty("Height")]
    private int height = 600;
    [JsonProperty("Maximized")]
    private bool maximized = false;
    [JsonProperty("WindowState")]
    private WindowState windowState = WindowState.Windowed;
    [JsonProperty("Vsync")]
    private bool vsync = true;
    [JsonProperty("CapFPS")]
    private bool capFPS = false;
    [JsonProperty("FPSCap")]
    private int fPSCap = 200;

    private bool wantToResizeGraphics = false;
    private bool wantToMoveGraphics = false;
    private bool wantToApplyGraphics = false;
    private bool centerWindow = false;

    [JsonIgnore]
    private int x;
    [JsonIgnore]
    private int y;

    [JsonIgnore]
    public GraphicsDeviceManager? Graphics = null;
    [JsonIgnore]
    public IntPtr WindowHandle = IntPtr.Zero;

    [JsonIgnore]
    public int Width
    {
        get => width;
        set
        {
            width = value;
            SDL.SDL_SetWindowSize(WindowHandle, width, height);
            wantToResizeGraphics = true;
        }
    }

    [JsonIgnore]
    public int Height
    {
        get => height;
        set
        {
            height = value;
            SDL.SDL_SetWindowSize(WindowHandle, width, height);
            wantToResizeGraphics = true;
        }
    }

    [JsonIgnore]
    public Vector2i Size
    {
        get => new Vector2i(Width, Height);
        set
        {
            width = value.X;
            height = value.Y;
            SDL.SDL_SetWindowSize(WindowHandle, width, height);
            wantToResizeGraphics = true;
        }
    }

    [JsonIgnore]
    public WindowState State
    {
        get => windowState;
        set
        {
            windowState = value;
            switch (windowState)
            {
                case WindowState.Windowed:
                    {
                        SDL.SDL_SetWindowFullscreen(WindowHandle, 0u);
                        SDL.SDL_SetWindowBordered(WindowHandle, SDL.SDL_bool.SDL_TRUE);
                        SDL.SDL_SetWindowResizable(WindowHandle, SDL.SDL_bool.SDL_TRUE);
                        SDL.SDL_SetWindowSize(WindowHandle, Width, Height);

                        centerWindow = true;
                        Graphics!.IsFullScreen = false;
                        wantToResizeGraphics = true;
                    }
                    break;
                case WindowState.BorderlessFullscreen:
                    {
                        SDL.SDL_SetWindowFullscreen(WindowHandle, 0u);
                        SDL.SDL_SetWindowBordered(WindowHandle, SDL.SDL_bool.SDL_FALSE);
                        SDL.SDL_SetWindowResizable(WindowHandle, SDL.SDL_bool.SDL_FALSE);

                        Graphics!.IsFullScreen = false;
                        wantToResizeGraphics = true;
                    }
                    break;
                case WindowState.Fullscreen:
                    {
                        SDL.SDL_SetWindowFullscreen(WindowHandle, (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP);
                        SDL.SDL_SetWindowBordered(WindowHandle, SDL.SDL_bool.SDL_FALSE);
                        SDL.SDL_SetWindowResizable(WindowHandle, SDL.SDL_bool.SDL_FALSE);

                        Graphics!.IsFullScreen = true;
                        wantToResizeGraphics = true;
                    }
                    break;
            }
        }
    }

    [JsonIgnore]
    public bool Maximized
    {
        get => State == WindowState.Windowed && maximized;
        set
        {
            maximized = value;
            if (State == WindowState.Windowed)
            {
                if (maximized) SDL.SDL_MaximizeWindow(WindowHandle);
            }
        }
    }

    [JsonIgnore]
    public Vector2i MaximumWindowSize
    {
        get
        {
            SDL.SDL_GetDisplayBounds(SDL.SDL_GetWindowDisplayIndex(WindowHandle), out SDL.SDL_Rect rect);
            return new Vector2i(rect.w, rect.h);
        }
    }

    [JsonIgnore]
    public bool Vsync
    {
        get => vsync;
        set
        {
            vsync = value;
            Main.graphics.SynchronizeWithVerticalRetrace = vsync;
            wantToApplyGraphics = true;
        }
    }

    [JsonIgnore]
    public bool CapFPS
    {
        get => capFPS;
        set
        {
            capFPS = value;
            Main.instance.IsFixedTimeStep = value;
        }
    }

    [JsonIgnore]
    public int FPSCap
    {
        get => fPSCap;
        set
        {
            fPSCap = value;
            Main.instance.TargetElapsedTime = TimeSpan.FromSeconds(1d / (double)fPSCap);
        }
    }

    public WindowManager() { }
    public WindowManager(Game game)
    {
        WindowHandle = game.Window.Handle;
        Graphics = Main.graphics;

        if (!File.Exists(ClientLoader.WindowConfigPath))
        {
            WriteToFile();
        }
    }

    public void Init()
    {
        string s = "";
        using (FileStream fs = new FileStream(ClientLoader.WindowConfigPath, FileMode.OpenOrCreate))
        {
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer);
            s = Encoding.UTF8.GetString(buffer);
            fs.Close();
        }

        WindowManager? windowSettings = JsonConvert.DeserializeObject<WindowManager>(s);
        if (windowSettings is not null)
        {
            State = windowSettings.State;
            if (State == WindowState.Windowed)
            {
                Size = windowSettings.Size;
                Maximized = windowSettings.maximized;
            }
            Vsync = windowSettings.Vsync;
            FPSCap = windowSettings.FPSCap;
            CapFPS = windowSettings.CapFPS;
        }

        if (State == WindowState.Windowed) SDL.SDL_GetWindowPosition(WindowHandle, out x, out y);

        wantToResizeGraphics = true;
        centerWindow = true;
    }

    private int updateCount = 0;
    public void Update()
    {
        updateCount++;

        if (State == WindowState.Windowed && centerWindow && !maximized)
        {
            SDL.SDL_GetDisplayBounds(SDL.SDL_GetWindowDisplayIndex(WindowHandle), out SDL.SDL_Rect rect);
            SDL.SDL_SetWindowPosition(WindowHandle, rect.x + rect.w / 2 - Width / 2, rect.y + rect.h / 2 - Height / 2);
            centerWindow = false;
        }
        if (State == WindowState.Windowed)
        {
            uint flags = SDL.SDL_GetWindowFlags(WindowHandle);

            maximized = (flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED) != 0;

            SDL.SDL_GetWindowSize(WindowHandle, out int w, out int h);

            if (width != w || height != h)
            {
                width = w;
                height = h;
                wantToResizeGraphics = true;
            }

            SDL.SDL_GetWindowPosition(WindowHandle, out int lx, out int ly);

            if (x != lx || y != ly)
            {
                x = lx;
                y = ly;
                wantToMoveGraphics = true;
            }
        }

        
        if (wantToResizeGraphics)
        {
            wantToResizeGraphics = false;
            HandleResize();
        }
        if (wantToMoveGraphics)
        {
            wantToMoveGraphics = false;
            HandleMove();
        }
        if (wantToApplyGraphics)
        {
            wantToApplyGraphics = false;
            ApplyGraphics();
        }

        if (updateCount % 300 == 0) WriteToFile();
    }

    public void HandleResize()
    {
        if (State == WindowState.Windowed)
        {
            ResizeGraphics(Width, Height);
        }
        if (State == WindowState.Fullscreen || State == WindowState.BorderlessFullscreen)
        {
            SDL.SDL_GetDisplayBounds(SDL.SDL_GetWindowDisplayIndex(WindowHandle), out SDL.SDL_Rect rect);

            int w = rect.w;
            int h = rect.h;

            ResizeGraphics(w, h);
        }
    }

    public void HandleMove()
    {
        if (State == WindowState.Windowed)
        {
            MoveGraphics(Width, Height);
        }
        if (State == WindowState.Fullscreen || State == WindowState.BorderlessFullscreen)
        {
            SDL.SDL_GetDisplayBounds(SDL.SDL_GetWindowDisplayIndex(WindowHandle), out SDL.SDL_Rect rect);

            int w = rect.w;
            int h = rect.h;

            MoveGraphics(w, h);
        }
    }

    public void ApplyGraphics()
    {
        Main.graphics.ApplyChanges();
    }

    public void ResizeGraphics(int w, int h)
    {
        if (Graphics?.GraphicsDevice is not null)
        {
            Graphics!.GraphicsDevice.Viewport = new Viewport(0, 0, w, h);
        }
        Graphics!.PreferredBackBufferWidth = w;
        Graphics!.PreferredBackBufferHeight = h;
        Graphics!.ApplyChanges();

        Main.screenWidth = w;
        Main.screenHeight = h;

        PlayerInput.RawMouseScale = Vector2.One;

        Main.TryPickingDefaultUIScale(h);
        PlayerInput.CacheOriginalScreenDimensions();
        Main.FixUIScale();
        Main.PendingResolutionWidth = w;
        Main.PendingResolutionHeight = h;
        PlayerInput.CacheOriginalScreenDimensions();
        Lighting.Initialize();
        if (!Main.drawToScreen && !Main._isResizingAndRemakingTargets)
        {
            Main._isResizingAndRemakingTargets = true;
            Main.instance.InitTargets();
            Main._isResizingAndRemakingTargets = false;
        }
        UserInterface.ActiveInstance.Recalculate();
        Main.instance._needsMenuUIRecalculation = true;
    }

    public void MoveGraphics(int w, int h)
    {
        Main.screenWidth = w;
        Main.screenHeight = h;

        PlayerInput.RawMouseScale = Vector2.One;

        Main.TryPickingDefaultUIScale(h);
        PlayerInput.CacheOriginalScreenDimensions();
        Main.FixUIScale();
        Main.PendingResolutionWidth = w;
        Main.PendingResolutionHeight = h;
        PlayerInput.CacheOriginalScreenDimensions();
        UserInterface.ActiveInstance.Recalculate();
        Main.instance._needsMenuUIRecalculation = true;
    }

    public void CenterWindow()
    {
        centerWindow = true;
    }

    private static object FileLock = new object();

    public Task WriteToFile()
    {
        lock (FileLock)
        {
            string s = JsonConvert.SerializeObject(this);

            using (FileStream fs = new FileStream(ClientLoader.WindowConfigPath, FileMode.OpenOrCreate))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                fs.SetLength(bytes.Length);
                fs.Write(bytes);
                fs.Close();
            }
        }

        return Task.CompletedTask;
    }
}
