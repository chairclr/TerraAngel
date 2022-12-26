using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraAngel.Tests;

[SetUpFixture]
public class Preload
{
    private GameRunner Game = new GameRunner();

    [OneTimeSetUp]
    public async Task Setup()
    {
        Game.StartGame();

        await Game.ClientLoad();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await Game.StopGame();
    }
}
