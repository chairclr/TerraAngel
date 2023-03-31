using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpEval;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Tags;
using Terraria.GameContent.ObjectInteractions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Terraria.WorldBuilding.Modifiers;

namespace TerraAngel.UI.ClientWindows.Console;

public class ConsoleScriptWindow
{
    public FullCSharpEvaluator? Script;

    private ImmutableArray<CompletionItem> ScriptCompletionItems = ImmutableArray<CompletionItem>.Empty;

    private ImmutableArray<string> ScriptCompletionArguments = ImmutableArray<string>.Empty;

    private int SelectedCompletionItem = 0;

    private int CompletionViewCenterIndex = 0;

    private int SelectedCompletionOverload = 0;

    private readonly object CompletionLock = new object();

    public ConsoleScriptWindow()
    {
        string[] usings = new string[]
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
            "TerraAngel.Tools",
            "TerraAngel.Tools.Automation",
            "TerraAngel.Tools.Developer",
            "TerraAngel.Tools.Map",
            "TerraAngel.Tools.Visuals",
            "TerraAngel.Graphics",
            "TerraAngel.Hooks",
            "TerraAngel.Hooks",
            "TerraAngel.ID",
            "TerraAngel.Input",
            "TerraAngel.Net",
            "TerraAngel.Plugin",
            "TerraAngel.UI",
            "TerraAngel.Utility",
            "TerraAngel.WorldEdits"
        };

