using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace TerraAngel.Tests;

public class GameRunner
{
    private Thread? GameThread;

    public void StartGame()
    {
        GameThread = new Thread(() =>
        {
            typeof(WindowsLaunch)
                .GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, new object[] { new string[] { } });
        });

        GameThread.Start();

        Console.WriteLine("Game Started");
    }

    public Task ClientLoad()
    {
        return Task.Run(
            async () =>
            {
                while (!ClientLoader.ClientLoaded || ClientLoader.MainRenderer is null || Main.graphics?.GraphicsDevice is null)
                {
                    await Task.Delay(40);
                }

                Console.WriteLine("Game Loaded");
            });
    }

    public Task StopGame()
    {
        Main.instance.Exit();

        return Task.Run(
            () =>
            {
                GameThread?.Join();
                GameThread = null;

                Console.WriteLine("Game Stopped");
            });
    }
}
