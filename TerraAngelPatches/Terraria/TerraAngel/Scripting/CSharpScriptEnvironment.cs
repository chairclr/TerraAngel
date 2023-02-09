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
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CodeAnalysis.Text;

namespace TerraAngel.Scripting;

public class CSharpScriptEnvironment
{
    private static readonly string[] defaultUsings = new string[]
    {
        "ImGuiNET",
        "Microsoft.Xna.Framework",
        "Microsoft.Xna.Framework.Graphics",
        "Microsoft.Xna.Framework.Input",
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
        "TerraAngel.UI.ClientWindows",
        "TerraAngel.Config",
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

    public bool Ready { get; protected set; }
    private ScriptState? scriptState = null;
    private MefHostServices? scriptHost = null;
    private AdhocWorkspace? scriptWorkspace = null;
    private Project? scriptProject = null;
    private Document? scriptDocument = null;
    private DocumentId? scriptDocumentId;
    private CSharpCompilationOptions? scriptCompilationOptions => (CSharpCompilationOptions?)scriptState?.Script.GetCompilation().Options;
    private string lastString = "";

    public CSharpScriptEnvironment()
    {

    }

    public Task Init()
    {
        if (Ready)
            return Task.CompletedTask;

        return Task.Run(
            async () =>
            {
                try
                {
                    await CreateScriptState();
                    if (scriptState is null) throw new InvalidOperationException("Failed to create script state.");

                    CreateWorkspace();

                    ProjectInfo? scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp)
                        .WithMetadataReferences(MefHostServices.DefaultAssemblies.Concat(DefaultAssemblies).Select(x => MetadataReference.CreateFromFile(x.Location)))
                        .WithCompilationOptions(scriptCompilationOptions)
                        .WithParseOptions(new CSharpParseOptions(languageVersion: LanguageVersion.Latest, kind: SourceCodeKind.Script));

                    scriptProject = scriptWorkspace?.AddProject(scriptProjectInfo);

                    SubmitCodeToDocument("using System;");

                    Ready = true;
                    ClientLoader.Console.WriteLine("C# Scripting Engine Initialized");
                }
                catch (Exception ex)
                {
                    ClientLoader.Console.WriteLine($"Exception occured when initializing C# scripting engine: {ex}");
                }
            });
    }

