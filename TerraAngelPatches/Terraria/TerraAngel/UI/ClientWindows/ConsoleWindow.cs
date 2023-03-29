using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CSharpEval;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Tags;
using TerraAngel.Plugin;
using static System.Net.Mime.MediaTypeNames;

namespace TerraAngel.UI.ClientWindows;

public class ConsoleWindow : ClientWindow
{
    public override bool DefaultEnabled => true;

    public override bool IsEnabled => ClientConfig.Settings.ShowConsoleWindow;

    public override bool IsToggleable => true;

    public override string Title => "Console";

    private string ConsoleInput = "";

    public Dictionary<string, ConsoleCommand> ConsoleCommands = new Dictionary<string, ConsoleCommand>();

    public List<ConsoleElement> ConsoleItems = new List<ConsoleElement>();

    public List<string> ConsoleHistory => ClientConfig.Settings.ConsoleHistory;

    private int HistoryPosition = -1;

    private List<UndoState> UndoStack = new List<UndoState>() { new UndoState("", 0) };

    public int UndoStackSize => ClientConfig.Settings.ConsoleUndoStackSize;

    private int UndoPosition = 1;

    public FullCSharpEvaluator Script;

    private ImmutableArray<CompletionItem> ScriptCompletionItems = ImmutableArray<CompletionItem>.Empty;

    private int SelectedCompletionItem = 0;

    private int CompletionViewIndex = 0;

    private bool FlipCompletion = false;

    private ImmutableArray<string> ScriptCompletionArguments = ImmutableArray<string>.Empty;

    private int SelectedCompletionOverload = 0;

    private readonly object ConsoleLock = new object();

    private readonly object CandidateLock = new object();

    private bool ForceConsoleInputFocus = false;

    private bool ForecConsoleReclaimFocus = false;

    public bool ScrollToBottom = false;

    public ConsoleWindow()
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
            WriteLine("Initializing Scripting Evaluator");
            try

