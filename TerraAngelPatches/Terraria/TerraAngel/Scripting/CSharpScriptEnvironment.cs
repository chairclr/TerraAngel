using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;

namespace TerraAngel.Scripting
{
    public class CSharpScriptEnvironment
    {
        private static readonly string[] defaultUsings = new string[]
        {
            "ImGuiNET",
            "Microsoft.Xna.Framework",
            "Microsoft.Xna.Framework.Graphics",
            "Microsoft.Xna.Framework.Input",
            "MonoMod",
            "MonoMod.RuntimeDetour",
            "System",
            "System.IO",
            "System.Collections.Generic",
            "System.Diagnostics",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Text",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Runtime.CompilerServices",
            "System.Runtime.InteropServices",
            "Terraria",
            "Terraria.DataStructures",
            "Terraria.ID",
            "TerraAngel",
            "TerraAngel.Client",
            "TerraAngel.Client.ClientWindows",
            "TerraAngel.Client.Config",
            "TerraAngel.Cheat",
            "TerraAngel.Cheat.Cringes",
            "TerraAngel.Graphics",
            "TerraAngel.Hooks",
            "TerraAngel.Hooks.Hooks",
            "TerraAngel.ID",
            "TerraAngel.Input",
            "TerraAngel.Net",
            "TerraAngel.Plugin",
            "TerraAngel.Scripting",
            "TerraAngel.UI",
            "TerraAngel.Utility",
            "TerraAngel.WorldEdits",
        };

        public static IEnumerable<Assembly> DefaultAssemblies => AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && File.Exists(x.Location));

        private bool warmedUp = false;
        private ScriptState? scriptState = null;
        private MefHostServices? scriptHost = null;
        private AdhocWorkspace? scriptWorkspace = null;
        private Project? scriptProject = null;
        private Document? scriptDocument = null;
        private DocumentId? scriptDocumentId;
        private CompilationOptions? scriptCompilationOptions => scriptState?.Script.GetCompilation().Options;

        public CSharpScriptEnvironment()
        {

        }

