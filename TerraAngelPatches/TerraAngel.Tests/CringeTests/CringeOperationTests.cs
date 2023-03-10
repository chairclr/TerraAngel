using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using TerraAngel.Tools;

namespace TerraAngel.Tests.CringeTests;

public class CringeOperationTests
{
    [Test]
    public void RemoveToolTest()
    {
        Assert.Throws<KeyNotFoundException>(() =>
        {
            ToolManager.AddTool<TestCringe>();
            ToolManager.RemoveTool<TestCringe>();

            TestCringe cringe = ToolManager.GetTool<TestCringe>();
        });
    }

    [Test]
    public void AddMultipleCringesTest()
    {
        Assert.Throws<ArgumentException>(() => 
        { 
            ToolManager.AddTool<TestCringe>();
            ToolManager.AddTool<TestCringe>(); 
        });

        ToolManager.RemoveTool<TestCringe>();
    }

    [Test]
    public void SortCringesTest()
    {
        ToolManager.AddTool<DTestCringe>();
        ToolManager.AddTool<BTestCringe>();
        ToolManager.AddTool<CTestCringe>();
        ToolManager.AddTool<ATestCringe>();

        ToolManager.SortTabs();

        List<Tool> cringes = ToolManager.GetToolsOfTab(ToolTabs.None);

        int aIndex = cringes.IndexOf(ToolManager.GetTool<ATestCringe>());
        int bIndex = cringes.IndexOf(ToolManager.GetTool<BTestCringe>());
        int cIndex = cringes.IndexOf(ToolManager.GetTool<CTestCringe>());
        int dIndex = cringes.IndexOf(ToolManager.GetTool<DTestCringe>());

        Assert.Multiple(() =>
        {
            Assert.That(aIndex, Is.LessThan(bIndex).And.LessThan(cIndex).And.LessThan(dIndex));
            Assert.That(bIndex, Is.GreaterThan(aIndex).And.LessThan(cIndex).And.LessThan(dIndex));
            Assert.That(cIndex, Is.GreaterThan(aIndex).And.GreaterThan(bIndex).And.LessThan(dIndex));
            Assert.That(dIndex, Is.GreaterThan(aIndex).And.GreaterThan(bIndex).And.GreaterThan(cIndex));
        });

        ToolManager.RemoveTool<ATestCringe>();
        ToolManager.RemoveTool<BTestCringe>();
        ToolManager.RemoveTool<CTestCringe>();
        ToolManager.RemoveTool<DTestCringe>();
    }

    private class TestCringe : Tool
    {

    }

    private class ATestCringe : Tool
    {

    }

    private class BTestCringe : Tool
    {

    }

    private class CTestCringe : Tool
    {

    }

    private class DTestCringe : Tool
    {

    }
}