            {
                Script = new FullCSharpEvaluator(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).Where(x => x != typeof(Steamworks.AppId_t).Assembly), usings);
                WriteLine("Finished Initializing Scripting Evaluator");
            }
            catch (Exception ex) 
            {
                WriteError("Failed to Initialzie Scripting Evaluator");
                WriteError(ex.ToString());
            }
        });

        AddCommand(
            "clear",
            (x) =>
            {
                ConsoleItems.Clear();
            }, "Clears the console");

        AddCommand(
            "help",
            (x) =>
            {
                foreach (ConsoleCommand command in ConsoleCommands.Values)
                {
                    WriteLine($"{command.CommandName}: {command.CommandDescription}");
                }
            }, "Prints help");

        AddCommand(
            "reload_plugins",
            (x) =>
            {
                ClientConfig.WriteToFile();
                PluginLoader.UnloadPlugins();
                PluginLoader.LoadAndInitializePlugins();
                ClientLoader.PluginUI!.NeedsUpdate = true;
            }, "Reloads plugins");

        WriteLine("Type #help for a list of commands");
    }

    public override void Draw(ImGuiIOPtr io)
    {
        ImGui.PushFont(ClientAssets.GetMonospaceFont(16f));

        ImGuiStylePtr style = ImGui.GetStyle();

        Vector2 windowSize = io.DisplaySize / new Vector2(2.8f, 1.9f);
        ImGui.SetNextWindowPos(new Vector2(io.DisplaySize.X - windowSize.X, io.DisplaySize.Y - windowSize.Y), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(windowSize, ImGuiCond.FirstUseEver);

        ImGui.SetNextWindowBgAlpha(0.85f);

        if (!ImGui.Begin("Console"))
        {
            ImGui.End();
            return;
        }

        float footerHeight = style.ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
        ImGui.BeginChild("ConsoleScrollingRegion", new Vector2(0, -footerHeight), false, ImGuiWindowFlags.HorizontalScrollbar);

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));
        float wrapWidth = ImGui.GetContentRegionAvail().X;
        lock (ConsoleLock)
        {
            for (int i = 0; i < ConsoleItems.Count; i++)
            {
                ConsoleElement item = ConsoleItems[i];
                ImGui.PushStyleColor(ImGuiCol.Text, item.TextColor.PackedValue);
                string text = "";
                if (item.AboveDuplicateCount > 0) text = $"{item.Text} ({item.AboveDuplicateCount})";
                else text = item.Text;

                Vector2 textSize = ImGui.CalcTextSize(text, wrapWidth);
                if (ImGui.IsRectVisible(textSize))
                {
                    if (ImGuiUtil.WrappedSelectable($"coni{i}", text, wrapWidth))
                    {
                        ImGui.SetClipboardText(item.Text);
                    }
                }
                else
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textSize.Y + (i + 1 < ConsoleItems.Count ? 1f : 0f));
                    ImGui.Dummy(Vector2.Zero);
                }

                ImGui.PopStyleColor();
            }
        }
        ImGui.PopStyleVar();
        if (ScrollToBottom || (ClientConfig.Settings.ConsoleAutoScroll && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())))
            ImGui.SetScrollY(ImGui.GetScrollMaxY() * 2);

        ImGui.EndChild();

        ScrollToBottom = false;


        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X);
        Vector2 minInput;
        Vector2 maxInput;
        ForceConsoleInputFocus = false;
        unsafe
        {
            lock (CandidateLock)
            {
                // wip code for multiline W
                //if (ImGui.InputTextMultiline("##consoleInput", ref consoleInput, 2048, new Vector2(ImGui.GetWindowWidth() - style.ItemSpacing.X * 2f, MathF.Min(ImGui.CalcTextSize(consoleInput + " ").Y, ImGui.CalcTextSize(" ").Y * 6f) + style.ItemSpacing.Y * 2f), ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CtrlEnterForNewLine | ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackAlways | ImGuiInputTextFlags.CallbackEdit | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.NoUndoRedo, (x) => { lock (CandidateLock) { return TextEditCallback(x); } }))
                if (ImGui.InputText("##consoleInput", ref ConsoleInput, 2048, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackAlways | ImGuiInputTextFlags.CallbackEdit | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.NoUndoRedo, (x) => { lock (CandidateLock) { return TextEditCallback(x); } }))
                {
                    if (ConsoleInput.Length > 0)
                    {
                        WriteLine(">> " + ConsoleInput, new Color(0, 255, 0, 255));
                        ExecuteMessage(ConsoleInput);
                        ConsoleInput = "";

                        if (ClientConfig.Settings.ConsoleAutoScroll) ScrollToBottom = true;
                    }

                    ForecConsoleReclaimFocus = true;
                }
            }
            minInput = ImGui.GetItemRectMin();
            maxInput = ImGui.GetItemRectMax();
        }

        ImGui.SetItemDefaultFocus();
        if (ForecConsoleReclaimFocus)
        {
            ForecConsoleReclaimFocus = false;
            ImGui.SetKeyboardFocusHere(-1);
        }



        lock (CandidateLock)
        {
            if (ForceConsoleInputFocus)
            {
                if (ConsoleInput.Trim().Length == 0)
                {
                    ScriptCompletionArguments.Clear();
                    ScriptCompletionItems.Clear();
                }
                RenderScriptCandidates(io, minInput, maxInput);
            }
        }

        ImGui.End();
        ImGui.PopFont();
    }

    public void WriteLine(string message)
    {
        WriteLine(message, Color.White);
    }

    public void WriteError(string error)
    {
        WriteLine($"[Error] {error}", Color.Red);
    }

    public void WriteLine(string message, Color color)
    {
        lock (ConsoleLock)
        {
            if (ConsoleItems.Count > 0)
            {
                ConsoleElement lastElement = ConsoleItems[^1];
                if (message == lastElement.Text)
                {
                    lastElement.AboveDuplicateCount++;
                }
                else
                {
                    ConsoleItems.Add(new ConsoleElement(message, color, 0));
                }
            }
            else
            {
                ConsoleItems.Add(new ConsoleElement(message, color, 0));
            }
        }
    }

    public void AddCommand(string name, Action<CmdStr> action, string description = "No Description Given")
    {
        if (!ConsoleCommands.ContainsKey(name))
        {
            ConsoleCommands.Add(name, new ConsoleCommand(name, action, description));
        }
        else
        {
            ConsoleCommand command = ConsoleCommands[name];
            command.CommandAction = action;
            command.CommandDescription = description;
        }
    }

    public void RemoveCommand(string name)
    {
        ConsoleCommands.Remove(name);
    }

    private void ExecuteMessage(string message)
    {
        HistoryPosition = -1;
        for (int i = ConsoleHistory.Count - 1; i >= 0; i--)
        {
            if (ConsoleHistory[i] == message)
            {
                ConsoleHistory.RemoveAt(i);
                break;
            }
        }

        if (ConsoleHistory.Count > ClientConfig.Settings.ConsoleHistoryLimit)
            ConsoleHistory.RemoveRange(0, ConsoleHistory.Count - ClientConfig.Settings.ConsoleHistoryLimit);

        ConsoleHistory.Add(message);

        if (message.StartsWith("#"))
        {
            CmdStr cmd = new CmdStr(message.Remove(0, 1));
            if (ConsoleCommands.TryGetValue(cmd.Command, out ConsoleCommand? command))
            {
                command.CommandAction(cmd);
                return;
            }
        }

        UpdateCompletion("", 0);

        ScriptEvaluationResult result = Script.Eval(ConsoleInput);
        if (result.Result is not null)
        {
            WriteLine(CSharpObjectFormatter.Instance.FormatObject(result.Result));
        }
        if (result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Any())
        {
            foreach (Diagnostic diagnostic in result.Diagnostics)
            {
                WriteError(diagnostic.ToString());
            }
        }
        if (result.Exception is not null)
        {
            WriteError(result.Exception.ToString());
        }
    }

    private void RenderScriptCandidates(ImGuiIOPtr io, Vector2 textboxMin, Vector2 textboxMax)
    {
        ImGuiStylePtr style = ImGui.GetStyle();

        SelectedCompletionItem = Utils.Clamp(SelectedCompletionItem, 0, ScriptCompletionItems.Length - 1);
        CompletionViewIndex = Utils.Clamp(CompletionViewIndex, 0, ScriptCompletionItems.Length - 1);
        Vector2 completionOrigin = Vector2.Zero;
        Vector2 completionSize = Vector2.Zero;

        if (ScriptCompletionItems.Any())
        {
            string GetCandidateIcon(int i)
            {
                if (ScriptCompletionItems[i].Tags.Length > 0)
                {
                    string tag = ScriptCompletionItems[i].Tags[0];
                    switch (tag)
                    {
                        case WellKnownTags.Field:
                            return Icon.SymbolField;
                        case WellKnownTags.Method:
                        case WellKnownTags.ExtensionMethod:
                            return Icon.SymbolMethod;
                        case WellKnownTags.Namespace:
                            return Icon.SymbolNamespace;
                        case WellKnownTags.Class:
                            return Icon.SymbolClass;
                        case WellKnownTags.Structure:
                            return Icon.SymbolStructure;
                        case WellKnownTags.Enum:
                            return Icon.SymbolEnum;
                        case WellKnownTags.Interface:
                            return Icon.SymbolInterface;
                        case WellKnownTags.Event:
                            return Icon.SymbolEvent;
                        case WellKnownTags.Property:
                            return Icon.SymbolProperty;
                        case WellKnownTags.Constant:
                            return Icon.SymbolConstant;
                        case WellKnownTags.Delegate:
                            return Icon.TypeHierarchySub;
                        case WellKnownTags.EnumMember:
                            return Icon.SymbolEnumMember;
                        case WellKnownTags.Keyword:
                            return Icon.SymbolKeyword;
                        case WellKnownTags.Local:
                            return Icon.SymbolVariable;

                    }
                }
                return Icon.SymbolMisc;
            }
            Color GetIconColor(int i)
            {
                if (ScriptCompletionItems[i].Tags.Length > 0)
                {
                    string tag = ScriptCompletionItems[i].Tags[0];
                    switch (tag)
                    {
                        case WellKnownTags.Field:
                        case WellKnownTags.Interface:
                        case WellKnownTags.Local:
                            return new Color(0x00, 0x5d, 0xba);
                        case WellKnownTags.Method:
                        case WellKnownTags.ExtensionMethod:
                            return new Color(0x69, 0x36, 0xaa);
                        case WellKnownTags.Namespace:
                        case WellKnownTags.Property:
                        case WellKnownTags.Keyword:
                            return new Color(0xe0, 0xe0, 0xe0);
                        case WellKnownTags.Class:
                        case WellKnownTags.Enum:
                        case WellKnownTags.Event:
                            return new Color(0xff, 0xe3, 0x9e);
                        case WellKnownTags.Structure:
                            return new Color(0x55, 0xaa, 0xff);
                        case WellKnownTags.Constant:
                            return new Color(0x55, 0xaa, 0xff);
                        case WellKnownTags.Delegate:
                            return new Color(0x92, 0x64, 0xcd);
                        case WellKnownTags.EnumMember:
                            return new Color(0x55, 0xaa, 0xff);
                    }
                }
                return new Color(0xff, 0xe3, 0x9e);
            }
            string GetCandidateText(int i)
            {
                return ScriptCompletionItems[i].DisplayText;
            }

            ImDrawListPtr drawList = ImGui.GetForegroundDrawList();


            float maxSize = 0f;
            float drawHeight = style.ItemSpacing.Y * 2f;

            int startCandidate = Utils.Clamp(CompletionViewIndex - 5, 0, ScriptCompletionItems.Length);
            int endCandidate = Utils.Clamp(startCandidate + 10, 0, ScriptCompletionItems.Length);
            if ((endCandidate - startCandidate) < 10)
            {
                startCandidate = Utils.Clamp(endCandidate - 10, 0, ScriptCompletionItems.Length);
            }

            for (int i = startCandidate; i < endCandidate; i++)
            {
                string s = GetCandidateText(i);
                Vector2 textSize = ImGui.CalcTextSize(s);
                if (textSize.X > maxSize)
                    maxSize = textSize.X;
                drawHeight += textSize.Y + style.ItemSpacing.Y;
            }
            maxSize += 18f + style.ItemSpacing.X * 3.75f;
            Vector2 origin = new Vector2(textboxMin.X, textboxMax.Y + style.ItemSpacing.Y);
            Vector2 size = new Vector2(maxSize + style.ItemSpacing.X * 2f, drawHeight);
            if (origin.X + size.X > io.DisplaySize.X)
            {
                origin.X -= (origin.X + size.X) - io.DisplaySize.X;
            }
            FlipCompletion = false;
            if (origin.Y + size.Y > io.DisplaySize.Y)
            {
                FlipCompletion = true;
                origin.Y = textboxMin.Y - style.ItemSpacing.Y - size.Y;
            }
            drawList.AddRectFilled(origin, origin + size, ImGui.GetColorU32(ImGuiCol.WindowBg));


            float offset = style.ItemSpacing.Y;
            void RenderCandidate(int i)
            {
                string s = GetCandidateText(i);
                Color iconColor = GetIconColor(i);
                Color col = Color.Gray;



                float offsetOffset = ImGui.CalcTextSize(s).Y + style.ItemSpacing.Y;
                if (Util.IsMouseHoveringRect(origin + new Vector2(style.ItemSpacing.X, offset + 1f), origin + new Vector2(maxSize, offset + offsetOffset - 1f)))
                {
                    col = Color.Gray * 1.45f;
                }

                if (i == SelectedCompletionItem)
                {
                    col = Color.White;
                }

                drawList.AddText(origin + new Vector2(style.ItemSpacing.X * 2f + 18f, offset), col.PackedValue, s);

                drawList.AddText(origin + new Vector2(style.ItemSpacing.X, offset), iconColor.PackedValue, GetCandidateIcon(i));
                offset += offsetOffset;
            }
            if (FlipCompletion)
            {
                for (int i = endCandidate - 1; i > startCandidate - 1; i--)
                {
                    RenderCandidate(i);
                }
            }
            else
            {
                for (int i = startCandidate; i < endCandidate; i++)
                {
                    RenderCandidate(i);
                }
            }


            completionOrigin = origin;
            completionSize = size;

            if (Util.IsMouseHoveringRect(origin, origin + size))
            {

                io.WantCaptureMouse = true;

                if (CompletionViewIndex > 4 && CompletionViewIndex < ScriptCompletionItems.Length - 4)
                    CompletionViewIndex -= (int)io.MouseWheel;
                else
                {
                    if (CompletionViewIndex <= 5) CompletionViewIndex = 5;
                    else if (CompletionViewIndex >= ScriptCompletionItems.Length - 4) CompletionViewIndex = ScriptCompletionItems.Length - 5;
                }


                ImGui.SetItemDefaultFocus();
                ImGui.SetKeyboardFocusHere(-1);
            }
        }

        if (ScriptCompletionArguments.Any())
        {
            SelectedCompletionOverload = Math.Clamp(SelectedCompletionOverload, 0, ScriptCompletionArguments.Length - 1);
            ImDrawListPtr drawList = ImGui.GetForegroundDrawList();
            string currentText = (ScriptCompletionArguments.Length > 1 ? $"({SelectedCompletionOverload + 1}/{ScriptCompletionArguments.Length}) " : "") + ScriptCompletionArguments[SelectedCompletionOverload];
            Vector2 origin = new Vector2(textboxMin.X, textboxMax.Y + completionSize.Y + style.ItemSpacing.Y * 2f);
            Vector2 size = new Vector2(ImGui.CalcTextSize(currentText).X + style.ItemSpacing.X * 2f, ImGui.CalcTextSize(currentText).Y + style.ItemSpacing.Y * 2f);

            if (origin.X + size.X > io.DisplaySize.X)
            {
                origin.X -= (origin.X + size.X) - io.DisplaySize.X;
            }
            if (origin.Y + size.Y > io.DisplaySize.Y)
            {
                origin.Y = textboxMin.Y - style.ItemSpacing.Y - size.Y;
            }
            if (FlipCompletion && ScriptCompletionItems.Any())
            {
                origin.Y = textboxMin.Y - style.ItemSpacing.Y * 2f - size.Y - completionSize.Y;
            }

            drawList.AddRectFilled(origin, origin + size, ImGui.GetColorU32(ImGuiCol.WindowBg));

            drawList.AddText(origin + style.ItemSpacing, Color.White.PackedValue, currentText);
        }
    }

    private Task UpdateCompletion(string text, int cursorPosition)
    {
        return Task.Run(
            () =>
            {
                lock (CandidateLock)
                {
                    ScriptCompletionItems = Script.GetCompletionsAsync(text, cursorPosition, CompletionTrigger.CreateInsertionTrigger('a')).Result;
                    ScriptCompletionArguments = Script.GetSymbolArgumentsAsync(text, cursorPosition).Result;
                }
            });
    }

    private void PushEdit(ImGuiInputTextCallbackDataPtr data)
    {
        if (UndoPosition < UndoStack.Count)
        {
            UndoStack.RemoveRange(UndoPosition, UndoStack.Count - 1 - UndoPosition);
        }

        UndoStack.Add(new UndoState(data.GetText(), data.CursorPos));

        UndoPosition++;
    }

    private void Undo(ImGuiInputTextCallbackDataPtr data)
    {
        if (UndoPosition > 1)
        {
            UndoState state = UndoStack[UndoPosition - 2];
            UndoPosition--;

            data.SetText(state.Text);
            data.CursorPos = state.CursorPosition;

            UpdateCompletion(state.Text, data.CursorPos);
        }
    }

    private void Redo(ImGuiInputTextCallbackDataPtr data)
    {
        if (UndoPosition > 1 && UndoPosition < UndoStack.Count)
        {
            UndoState state = UndoStack[UndoPosition];
            UndoPosition++;

            data.SetText(state.Text);
            data.CursorPos = state.CursorPosition;

            UpdateCompletion(state.Text, data.CursorPos);
        }
    }

    private unsafe int TextEditCallback(ImGuiInputTextCallbackDataPtr data)
    {
        switch (data.EventFlag)
        {
            case ImGuiInputTextFlags.CallbackHistory:
                {
                    if ((!ScriptCompletionItems.Any() && ScriptCompletionArguments.Length <= 1) || InputSystem.Ctrl)
                    {
                        int previousHistoryPosition = HistoryPosition;
                        if (data.EventKey == ImGuiKey.UpArrow)
                        {
                            if (HistoryPosition == -1)
                            {
                                HistoryPosition = ConsoleHistory.Count - 1;
                            }
                            else if (HistoryPosition > 0)
                            {
                                HistoryPosition--;
                            }
                        }
                        else if (data.EventKey == ImGuiKey.DownArrow)
                        {
                            if (HistoryPosition != -1)
                            {
                                HistoryPosition++;
                                if (HistoryPosition >= ConsoleHistory.Count)
                                {
                                    HistoryPosition = -1;
                                }
                            }
                        }

                        if (previousHistoryPosition != HistoryPosition)
                        {
                            data.SetText((HistoryPosition >= 0) ? ConsoleHistory[HistoryPosition] : "");
                        }
                    }
                    else
                    {
                        int completionItemScollAmount = 1;

                        if (InputSystem.Shift)
                            completionItemScollAmount = 5;

                        if (ScriptCompletionArguments.Length > 1)
                        {
                            if (data.EventKey == ImGuiKey.UpArrow)
                            {
                                SelectedCompletionOverload += completionItemScollAmount;
                            }
                            else if (data.EventKey == ImGuiKey.DownArrow)
                            {
                                SelectedCompletionOverload -= completionItemScollAmount;
                            }

                            SelectedCompletionOverload %= ScriptCompletionArguments.Length;

                            if (SelectedCompletionOverload < 0)
                            {
                                SelectedCompletionOverload = ScriptCompletionArguments.Length + SelectedCompletionOverload;
                            }
                        }
                        else
                        {
                            if (data.EventKey == ImGuiKey.UpArrow)
                            {
                                SelectedCompletionItem -= FlipCompletion ? -completionItemScollAmount : completionItemScollAmount;
                            }
                            else if (data.EventKey == ImGuiKey.DownArrow)
                            {
                                SelectedCompletionItem += FlipCompletion ? -completionItemScollAmount : completionItemScollAmount;
                            }

                            if (SelectedCompletionItem + 5 < CompletionViewIndex)
                                CompletionViewIndex = SelectedCompletionItem + 5;
                            else if (SelectedCompletionItem - 4 > CompletionViewIndex)
                                CompletionViewIndex = SelectedCompletionItem - 4;
                        }
                    }
                    break;
                }
            case ImGuiInputTextFlags.CallbackCompletion:
                {
                    if (ScriptCompletionItems.Any())
                    {
                        SelectedCompletionItem = Math.Clamp(SelectedCompletionItem, 0, ScriptCompletionItems.Length - 1);

                        string s = data.GetText();

                        int cursorPosition = data.CursorPos;

                        (string newText, int newCursorPos) = Script.ApplyCompletionAsync(s, ScriptCompletionItems[SelectedCompletionItem], cursorPosition, null).Result;

                        data.SetText(newText);

                        data.CursorPos = newCursorPos;
                    }

                    UpdateCompletion(data.GetText(), data.CursorPos);

                    PushEdit(data);
                    break;
                }
            case ImGuiInputTextFlags.CallbackEdit:
                {
                    UpdateCompletion(data.GetText(), data.CursorPos);

                    PushEdit(data);
                    break;
                }
            case ImGuiInputTextFlags.CallbackAlways:
                {
                    if (InputSystem.Ctrl)
                    {
                        if (ImGui.IsKeyPressed(ImGuiKey.Z, true))
                        {
                            Undo(data);
                        }

                        if (ImGui.IsKeyPressed(ImGuiKey.Y, true))
                        {
                            Redo(data);
                        }
                    }

                    if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow) || ImGui.IsKeyPressed(ImGuiKey.RightArrow))
                    {
                        UpdateCompletion(data.GetText(), data.CursorPos);
                    }
                    break;
                }
        }

        ForceConsoleInputFocus = true;

        return 0;
    }

    public class CmdStr
    {
        public string Command = "";

        public List<string> Args = new List<string>();

        public string FullMessage = "";

        public string FullArgs = "";

        public CmdStr(string toParse)
        {
            int firstSplit = toParse.IndexOf(" ");

            if (firstSplit > -1)
            {
                FullArgs = toParse[(firstSplit + 1)..];

                Command = toParse[..firstSplit];

                string s = toParse[firstSplit..];

                Args = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else
            {
                Command = toParse;
            }

            FullMessage = toParse;
        }

        public string FullArgsFrom(int index)
        {
            string s = "";

            for (int i = index; i < Args.Count; i++)
            {
                s += Args[i] + (i + 1 < Args.Count ? " " : "");
            }

            return s;
        }
    }

    public class ConsoleElement
    {
        public string Text;

        public Color TextColor;

        public uint AboveDuplicateCount;

        public ConsoleElement(string text, Color textColor, int aboveDuplicateCount)
        {
            Text = text;

            TextColor = textColor;

            AboveDuplicateCount = (uint)(aboveDuplicateCount);
        }
    }

    public class ConsoleCommand
    {
        public string CommandName;

        public string CommandDescription;

        public Action<CmdStr> CommandAction;

        public ConsoleCommand(string name, Action<CmdStr> function, string description = "No Description Given")
        {
            CommandName = name;

            CommandAction = function;

            CommandDescription = description;
        }
    }

    public struct UndoState
    {
        public string Text;

        public int CursorPosition;

        public UndoState(string text, int cursorPosition)
        {
            CursorPosition = cursorPosition;
            Text = text;
        }
    }
}