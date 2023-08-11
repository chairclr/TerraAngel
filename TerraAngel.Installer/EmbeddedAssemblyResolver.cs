using System.Runtime.Serialization;
using System.Xml.Linq;
using ICSharpCode.Decompiler.Metadata;

namespace TerraAngel.Installer;

internal class EmbeddedAssemblyResolver : IAssemblyResolver
{
    private readonly PEFile MainModule;

    private readonly Dictionary<string, PEFile> CachedAssemblies = new Dictionary<string, PEFile>();

    private readonly UniversalAssemblyResolver FallbackResolver;

    private readonly IEnumerable<Resource> EmbeddedResources;

    public EmbeddedAssemblyResolver(PEFile mainModule)
    {
        MainModule = mainModule;

        FallbackResolver = new UniversalAssemblyResolver(mainModule.FileName, true, mainModule.DetectTargetFrameworkId());

        EmbeddedResources = MainModule.Resources.Where(r => r.ResourceType == ResourceType.Embedded);
    }

    public PEFile? Resolve(IAssemblyReference reference)
    {
        if (CachedAssemblies.TryGetValue(reference.FullName, out PEFile? module))
        {
            return module;
        }

        string resourceName = $"{reference.Name}.dll";

        Resource? res = EmbeddedResources.SingleOrDefault(r => r.Name.EndsWith(resourceName));

        if (res is not null)
        {
            module = new PEFile(res.Name, res.TryOpenStream()!);
            CachedAssemblies.Add(reference.FullName, module);
            return module;
        }

        module = FallbackResolver.Resolve(reference);
        
        if (module is null)
        {
            throw new Exception($"Cound not resolve reference to {reference.FullName}");
        }

        CachedAssemblies.Add(reference.FullName, module);

        return module;
    }

    public Task<PEFile?> ResolveAsync(IAssemblyReference reference)
    {
        return Task.Run<PEFile?>(() =>
        {
            PEFile? module;

            lock (CachedAssemblies)
            {
                if (CachedAssemblies.TryGetValue(reference.FullName, out module))
                {
                    return module;
                }
            }

            string resourceName = $"{reference.Name}.dll";

            Resource? res = EmbeddedResources.SingleOrDefault(r => r.Name.EndsWith(resourceName));

            if (res is not null)
            {
                module = new PEFile(res.Name, res.TryOpenStream()!);
            }

            if (module is null)
            {
                module = FallbackResolver.Resolve(reference);

                // If the fallback resolver fails, we're in trouble
                if (module is null)
                {
                    throw new Exception($"Cound not resolve reference to {reference.FullName}");
                }
            }

            lock (CachedAssemblies)
            {
                CachedAssemblies.Add(reference.FullName, module);
            }

            return module;
        });
    }

    public PEFile? ResolveModule(PEFile mainModule, string moduleName)
    {
        return FallbackResolver.ResolveModule(mainModule, moduleName);
    }

    public Task<PEFile?> ResolveModuleAsync(PEFile mainModule, string moduleName)
    {
        return FallbackResolver.ResolveModuleAsync(mainModule, moduleName);
    }
}
