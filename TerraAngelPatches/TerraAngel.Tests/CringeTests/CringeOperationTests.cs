using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using TerraAngel.Cheat;

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
        public override ToolTabs Tab => ToolTabs.None;

        public override void DrawUI(ImGuiIOPtr io)
        {

        }
    }

    private class ATestCringe : Tool
    {
        public override ToolTabs Tab => ToolTabs.None;

        public override void DrawUI(ImGuiIOPtr io)
        {

        }
    }

    private class BTestCringe : Tool
    {
        public override ToolTabs Tab => ToolTabs.None;

        public override void DrawUI(ImGuiIOPtr io)
        {

        }
    }

    private class CTestCringe : Tool
    {
        public override ToolTabs Tab => ToolTabs.None;

        public override void DrawUI(ImGuiIOPtr io)
        {

        }
    }

    private class DTestCringe : Tool
    {
        public override ToolTabs Tab => ToolTabs.None;

        public override void DrawUI(ImGuiIOPtr io)
        {

        }
    }
}
