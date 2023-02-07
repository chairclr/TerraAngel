using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;
using TerraAngel.Timing;
using TerraAngel.Vectors;
using Terraria;

namespace TerraAngel.Tests.WindowTests;
public class WindowResizeTests
{
    [Test]
    public void TestResizeWindow()
    {
        const int ChangeToWidth = 459;
        const int ChangeToHeight = 689;

        ClientLoader.WindowManager!.Width = ChangeToWidth;
        ClientLoader.WindowManager!.Height = ChangeToHeight;

        Thread.Sleep(500);

        SDL.SDL_GetWindowSize(Main.instance.Window.Handle, out int windowWidth, out int windowHeight);

        int graphicsWidth = Main.graphics.GraphicsDevice.Viewport.Width;
        int graphicsHeight = Main.graphics.GraphicsDevice.Viewport.Height;

        Assert.Multiple(() =>
        {
            Assert.That(windowWidth, Is.EqualTo(ChangeToWidth));
            Assert.That(windowHeight, Is.EqualTo(ChangeToHeight));

            Assert.That(graphicsWidth, Is.EqualTo(ChangeToWidth));
            Assert.That(graphicsHeight, Is.EqualTo(ChangeToHeight));
        });
    }

    [Test]
    public void TestInvalidSize()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ClientLoader.WindowManager!.Width = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => ClientLoader.WindowManager!.Height = -1);

            Assert.Throws<ArgumentOutOfRangeException>(() => ClientLoader.WindowManager!.Width = int.MaxValue / 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => ClientLoader.WindowManager!.Height = int.MaxValue / 2);
        });
    }
}