    public Task<object?> EvalAsync(string code)
    {
        return Task.Run<object?>(
            async () =>
            {
                if (!Ready) throw new InvalidOperationException("Script state not intialized. Call CSharpScriptEnvironment.Warmup");

                object? returnValue = null;
                try
                {
                    scriptState = await scriptState!.ContinueWithAsync(code, catchException: (x) => true);
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

                if (scriptState?.Exception is not null) ClientLoader.Console.WriteError(scriptState.Exception.ToString());
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

    public void SetText(string code)
    {
        if (!Ready) return;
        if (code == lastString) return;

        UpdateDocumentWithCode(code);
        lastString = code;
    }

    private static char[] includes = new char[] { '.', ',' };
    public Task<List<CompletionItem>> GetCompletionAsync(string code, int cursorPosition)
    {
        return Task.Run(
            async () =>
            {
                SetText(code);

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

                List<CompletionItem> l = FilterCompletionItems(results.Items, textFilter, 0.7f); // completion.FilterItems(scriptDocument, results.Items, textFilter).ToList();


                if (l.Any(x => x.SortText == textFilter))
                    return new List<CompletionItem>();

                return l;
            });
    }
    public Task<List<string>> GetArgumentListCompletionSymbolsAsync(string code, int cursorPosition)
    {
        return Task.Run(
            async () =>
            {
                if (!Ready) new List<string>();
                if (scriptDocument is null) return new List<string>();
                if (string.IsNullOrWhiteSpace(code)) return new List<string>();

                SetText(code);

                SyntaxNode? rootNode = await scriptDocument.GetSyntaxRootAsync();
                SemanticModel? semanticModel = await scriptDocument.GetSemanticModelAsync();

                if (rootNode == null) return new List<string>();
                if (semanticModel == null) return new List<string>();

                SyntaxToken tokenAtCursor = rootNode.FindToken(Math.Max(cursorPosition - 1, 0));
                SyntaxNode? node = FindParentArgumentList(tokenAtCursor.Parent);

                if (node is not null && node.Parent is not null)
                {
                    SymbolInfo info = semanticModel.GetSymbolInfo(node.Parent);

                    List<string> candidates = new List<string>(info.CandidateSymbols.Length);
                    if (info.Symbol is not null)
                    {
                        candidates.Add(info.Symbol.ToMinimalDisplayString(semanticModel, cursorPosition));
                        return candidates;
                    }
                    for (int i = 0; i < info.CandidateSymbols.Length; i++)
                    {
                        ISymbol symbol = info.CandidateSymbols[i];
                        candidates.Add(symbol.ToMinimalDisplayString(semanticModel, cursorPosition));
                    }
                    return candidates;
                }

                return new List<string>();
            });
    }
    public string GetChangedText(string code, CompletionItem item, int previousCursorPosition, out int cursorPosition)
    {
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

    // wip code
    public string FormatDocument(string code, int previousCursorPosition, out int cursorPosition)
    {
        cursorPosition = previousCursorPosition;
        if (!Ready) return code;
        if (scriptDocument is null) return code;

        try
        {

            SyntaxNode? n = scriptDocument?.GetSyntaxRootAsync().Result;

            if (n is null) return code;

            SyntaxNode formattedNode = Formatter.Format(n.NormalizeWhitespace("    "), scriptWorkspace, scriptWorkspace.Options);

            return formattedNode.GetText().ToString();
        }
        catch (Exception ex)
        {
        }
        return code;
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

        if (scriptWorkspace is null) return;

        OptionSet options = scriptWorkspace.Options;

        options = options.WithChangedOption(CSharpFormattingOptions.IndentBraces, false);
        options = options.WithChangedOption(CSharpFormattingOptions.IndentBlock, true);
        options = options.WithChangedOption(new OptionKey(FormattingOptions.UseTabs, "CSharp"), false);

        scriptWorkspace.TryApplyChanges(scriptWorkspace.CurrentSolution.WithOptions(options));
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

        SourceText source = SourceText.From(code);

        List<string> usings = ((CSharpCompilationOptions)scriptProject!.CompilationOptions).Usings.ToList();

        scriptDocument = scriptProject?.AddDocument("Script", source);

        foreach (SyntaxNode node in scriptDocument!.GetSyntaxTreeAsync().Result!.GetRoot().DescendantNodes())
        {
            if (node is UsingDirectiveSyntax)
            {
                UsingDirectiveSyntax usingNode = (UsingDirectiveSyntax)node;

                string t = usingNode.Name.ToString();

                if (!usings.Contains(t))
                    usings.Add(t);
            }
        }

        scriptDocument = scriptProject?.WithCompilationOptions(((CSharpCompilationOptions)scriptProject!.CompilationOptions).WithUsings(usings)).AddDocument("Script", source);

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
    private SyntaxNode? FindParentArgumentList(SyntaxNode? node)
    {
        SyntaxNode? workingNode = node;
        while (workingNode is not null)
        {
            if (workingNode is ArgumentListSyntax)
            {
                return workingNode;
            }
            workingNode = workingNode.Parent;
        }
        return null;
    }
    private List<CompletionItem> FilterCompletionItems(ImmutableArray<CompletionItem> items, string textFilter, float fuzziness)
    {
        if (string.IsNullOrWhiteSpace(textFilter))
            return new List<CompletionItem>();

        if (textFilter.Length == 1 && !char.IsLetter(textFilter[0]))
            return items.ToList();
        string lowerTextFilter = textFilter.ToLower();

        int dist(string s)
        {
            return Math.Min(StringExtensions.CompareStringDist(s.ToLower(), textFilter), StringExtensions.CompareStringDist(s, textFilter));
        }

        List<CompletionItem> filteredItems = new List<CompletionItem>(items.Length / 10);

        for (int i = 0; i < items.Length; i++)
        {
            string sortText = items[i].SortText;

            if (sortText.ToLower().Contains(textFilter.ToLower()))
            {
                filteredItems.Add(items[i]);
            }
            else
            {
                int d = dist(sortText);
                int length = Math.Max(sortText.Length, textFilter.Length);

                float score = 1.0f - (float)d / (float)length;

                if (score > fuzziness)
                {
                    filteredItems.Add(items[i]);
                }
            }
        }

        filteredItems.Sort((x, y) => dist(x.SortText).CompareTo(dist(y.SortText)));

        return filteredItems;
    }
}