        Task.Run(() =>
        {
            ClientLoader.Console.WriteLine("Initializing Scripting Evaluator");
            try

            {
                Script = new FullCSharpEvaluator(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).Where(x => x != typeof(Steamworks.AppId_t).Assembly), usings);
                ClientLoader.Console.WriteLine("Finished Initializing Scripting Evaluator");
            }
            catch (Exception ex)
            {
                ClientLoader.Console.WriteLine("Failed to Initialzie Scripting Evaluator");
                ClientLoader.Console.WriteLine(ex.ToString());
            }
        });
    }

    public void Draw(Vector2 inputTextMin, Vector2 inputTextMax)
    {
        ImmutableArray<CompletionItem> completionItems = ScriptCompletionItems;
        ImmutableArray<string> argumentCompletionItems = ScriptCompletionArguments;

        ImGuiIOPtr io = ImGui.GetIO();
        ImGuiStylePtr style = ImGui.GetStyle();
        ImDrawListPtr drawList = ImGui.GetForegroundDrawList();

        if (completionItems.Length > 0)
        {
            #region Completion Items Draw
            SelectedCompletionItem = Math.Clamp(SelectedCompletionItem, 0, completionItems.Length - 1);
            CompletionViewCenterIndex = Math.Clamp(CompletionViewCenterIndex, 0, completionItems.Length - 1);

            int startIndex = Math.Clamp(CompletionViewCenterIndex - 5, 0, completionItems.Length - 1);
            int endIndex = Math.Clamp(startIndex + 10, 0, completionItems.Length);

            if ((endIndex - startIndex) < 10)
            {
                startIndex = Utils.Clamp(endIndex - 10, 0, completionItems.Length);
            }

            Vector2 size = CalculateCompletionWindowSize(startIndex, endIndex);
            (Vector2 origin, bool flipCompletion) = CalculateCompletionWindowOrigin(inputTextMin, inputTextMax, size);

            drawList.AddRectFilled(origin, origin + size, ImGui.GetColorU32(ImGuiCol.WindowBg));

            float currentDrawOffset = style.ItemSpacing.Y;

            void DrawCompletionItem(CompletionItem item)
            {
                string text = item.DisplayText;

                Color iconColor = item.Tags.Length > 0 ? GetTagColor(item.Tags[0]) : GetTagColor();

                Color textColor = Color.Gray;

                if (item == completionItems[SelectedCompletionItem])
                {
                    textColor = Color.White;
                }

                drawList.AddText(origin + new Vector2(style.ItemSpacing.X, currentDrawOffset), iconColor.PackedValue, item.Tags.Length > 0 ? GetTagIcon(item.Tags[0]) : GetTagIcon());

                drawList.AddText(origin + new Vector2(style.ItemSpacing.X * 2f + 18f, currentDrawOffset), textColor.PackedValue, text);

                currentDrawOffset += ImGui.CalcTextSize(text).Y + style.ItemSpacing.Y;
            }

            if (flipCompletion)
            {
                for (int i = endIndex - 1; i > startIndex - 1; i--)
                {
                    DrawCompletionItem(completionItems[i]);
                }
            }
            else
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    DrawCompletionItem(completionItems[i]);
                }
            }
            #endregion

            if (ImGui.IsKeyPressed(ImGuiKey.UpArrow, true))
            {
                SelectedCompletionItem -= flipCompletion ? -1 : 1;
            }
            else if (ImGui.IsKeyPressed(ImGuiKey.DownArrow, true))
            {
                SelectedCompletionItem += flipCompletion ? -1 : 1;
            }

            if (SelectedCompletionItem + 5 < CompletionViewCenterIndex)
                CompletionViewCenterIndex = SelectedCompletionItem + 5;
            else if (SelectedCompletionItem - 4 > CompletionViewCenterIndex)
                CompletionViewCenterIndex = SelectedCompletionItem - 4;

            if (argumentCompletionItems.Length > 0)
            {
                if (InputSystem.Shift)
                {
                    if (ImGui.IsKeyPressed(ImGuiKey.UpArrow, true))
                    {
                        SelectedCompletionOverload++;
                    }
                    else if (ImGui.IsKeyPressed(ImGuiKey.DownArrow, true))
                    {
                        SelectedCompletionOverload--;
                    }

                    SelectedCompletionOverload %= ScriptCompletionArguments.Length;

                    if (SelectedCompletionOverload < 0)
                    {
                        SelectedCompletionOverload = ScriptCompletionArguments.Length + SelectedCompletionOverload;
                    }
                }

                SelectedCompletionOverload = Math.Clamp(SelectedCompletionOverload, 0, argumentCompletionItems.Length - 1);

                string currentText = (ScriptCompletionArguments.Length > 1 ? $"({SelectedCompletionOverload + 1}/{ScriptCompletionArguments.Length}) " : "") + ScriptCompletionArguments[SelectedCompletionOverload];
                Vector2 argumentOrigin = new Vector2(inputTextMin.X, inputTextMax.Y + size.Y + style.ItemSpacing.Y);
                Vector2 argumentSize = new Vector2(ImGui.CalcTextSize(currentText).X + style.ItemSpacing.X * 2f, ImGui.CalcTextSize(currentText).Y + style.ItemSpacing.Y * 2f);

                if (argumentOrigin.X + argumentSize.X > io.DisplaySize.X)
                {
                    argumentOrigin.X -= (argumentOrigin.X + argumentSize.X) - io.DisplaySize.X;
                }
                if (argumentOrigin.Y + argumentSize.Y > io.DisplaySize.Y)
                {
                    argumentOrigin.Y = inputTextMin.Y - style.ItemSpacing.Y - argumentSize.Y;
                }
                if (flipCompletion)
                {
                    argumentOrigin.Y = inputTextMin.Y - style.ItemSpacing.Y * 2f - argumentSize.Y - size.Y;
                }

                drawList.AddRectFilled(argumentOrigin, argumentOrigin + argumentSize, ImGui.GetColorU32(ImGuiCol.WindowBg));

                drawList.AddText(argumentOrigin + style.ItemSpacing, Color.White.PackedValue, currentText);
            }
        }
        else
        {
            if (argumentCompletionItems.Length > 0)
            {
                if (ImGui.IsKeyPressed(ImGuiKey.UpArrow, true))
                {
                    SelectedCompletionOverload++;
                }
                else if (ImGui.IsKeyPressed(ImGuiKey.DownArrow, true))
                {
                    SelectedCompletionOverload--;
                }

                SelectedCompletionOverload %= ScriptCompletionArguments.Length;

                if (SelectedCompletionOverload < 0)
                {
                    SelectedCompletionOverload = ScriptCompletionArguments.Length + SelectedCompletionOverload;
                }

                if (argumentCompletionItems.Length > 0)
                {
                    SelectedCompletionOverload = Math.Clamp(SelectedCompletionOverload, 0, argumentCompletionItems.Length - 1);

                    string currentText = (ScriptCompletionArguments.Length > 1 ? $"({SelectedCompletionOverload + 1}/{ScriptCompletionArguments.Length}) " : "") + ScriptCompletionArguments[SelectedCompletionOverload];
                    Vector2 origin = new Vector2(inputTextMin.X, inputTextMax.Y + 0f + style.ItemSpacing.Y);
                    Vector2 size = new Vector2(ImGui.CalcTextSize(currentText).X + style.ItemSpacing.X * 2f, ImGui.CalcTextSize(currentText).Y + style.ItemSpacing.Y * 2f);

                    if (origin.X + size.X > io.DisplaySize.X)
                    {
                        origin.X -= (origin.X + size.X) - io.DisplaySize.X;
                    }
                    if (origin.Y + size.Y > io.DisplaySize.Y)
                    {
                        origin.Y = inputTextMin.Y - style.ItemSpacing.Y - size.Y;
                    }
                    //if (FlipCompletion && ScriptCompletionItems.Any())
                    //{
                    //    origin.Y = textboxMin.Y - style.ItemSpacing.Y * 2f - size.Y - 0f;
                    //}

                    drawList.AddRectFilled(origin, origin + size, ImGui.GetColorU32(ImGuiCol.WindowBg));

                    drawList.AddText(origin + style.ItemSpacing, Color.White.PackedValue, currentText);
                }
            }
        }
    }

    public void RunScript(string message)
    {
        if (Script is null)
        {
            ClientLoader.Console.WriteError("Console script not initialized");
            return;
        }

        ScriptEvaluationResult result = Script.Eval(message);
        if (result.Result is not null)
        {
            ClientLoader.Console.WriteLine(CSharpObjectFormatter.Instance.FormatObject(result.Result));
        }

        if (result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Any())
        {
            foreach (Diagnostic diagnostic in result.Diagnostics)
            {
                ClientLoader.Console.WriteError(diagnostic.ToString());
            }
        }

        if (result.Exception is not null)
        {
            ClientLoader.Console.WriteError(result.Exception.ToString());
        }
    }

    public void UpdateCompletionItems(string text, int cursorPosition)
    {
        if (Script is null)
        {
            ClientLoader.Console.WriteError("Console script not initialized");
            return;
        }

        Task.Run(async () =>
        {
            ImmutableArray<CompletionItem> items = await Script.GetCompletionsAsync(text, cursorPosition, CompletionTrigger.Invoke);
            ImmutableArray<string> argumentItems = await Script.GetSymbolArgumentsAsync(text, cursorPosition);

            lock (CompletionLock)
            {
                ScriptCompletionItems = items;
                ScriptCompletionArguments = argumentItems;
            }
        });
    }

    public bool ScrollThroughCompletions(ImGuiInputTextCallbackDataPtr data)
    {
        return ScriptCompletionItems.Length > 0 || ScriptCompletionArguments.Length > 0;
    }

    public bool TriggerCompletion(ImGuiInputTextCallbackDataPtr data)
    {
        if (Script is null)
        {
            ClientLoader.Console.WriteError("Console script not initialized");
            return false;
        }

        UpdateCompletionItems(data.GetText(), data.CursorPos);

        if (ScriptCompletionItems.Length > 0)
        {
            (string text, int newCursorPos) = Script.ApplyCompletionAsync(data.GetText(), ScriptCompletionItems[SelectedCompletionItem], data.CursorPos, null).Result;

            data.SetText(text);
            data.CursorPos = newCursorPos;

            UpdateCompletionItems(data.GetText(), data.CursorPos);

            return true;
        }

        return false;
    }

    private Color GetTagColor(string? tag = null)
    {
        return tag switch
        {
            WellKnownTags.Field or WellKnownTags.Interface or WellKnownTags.Local => new Color(0x00, 0x5d, 0xba),
            WellKnownTags.Method or WellKnownTags.ExtensionMethod => new Color(0x69, 0x36, 0xaa),
            WellKnownTags.Namespace or WellKnownTags.Property or WellKnownTags.Keyword => new Color(0xe0, 0xe0, 0xe0),
            WellKnownTags.Class or WellKnownTags.Enum or WellKnownTags.Event => new Color(0xff, 0xe3, 0x9e),
            WellKnownTags.Structure => new Color(0x55, 0xaa, 0xff),
            WellKnownTags.Constant => new Color(0x55, 0xaa, 0xff),
            WellKnownTags.Delegate => new Color(0x92, 0x64, 0xcd),
            WellKnownTags.EnumMember => new Color(0x55, 0xaa, 0xff),
            _ => new Color(0xff, 0xe3, 0x9e),
        };
    }

    private string GetTagIcon(string? tag = null)
    {
        return tag switch
        {
            WellKnownTags.Field => Icon.SymbolField,
            WellKnownTags.Method or WellKnownTags.ExtensionMethod => Icon.SymbolMethod,
            WellKnownTags.Namespace => Icon.SymbolNamespace,
            WellKnownTags.Class => Icon.SymbolClass,
            WellKnownTags.Structure => Icon.SymbolStructure,
            WellKnownTags.Enum => Icon.SymbolEnum,
            WellKnownTags.Interface => Icon.SymbolInterface,
            WellKnownTags.Event => Icon.SymbolEvent,
            WellKnownTags.Property => Icon.SymbolProperty,
            WellKnownTags.Constant => Icon.SymbolConstant,
            WellKnownTags.Delegate => Icon.TypeHierarchySub,
            WellKnownTags.EnumMember => Icon.SymbolEnumMember,
            WellKnownTags.Keyword => Icon.SymbolKeyword,
            WellKnownTags.Local => Icon.SymbolVariable,
            _ => Icon.SymbolMisc,
        };
    }

    private Vector2 CalculateCompletionWindowSize(int start, int end)
    {
        ImGuiStylePtr style = ImGui.GetStyle();

        float maxWidth = 0f;
        float drawHeight = style.ItemSpacing.Y * 2f;

        for (int i = start; i < end; i++)
        {
            Vector2 textSize = ImGui.CalcTextSize(ScriptCompletionItems[i].DisplayText);

            if (textSize.X > maxWidth)
            {
                maxWidth = textSize.X;
            }

            drawHeight += textSize.Y + style.ItemSpacing.Y;
        }

        maxWidth += 18f + style.ItemSpacing.X * 3.75f + style.ItemSpacing.X * 1.5f;

        return new Vector2(maxWidth, drawHeight);
    }

    private (Vector2, bool) CalculateCompletionWindowOrigin(Vector2 inputTextMin, Vector2 inputTextMax, Vector2 size)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        ImGuiStylePtr style = ImGui.GetStyle();

        Vector2 origin = new Vector2(inputTextMin.X, inputTextMax.Y + style.ItemSpacing.Y);
        if (origin.X + size.X > io.DisplaySize.X)
        {
            origin.X -= (origin.X + size.X) - io.DisplaySize.X;
        }

        bool flipCompletion = false;

        if (origin.Y + size.Y > io.DisplaySize.Y)
        {
            flipCompletion = true;
            origin.Y = inputTextMin.Y - style.ItemSpacing.Y - size.Y;
        }

        return (origin, flipCompletion);
    }

    private void RenderScriptCandidates(ImGuiIOPtr io, Vector2 textboxMin, Vector2 textboxMax)
    {
        ImGuiStylePtr style = ImGui.GetStyle();

        //    Vector2 origin = new Vector2(textboxMin.X, textboxMax.Y + style.ItemSpacing.Y);
        //    Vector2 size = new Vector2(maxSize + style.ItemSpacing.X * 2f, drawHeight);
        //    if (origin.X + size.X > io.DisplaySize.X)
        //    {
        //        origin.X -= (origin.X + size.X) - io.DisplaySize.X;
        //    }
        //    FlipCompletion = false;
        //    if (origin.Y + size.Y > io.DisplaySize.Y)
        //    {
        //        FlipCompletion = true;
        //        origin.Y = textboxMin.Y - style.ItemSpacing.Y - size.Y;
        //    }
        //    drawList.AddRectFilled(origin, origin + size, ImGui.GetColorU32(ImGuiCol.WindowBg));


        //    float offset = style.ItemSpacing.Y;
        //    void RenderCandidate(int i)
        //    {
        //        string s = GetCandidateText(i);
        //        Color iconColor = GetIconColor(i);
        //        Color col = Color.Gray;

        //        float offsetOffset = ImGui.CalcTextSize(s).Y + style.ItemSpacing.Y;
        //        if (Util.IsMouseHoveringRect(origin + new Vector2(style.ItemSpacing.X, offset + 1f), origin + new Vector2(maxSize, offset + offsetOffset - 1f)))
        //        {
        //            col = Color.Gray * 1.45f;
        //        }

        //        if (i == SelectedCompletionItem)
        //        {
        //            col = Color.White;
        //        }

        //        drawList.AddText(origin + new Vector2(style.ItemSpacing.X * 2f + 18f, offset), col.PackedValue, s);

        //        drawList.AddText(origin + new Vector2(style.ItemSpacing.X, offset), iconColor.PackedValue, GetCandidateIcon(i));
        //        offset += offsetOffset;
        //    }
        //    if (FlipCompletion)
        //    {
        //        for (int i = endCandidate - 1; i > startCandidate - 1; i--)
        //        {
        //            RenderCandidate(i);
        //        }
        //    }
        //    else
        //    {
        //        for (int i = startCandidate; i < endCandidate; i++)
        //        {
        //            RenderCandidate(i);
        //        }
        //    }


        //    completionOrigin = origin;
        //    completionSize = size;

        //    if (Util.IsMouseHoveringRect(origin, origin + size))
        //    {

        //        io.WantCaptureMouse = true;

        //        if (CompletionViewIndex > 4 && CompletionViewIndex < ScriptCompletionItems.Length - 4)
        //            CompletionViewIndex -= (int)io.MouseWheel;
        //        else
        //        {
        //            if (CompletionViewIndex <= 5) CompletionViewIndex = 5;
        //            else if (CompletionViewIndex >= ScriptCompletionItems.Length - 4) CompletionViewIndex = ScriptCompletionItems.Length - 5;
        //        }


        //        ImGui.SetItemDefaultFocus();
        //        ImGui.SetKeyboardFocusHere(-1);
        //    }
        //}

        //if (ScriptCompletionArguments.Any())
        //{
        //    SelectedCompletionOverload = Math.Clamp(SelectedCompletionOverload, 0, ScriptCompletionArguments.Length - 1);
        //    ImDrawListPtr drawList = ImGui.GetForegroundDrawList();
        //    string currentText = (ScriptCompletionArguments.Length > 1 ? $"({SelectedCompletionOverload + 1}/{ScriptCompletionArguments.Length}) " : "") + ScriptCompletionArguments[SelectedCompletionOverload];
        //    Vector2 origin = new Vector2(textboxMin.X, textboxMax.Y + completionSize.Y + style.ItemSpacing.Y * 2f);
        //    Vector2 size = new Vector2(ImGui.CalcTextSize(currentText).X + style.ItemSpacing.X * 2f, ImGui.CalcTextSize(currentText).Y + style.ItemSpacing.Y * 2f);

        //    if (origin.X + size.X > io.DisplaySize.X)
        //    {
        //        origin.X -= (origin.X + size.X) - io.DisplaySize.X;
        //    }
        //    if (origin.Y + size.Y > io.DisplaySize.Y)
        //    {
        //        origin.Y = textboxMin.Y - style.ItemSpacing.Y - size.Y;
        //    }
        //    if (FlipCompletion && ScriptCompletionItems.Any())
        //    {
        //        origin.Y = textboxMin.Y - style.ItemSpacing.Y * 2f - size.Y - completionSize.Y;
        //    }

        //    drawList.AddRectFilled(origin, origin + size, ImGui.GetColorU32(ImGuiCol.WindowBg));

        //    drawList.AddText(origin + style.ItemSpacing, Color.White.PackedValue, currentText);
        //}
    }
}
