using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;

namespace TerraAngel.Utility
{
    public class CSharpREPL
    {
        private static ScriptState<object>? scriptState = null;
        private static string[] defaultUsings = new string[] 
        {
            "System",
            "System.IO",
            "System.Collections.Generic",
            "System.Console", 
            "System.Diagnostics", 
            "System.Dynamic",
            "System.Linq",
            "System.Linq.Expressions", 
            "System.Net.Http",
            "System.Text",
            "System.Threading.Tasks",
            "Terraria",
            "TerraAngel",
            "TerraAngel.Client",
            "TerraAngel.Client.ClientWindows",
            "TerraAngel.Client.Config",
            "TerraAngel.Cheat",
            "Microsoft.Xna.Framework",
        };

        public static IEnumerable<Assembly> RefernceAssemblies => AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && File.Exists(x.Location));

        public static Task Warmup()
        {
            return Task.Run(
                async ()=>
                {
                    ClientLoader.Console.WriteLine("Warming up c# REPL");
                    if (scriptState is null)
                    {
                        ScriptOptions compilationOptions = ScriptOptions.Default
                            .AddReferences(RefernceAssemblies)
                            .AddImports(defaultUsings);
                        scriptState = await CSharpScript.RunAsync("", compilationOptions);
                    }

                    CompilationOptions options = scriptState.Script.GetCompilation().Options;

                    if (completionProject is null)
                    {
                        Type[] partTypes = MefHostServices.DefaultAssemblies.Concat(
                            AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && File.Exists(x.Location)))
                            .Distinct()
                            .SelectMany(x =>
                            {
                                Type[] types = new Type[0];
                                try { types = x.GetTypes(); } catch (Exception e) { }
                                return types;
                            })
                            .ToArray();

                        CompositionHost? compositionContext = new ContainerConfiguration()
                            .WithParts(partTypes)
                            .CreateContainer();

                        completionHost = new MefHostServices(compositionContext);

                        completionWorkspace = new AdhocWorkspace(completionHost);

                        ProjectInfo? scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true)
                            .WithMetadataReferences(MefHostServices.DefaultAssemblies.Concat(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && File.Exists(x.Location))).Select(x => MetadataReference.CreateFromFile(x.Location)))
                            .WithCompilationOptions(options);

                        completionProject = completionWorkspace.AddProject(scriptProjectInfo);

                        completionDocument = completionProject.AddDocument("Script", "").WithSourceCodeKind(SourceCodeKind.Script);
                    }

