using TerraAngel.UI.ClientWindows;

namespace TerraAngel.Tests.ConsoleTests;

public class CommandParseTests
{
    [TestCase("-1", "-1 -1 -1 -1")]
    [TestCase("test", "test a j nskadjas ")]
    [TestCase("test", " test dsaj nskadjas ")]
    [TestCase("a", "    a   b b   b test dsaj nskadjas ")]
    [TestCase("", "")]
    public void GetCommandTest(string command, string fullText)
    {
        ConsoleWindow.CmdStr cmdStr = new ConsoleWindow.CmdStr(fullText);

        Assert.That(cmdStr.Command, Is.EqualTo(command));
    }

    [TestCase(new string[]{ "-1", "-1", "-1" }, "-1 -1 -1 -1")]
    [TestCase(new string[] { "a", "j", "nskadjas" }, "test a j nskadjas ")]
    [TestCase(new string[] { "dsaj", "nskadjas" }, " test dsaj nskadjas ")]
    [TestCase(new string[] { "b", "b", "b", "test", "dsaj", "nskadjas" }, "    a   b b   b test dsaj nskadjas ")]
    [TestCase(new string[] { }, "")]
    public void GetArgumentsTest(string[] args, string fullText)
    {
        ConsoleWindow.CmdStr cmdStr = new ConsoleWindow.CmdStr(fullText);

        Assert.That(cmdStr.Args, Is.EquivalentTo(args));
    }

    [TestCase("-1 -1 -1", "-1 -1 -1 -1")]
    [TestCase("a j nskadjas ", "test a j nskadjas ")]
    [TestCase("dsaj nskadjas ", " test dsaj nskadjas ")]
    [TestCase("b b   b test dsaj nskadjas ", "    a   b b   b test dsaj nskadjas ")]
    [TestCase("", "")]
    public void GetFullArgsTest(string fullArgs, string fullText)
    {
        ConsoleWindow.CmdStr cmdStr = new ConsoleWindow.CmdStr(fullText);
        Assert.That(cmdStr.FullArgs, Is.EquivalentTo(fullArgs));
    }
}
