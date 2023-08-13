using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.Solution;
using ICSharpCode.Decompiler.Util;

namespace TerraAngel.Installer;

internal class ProjectDecompiler : WholeProjectDecompiler
{
    public ProjectDecompiler(IAssemblyResolver assemblyResolver)
        : base(assemblyResolver)
    {

    }

    protected override IEnumerable<ProjectItemInfo> WriteResourceFilesInProject(PEFile module)
    {
        IEnumerable<ProjectItemInfo> items = base.WriteResourceFilesInProject(module)
            .Select(x =>
            {
                if (x.ItemType == "EmbeddedResource")
                {
                    string[] endings = new string[] { "Game.json", "Items.json", "Legacy.json", "NPCs.json", "Projectiles.json", "Town.json" };

                    Directory.CreateDirectory(Path.Combine(TargetDirectory, "EmbeddedResources"));

                    string newDir = "EmbeddedResources";

                    string newFileName = Path.Combine(newDir, x.FileName);
                    
                    if (x.FileName.StartsWith("Terraria.Localization.Content.") && x.FileName.EndsWith(".json") && !endings.Any(y => newFileName.EndsWith(y)))
                    {
                        newFileName = newFileName.Remove(newFileName.Length - ".json".Length) + ".Content.json";
                    }

                    File.Copy(Path.Combine(TargetDirectory, x.FileName), Path.Combine(TargetDirectory, newFileName));

                    File.Delete(Path.Combine(TargetDirectory, x.FileName));

                    x = x with
                    {
                        FileName = newFileName
                    };
                }

                return x;
            });

        return items;
    }
}
