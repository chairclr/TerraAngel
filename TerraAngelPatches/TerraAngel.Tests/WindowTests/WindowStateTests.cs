using SDL2;
using Terraria;

namespace TerraAngel.Tests.WindowTests;

public class WindowStateTests
{
    [SetUp]
    public void Setup()
    {
        ClientLoader.WindowManager!.State = WindowManager.WindowState.Windowed;
        ClientLoader.WindowManager!.Width = 1920;
        ClientLoader.WindowManager!.Height = 1080;

        Thread.Sleep(1000);
    }

    [Test]
    public void GoWindowed()
    {
        ClientLoader.WindowManager!.State = WindowManager.WindowState.Windowed;

        SDL.SDL_WindowFlags windowFlags = (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(ClientLoader.WindowManager.WindowHandle);

        Assert.Multiple(() =>
        {
            Assert.That((int)(windowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE),          Is.Not.EqualTo(0));
            Assert.That((int)(windowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN),         Is.EqualTo(0));
            Assert.That((int)(windowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP), Is.EqualTo(0));
            Assert.That((int)(windowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS),         Is.EqualTo(0));
        });
    }

    [Test]
    public void GoFullscreen()
    {
        ClientLoader.WindowManager!.State = WindowManager.WindowState.Fullscreen;

        Thread.Sleep(1000);

        SDL.SDL_WindowFlags windowFlags = (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(ClientLoader.WindowManager.WindowHandle);

        SDL.SDL_GetWindowSize(ClientLoader.WindowManager.WindowHandle, out int windowWidth, out int windowHeight);

        Assert.Multiple(() =>
        {
            Assert.That((int)(windowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE),          Is.EqualTo(0));
            Assert.That((int)(windowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN),         Is.Not.EqualTo(0));
            Assert.That((int)(windowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP), Is.Not.EqualTo(0));
            Assert.That((int)(windowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS),         Is.Not.EqualTo(0));

            Assert.That(windowWidth, Is.EqualTo(ClientLoader.WindowManager.MaximumWindowSize.X));
            Assert.That(windowHeight, Is.EqualTo(ClientLoader.WindowManager.MaximumWindowSize.Y));
        });
    }

    [Test]
    public void GoBorderlessFullscreen()
    {
        ClientLoader.WindowManager!.State = WindowManager.WindowState.BorderlessFullscreen;

        Thread.Sleep(1000);

        SDL.SDL_WindowFlags windowFlags = (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(ClientLoader.WindowManager.WindowHandle);

        SDL.SDL_GetWindowSize(ClientLoader.WindowManager.WindowHandle, out int windowWidth, out int windowHeight);

        Assert.Multiple(() =>
        {
            Assert.That((int)(windowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE),          Is.EqualTo(0));
            Assert.That((int)(windowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN),         Is.EqualTo(0));
            Assert.That((int)(windowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP), Is.EqualTo(0));
            Assert.That((int)(windowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS),         Is.Not.EqualTo(0));

            Assert.That(windowWidth, Is.EqualTo(ClientLoader.WindowManager.MaximumWindowSize.X));
            Assert.That(windowHeight, Is.EqualTo(ClientLoader.WindowManager.MaximumWindowSize.Y));
        });
    }
}