        public Task Warmup()
        {
            if (warmedUp)
                return Task.CompletedTask;

            return Task.Run(
                async () =>
                {
                    ClientLoader.Console.WriteLine("Warming up c# REPL");

                    await CreateScriptState();
                    if (scriptState is null) throw new InvalidOperationException("Failed to create script state.");


                    CreateWorkspace();

                    ProjectInfo? scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp)
                        .WithMetadataReferences(MefHostServices.DefaultAssemblies.Concat(DefaultAssemblies).Select(x => MetadataReference.CreateFromFile(x.Location)))
                        .WithCompilationOptions(scriptCompilationOptions)
                        .WithParseOptions(new CSharpParseOptions(languageVersion: LanguageVersion.Latest, kind: SourceCodeKind.Script));

                    scriptProject = scriptWorkspace?.AddProject(scriptProjectInfo);

                    SubmitCodeToDocument("using System;");

                    warmedUp = true;
                    ClientLoader.Console.WriteLine("Warmed up c# REPL");
                });
        }

        public Task<object?> EvalAsync(string code)
        {
            return Task.Run<object?>(
                async () =>
                {
                    if (scriptState is null) throw new InvalidOperationException("Script state not intialized. Call CSharpScriptEnvironment.Warmup");

                    object? returnValue = null;
                    try
                    {
                        scriptState = await scriptState.ContinueWithAsync(code, catchException: (x) => true);
                        returnValue = scriptState.ReturnValue;
                        SubmitCodeToDocument(code);
                    }
                    catch (CompilationErrorException cex)
                    {
                        ClientLoader.Console.WriteError(cex.Message);
                    }
                    catch (Exception ex)
                    {
                        ClientLoader.Console.WriteError(ex.ToString());
                    }

                    if (scriptState.Exception is not null) ClientLoader.Console.WriteError(scriptState.Exception.ToString());
                    return returnValue;
                });
        }
        public object? Eval(string code)
        {
            return EvalAsync(code).Result;
        }

        public string FormatObject(object? obj)
        {
            return CSharpObjectFormatter.Instance.FormatObject(obj);
        }

        private static char[] includes = new char[] { '.', ',' };
        public Task<List<CompletionItem>> GetCompletionAsync(string code, int cursorPosition)
        {
            return Task.Run(
                async () =>
                {
                    if (!warmedUp) return new List<CompletionItem>();

                    UpdateDocumentWithCode(code);

                    if (scriptDocument is null) return new List<CompletionItem>();

                    CompletionService? completion = CompletionService.GetService(scriptDocument);
                    if (completion is null) return new List<CompletionItem>();
                    if (!completion.ShouldTriggerCompletion(await scriptDocument.GetTextAsync(), cursorPosition, CompletionTrigger.Invoke)) return new List<CompletionItem>();

                    CompletionList? results = await completion.GetCompletionsAsync(scriptDocument, cursorPosition);
                    if (results is null) return new List<CompletionItem>();
                    SyntaxNode? rootNode = await scriptDocument.GetSyntaxRootAsync();

                    string textFilter = "";
                    if (rootNode is not null && !string.IsNullOrWhiteSpace(code))
                    {
                        SyntaxToken token = rootNode.FindToken(Math.Max(cursorPosition - 1, 0));
                        textFilter = token.Text;
                    }

                    if (string.IsNullOrEmpty(textFilter)) return new List<CompletionItem>();
                    if (textFilter.All(x => !char.IsLetter(x) && !includes.Contains(x))) return new List<CompletionItem>();

                    List<CompletionItem> l = completion.FilterItems(scriptDocument, results.Items, textFilter).ToList();


                    if (l.Any(x => x.SortText == textFilter))
                        return new List<CompletionItem>();

                    return l;
                });
        }
        // wip method signature code
        //public Task<string> GetCompletionOverride(string code, int cursorPosition)
        //{
        //    return Task.Run(
        //        async () =>
        //        {
        //            if (!warmedUp) return "";
        //            if (scriptDocument is null) return "";
        //            SyntaxNode? rootNode = await scriptDocument.GetSyntaxRootAsync();

        //            if (rootNode is not null && !string.IsNullOrWhiteSpace(code))
        //            {
        //                SyntaxToken token = rootNode.FindToken(Math.Max(cursorPosition - 1, 0));
        //                SyntaxNode? n = token.Parent;

        //                while (n is not null)
        //                {
        //                    if (n is ArgumentSyntax)
        //                    {
        //                        (n as ArgumentSyntax).
        //                        return "";
        //                    }
        //                    n = n.Parent;
        //                }

        //                return "";
        //            }

        //            return "";
        //        });
        //}
        public string GetChangedText(string code, CompletionItem item, int previousCursorPosition, out int cursorPosition)
        {
            UpdateDocumentWithCode(code);


            if (scriptDocument is null)
            {
                cursorPosition = previousCursorPosition;
                return code;
            }

            try
            {

                CompletionService? completion = CompletionService.GetService(scriptDocument);

                if (completion is null)
                {
                    cursorPosition = previousCursorPosition;
                    return code;
                }

                CompletionChange change = completion.GetChangeAsync(scriptDocument, item).Result;

                string insertedText = change.TextChange.NewText ?? "";
                string newCode = code.Remove(change.TextChange.Span.Start, change.TextChange.Span.Length).Insert(change.TextChange.Span.Start, insertedText);

                if (change.NewPosition.HasValue)
                {
                    cursorPosition = change.NewPosition.Value;
                }
                else
                {
                    cursorPosition = previousCursorPosition + insertedText.Length - change.TextChange.Span.Length;
                }

                return newCode;
            }
            catch (Exception ex)
            {
                cursorPosition = previousCursorPosition;
                return code;
            }
        }

        private void CreateWorkspace()
        {
            Type[] partTypes = MefHostServices.DefaultAssemblies.Concat(
                            AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && File.Exists(x.Location)))
                            .Distinct()
                            .SelectMany(x =>
                            {
                                Type[] types = new Type[0];
                                try { types = x.GetTypes(); } catch (Exception) { }
                                return types;
                            })
                            .ToArray();

            CompositionHost? compositionContext = new ContainerConfiguration()
                .WithParts(partTypes)
                .CreateContainer();

            scriptHost = new MefHostServices(compositionContext);

            scriptWorkspace = new AdhocWorkspace(scriptHost);
        }
        private async Task CreateScriptState()
        {
            ScriptOptions compilationOptions = ScriptOptions.Default
                        .AddReferences(DefaultAssemblies)
                        .AddImports(defaultUsings);
            scriptState = await CSharpScript.RunAsync("using Terraria;", compilationOptions);
        }
        private void SubmitCodeToDocument(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;

            scriptDocument = scriptProject?.AddDocument("Script", SourceText.From(code));
            if (scriptDocumentId is null) scriptDocumentId = scriptDocument?.Id;

            if (scriptDocument is null) throw new InvalidOperationException("Could not append state to document.");
            if (!scriptWorkspace?.TryApplyChanges(scriptDocument.Project.Solution) ?? true) throw new InvalidOperationException("Could not apply changes.");

            scriptDocument = scriptWorkspace?.CurrentSolution.GetDocument(scriptDocumentId);
            scriptProject = scriptDocument?.Project;
        }
        private void UpdateDocumentWithCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;
            if (scriptDocument is null) throw new InvalidOperationException("Could not change state of document.");

            scriptDocument = scriptDocument.WithText(SourceText.From(code));

            if (!scriptWorkspace?.TryApplyChanges(scriptDocument.Project.Solution) ?? true) throw new InvalidOperationException("Could not apply changes.");

            scriptDocument = scriptWorkspace?.CurrentSolution.GetDocument(scriptDocumentId);
            scriptProject = scriptDocument?.Project;
        }


        private List<CompletionItem> FilterCompletionItems(Document document, ImmutableArray<CompletionItem> items, string textFilter)
        {
            if (string.IsNullOrWhiteSpace(textFilter))
                return new List<CompletionItem>();

            string lowerTextFilter = textFilter.ToLower();

            int d(string s)
            {
                return Math.Min(Util.CompareStringDist(s.ToLower(), textFilter), Util.CompareStringDist(s, textFilter));
            }


            List<CompletionItem> filteredItems = new List<CompletionItem>(items.Length);

            for (int i = 0; i < items.Length; i++)
            {
                int dist = d(items[i].SortText);
                if (dist == 0)
                    return new List<CompletionItem>();
                if (dist < 14)
                    filteredItems.Add(items[i]);
            }

            filteredItems.Sort((x, y) => d(x.SortText.ToLower()).CompareTo(d(y.SortText.ToLower())));

            return filteredItems;
        }
    }
}
