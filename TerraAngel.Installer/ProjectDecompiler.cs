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
                    Directory.CreateDirectory(Path.Combine(TargetDirectory, "EmbeddedResources"));

                    string newDir = "EmbeddedResources";

                    string newFileName = Path.Combine(newDir, x.FileName);

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
