using System.Reflection;
using ImGuiNET;
using Terraria;

namespace TerraAngel.Tests.ClientRendererTests;

public class ClientFontTests
{
    [Test]
    public void VerifyImGuiFont()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ClientLoader.MainRenderer!.FontTextureId!.HasValue);
            Assert.That(ClientLoader.MainRenderer!.FontTextureId!.Value, Is.EqualTo(ImGui.GetIO().Fonts.TexID));
        });
    }
}
