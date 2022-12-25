using System.Reflection;
using ImGuiNET;
using Terraria;

namespace TerraAngel.Tests.ClientRendererTests;
public class ClientFontTests
{
    private GameRunner Game = new GameRunner();

    [Test]
    public async Task VerifyImGuiFont()
    {
        Game.StartGame();
        await Game.WaitForClientToLoad();

        Assert.That(ClientLoader.MainRenderer!.FontTextureId!.Value, Is.EqualTo(ImGui.GetIO().Fonts.TexID));

        await Game.StopGame();
    }
}
