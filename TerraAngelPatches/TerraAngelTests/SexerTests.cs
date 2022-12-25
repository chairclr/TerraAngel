using TerraAngel;

namespace TerraAngelTests;

public class SexerTests
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void TestEmptyConstructor()
    {
        Sexer sexer = new Sexer();

        Assert.That(sexer.SexerName, Is.EqualTo(Sexer.DefaultSexerName));
    }

    [Test]
    public void TestCreateDefault()
    {
        Sexer sexer = Sexer.CreateDefaultSexer();

        Assert.That(sexer.SexerName, Is.EqualTo(Sexer.DefaultSexerName));
    }

    [Test]
    public void TestStringConstructor()
    {
        string sexerName = "TestSexerName";

        Sexer sexer = new Sexer(sexerName);

        Assert.That(sexer.SexerName, Is.EqualTo(sexerName));
    }
}