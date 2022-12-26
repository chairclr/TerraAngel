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
    public void RemoveCringeTest()
    {
        Assert.Throws<KeyNotFoundException>(() =>
        {
            CringeManager.AddCringe<TestCringe>();
            CringeManager.RemoveCringe<TestCringe>();

            TestCringe cringe = CringeManager.GetCringe<TestCringe>();
        });
    }

    [Test]
    public void AddMultipleCringesTest()
    {
        Assert.Throws<ArgumentException>(() => 
        { 
            CringeManager.AddCringe<TestCringe>();
            CringeManager.AddCringe<TestCringe>(); 
        });

        CringeManager.RemoveCringe<TestCringe>();
    }

    [Test]
    public void SortCringesTest()
    {
        CringeManager.AddCringe<DTestCringe>();
        CringeManager.AddCringe<BTestCringe>();
        CringeManager.AddCringe<CTestCringe>();
        CringeManager.AddCringe<ATestCringe>();

        CringeManager.SortTabs();

        List<Cringe> cringes = CringeManager.GetCringeOfTab(CringeTabs.None);

        int aIndex = cringes.IndexOf(CringeManager.GetCringe<ATestCringe>());
        int bIndex = cringes.IndexOf(CringeManager.GetCringe<BTestCringe>());
        int cIndex = cringes.IndexOf(CringeManager.GetCringe<CTestCringe>());
        int dIndex = cringes.IndexOf(CringeManager.GetCringe<DTestCringe>());

        Assert.Multiple(() =>
        {
            Assert.That(aIndex, Is.LessThan(bIndex).And.LessThan(cIndex).And.LessThan(dIndex));
            Assert.That(bIndex, Is.GreaterThan(aIndex).And.LessThan(cIndex).And.LessThan(dIndex));
            Assert.That(cIndex, Is.GreaterThan(aIndex).And.GreaterThan(bIndex).And.LessThan(dIndex));
            Assert.That(dIndex, Is.GreaterThan(aIndex).And.GreaterThan(bIndex).And.GreaterThan(cIndex));
        });

        CringeManager.RemoveCringe<ATestCringe>();
        CringeManager.RemoveCringe<BTestCringe>();
        CringeManager.RemoveCringe<CTestCringe>();
        CringeManager.RemoveCringe<DTestCringe>();
    }

    private class TestCringe : Cringe
    {
        public override CringeTabs Tab => CringeTabs.None;

        public override void DrawUI(ImGuiIOPtr io)
        {

        }
    }

    private class ATestCringe : Cringe
    {
        public override CringeTabs Tab => CringeTabs.None;

        public override void DrawUI(ImGuiIOPtr io)
        {

        }
    }

    private class BTestCringe : Cringe
    {
        public override CringeTabs Tab => CringeTabs.None;

        public override void DrawUI(ImGuiIOPtr io)
        {

        }
    }

    private class CTestCringe : Cringe
    {
        public override CringeTabs Tab => CringeTabs.None;

        public override void DrawUI(ImGuiIOPtr io)
        {

        }
    }

    private class DTestCringe : Cringe
    {
        public override CringeTabs Tab => CringeTabs.None;

        public override void DrawUI(ImGuiIOPtr io)
        {

        }
    }
}