                    ClientLoader.Console.WriteLine("Warmed up c# REPL");
                });
        }

        public static Task ExecuteAsync(string code)
        {
            return Task.Run(
                async () =>
                {
                    if (scriptState is null)
                    {
                        ScriptOptions compilationOptions = ScriptOptions.Default
                            .AddReferences(RefernceAssemblies)
                            .AddImports(defaultUsings);
                        scriptState = await CSharpScript.RunAsync("", compilationOptions);
                    }

                    try
                    {
                        scriptState = await scriptState.ContinueWithAsync(code, catchException: (x) => true);
                    }
                    catch (CompilationErrorException cex)
                    {
                        ClientLoader.Console.WriteError(cex.Message);
                    }
                    catch (Exception ex)
                    {
                        ClientLoader.Console.WriteError(ex.ToString());
                    }

                    if (scriptState.ReturnValue is not null) ClientLoader.Console.WriteLine(CSharpObjectFormatter.Instance.FormatObject(scriptState.ReturnValue));
                    if (scriptState.Exception is not null) ClientLoader.Console.WriteError(scriptState.Exception.ToString());
                });
        }
        public static void Execute(string code)
        {
            if (scriptState is null)
            {
                ScriptOptions compilationOptions = ScriptOptions.Default
                    .AddReferences(RefernceAssemblies)
                    .AddImports(defaultUsings);
                scriptState = CSharpScript.RunAsync("", compilationOptions).Result;
            }

            bool success = false;
            try
            {
                scriptState = scriptState.ContinueWithAsync(code, catchException: (x) => true).Result;
                success = true;
            }
            catch (CompilationErrorException cex)
            {
                ClientLoader.Console.WriteError(cex.Message);
            }
            catch (Exception ex)
            {
                ClientLoader.Console.WriteError(ex.ToString());
            }

            if (success)
            {
                if (scriptState.ReturnValue is not null) ClientLoader.Console.WriteLine(CSharpObjectFormatter.Instance.FormatObject(scriptState.ReturnValue));
                if (scriptState.Exception is not null) ClientLoader.Console.WriteError(scriptState.Exception.ToString());
            }
        }

        private static MefHostServices? completionHost = null;
        private static AdhocWorkspace? completionWorkspace = null;
        private static Project? completionProject = null;
        private static Document? completionDocument = null;
        public static Task GetCompletionAsync(string code, int cursorPosition, Action<List<CompletionItem>> action)
        {
            return Task.Run(
                async () =>
                {
                    if (scriptState is null)
                    {
                        ScriptOptions compilationOptions = ScriptOptions.Default
                            .AddReferences(RefernceAssemblies)
                            .AddImports(defaultUsings);
                        scriptState = CSharpScript.RunAsync("", compilationOptions).Result;
                    }

                    CompilationOptions options = scriptState.Script.GetCompilation().Options;

                    if (completionProject is null)
                    {
                        Type[] partTypes = MefHostServices.DefaultAssemblies.Concat(RefernceAssemblies)
                            .Distinct()
                            .SelectMany(x =>
                            {
                                Type[] types = new Type[0];
                                try { types = x.GetTypes(); } catch (Exception e) { }
                                return types;
                            })
                            .ToArray();

                        CompositionHost? compositionContext = new ContainerConfiguration()
                            .WithParts(partTypes)
                            .CreateContainer();

                        completionHost = new MefHostServices(compositionContext);

                        completionWorkspace = new AdhocWorkspace(completionHost);

                        ProjectInfo? scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true)
                            .WithMetadataReferences(MefHostServices.DefaultAssemblies.Concat(RefernceAssemblies)
                            .Select(x => MetadataReference.CreateFromFile(x.Location)))
                            .WithCompilationOptions(options);

                        completionProject = completionWorkspace.AddProject(scriptProjectInfo);

                        completionDocument = completionProject.AddDocument("Script", code).WithSourceCodeKind(SourceCodeKind.Script);
                    }

                    completionDocument = completionDocument.WithText(Microsoft.CodeAnalysis.Text.SourceText.From(code));

                    try
                    {

                        CompletionService? completion = CompletionService.GetService(completionDocument);

                        if (completion is null)
                        {
                            action(new List<CompletionItem>());
                            return;
                        }

                        CompletionList? results = await completion?.GetCompletionsAsync(completionDocument, cursorPosition);

                        int lastWord = (int)MathF.Max(MathF.Max(code.LastIndexOf('.'), code.LastIndexOf(';')), code.LastIndexOf(' '));
                        lastWord = lastWord == -1 ? 0 : lastWord;
                        lastWord += 1;
                        if (!(lastWord < code.Length))
                            lastWord = code.Length - 1;

                        string t = code.Substring(lastWord);
                        List<CompletionItem> items = results.Items.Where(x => x.DisplayText.ToLower().Contains(t.ToLower())).ToList();

                        items.Sort((x, y) => CompareStringDist(x.DisplayText, t).CompareTo(CompareStringDist(y.DisplayText, t)));
                        action(items);
                    }
                    catch (Exception ex)
                    {

                    }
                });
        }


        public static int CompareStringDist(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException(s, "String Cannot Be Null Or Empty");
            }

            if (string.IsNullOrEmpty(t))
            {
                throw new ArgumentNullException(t, "String Cannot Be Null Or Empty");
            }

            int n = s.Length; // length of s
            int m = t.Length; // length of t

            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            int[] p = new int[n + 1]; //'previous' cost array, horizontally
            int[] d = new int[n + 1]; // cost array, horizontally

            // indexes into strings s and t
            int i; // iterates through s
            int j; // iterates through t

            for (i = 0; i <= n; i++)
            {
                p[i] = i;
            }

            for (j = 1; j <= m; j++)
            {
                char tJ = t[j - 1]; // jth character of t
                d[0] = j;

                for (i = 1; i <= n; i++)
                {
                    int cost = s[i - 1] == tJ ? 0 : 1; // cost
                                                       // minimum of cell to the left+1, to the top+1, diagonally left and up +cost                
                    d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
                }

                // copy current distance counts to 'previous row' distance counts
                int[] dPlaceholder = p; //placeholder to assist in swapping p and d
                p = d;
                d = dPlaceholder;
            }

            // our last action in the above loop was to switch d and p, so p now 
            // actually has the most recent cost counts
            return p[n];
        }
    }
}
