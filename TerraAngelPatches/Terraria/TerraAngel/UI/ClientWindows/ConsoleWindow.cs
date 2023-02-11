using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Tags;
using Microsoft.Xna.Framework.Input;
using TerraAngel.Scripting;

namespace TerraAngel.UI.ClientWindows;

public class ConsoleWindow : ClientWindow
{
    public override bool DefaultEnabled => true;

    public override bool IsEnabled => ClientConfig.Settings.ShowConsoleWindow;

    public override bool IsToggleable => true;

    public override string Title => "Console";

    public Dictionary<string, ConsoleCommand> ConsoleCommands = new Dictionary<string, ConsoleCommand>();
    public List<ConsoleElement> ConsoleItems = new List<ConsoleElement>();
    public ref List<string> ConsoleHistory => ref ClientConfig.Settings.ConsoleHistory;

    public ref int UndoStackSize => ref ClientConfig.Settings.ConsoleUndoStackSize;
    private List<UndoState> undoStack = new List<UndoState>() { new UndoState(0, "") };
    private int undoStackPointer = 1;


    public CSharpScriptEnvironment Script = new CSharpScriptEnvironment();

    private List<CompletionItem> completionCandidates = new List<CompletionItem>();
    private int currentCompletionCandidate = 0;
    private int completionCenterView = 0;

    private List<string> argumentSymbols = new List<string>();
    private int currentArgumentSymbolOverload = 0;

    private object ConsoleLock = new object();
    private object CandidateLock = new object();
    public bool ScrollToBottom = false;
    private string consoleInput = "";
    private int historyPos = -1;

    private bool consoleFocus = false;
    private bool consoleReclaimFocus = false;

    public ConsoleWindow()
    {
        Script.Init();
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
                ImGui.PushStyleColor(ImGuiCol.Text, item.color);
                string text = "";
                if (item.countAbove > 0) text = $"{item.text} ({item.countAbove})";
                else text = item.text;

                Vector2 textSize = ImGui.CalcTextSize(text, wrapWidth);
                if (ImGui.IsRectVisible(textSize))
                {
                    if (ImGuiUtil.WrappedSelectable($"coni{i}", text, wrapWidth))
                    {
                        ImGui.SetClipboardText(item.text);
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
        consoleFocus = false;
        unsafe
        {
            lock (CandidateLock)
            {
                // wip code for multiline W
                //if (ImGui.InputTextMultiline("##consoleInput", ref consoleInput, 2048, new Vector2(ImGui.GetWindowWidth() - style.ItemSpacing.X * 2f, MathF.Min(ImGui.CalcTextSize(consoleInput + " ").Y, ImGui.CalcTextSize(" ").Y * 6f) + style.ItemSpacing.Y * 2f), ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CtrlEnterForNewLine | ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackAlways | ImGuiInputTextFlags.CallbackEdit | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.NoUndoRedo, (x) => { lock (CandidateLock) { return TextEditCallback(x); } }))
                if (ImGui.InputText("##consoleInput", ref consoleInput, 2048, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackAlways | ImGuiInputTextFlags.CallbackEdit | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.NoUndoRedo, (x) => { lock (CandidateLock) { return TextEditCallback(x); } }))
                {
                    if (consoleInput.Length > 0)
                    {
                        WriteLine(">> " + consoleInput, new Color(0, 255, 0, 255));
                        ExecuteMessage(consoleInput);
                        consoleInput = "";

                        if (ClientConfig.Settings.ConsoleAutoScroll) ScrollToBottom = true;
                    }

                    consoleReclaimFocus = true;
                }
            }
            minInput = ImGui.GetItemRectMin();
            maxInput = ImGui.GetItemRectMax();
        }

        ImGui.SetItemDefaultFocus();
        if (consoleReclaimFocus)
        {
            consoleReclaimFocus = false;
            ImGui.SetKeyboardFocusHere(-1);
        }



        lock (CandidateLock)
        {
            if (consoleFocus)
            {
                if (consoleInput.Trim().Length == 0)
                {
                    argumentSymbols.Clear();
                    completionCandidates.Clear();
                }
                RenderScriptCandidates(io, minInput, maxInput);
            }
        }

        ImGui.End();
        ImGui.PopFont();
    }
    private unsafe int TextEditCallback(ImGuiInputTextCallbackDataPtr data)
    {
        string GetText() => Encoding.UTF8.GetString((byte*)data.Buf, data.BufTextLen);
        void ReplaceText(string t)
        {
            data.DeleteChars(0, data.BufTextLen);
            data.InsertChars(0, t);
        }

        switch (data.EventFlag)
        {
            case ImGuiInputTextFlags.CallbackHistory:
                {
                    if ((!completionCandidates.Any() && argumentSymbols.Count <= 1) || (InputSystem.IsKeyDownRaw(Keys.RightControl) || InputSystem.IsKeyDownRaw(Keys.LeftControl)))
                    {
                        int prev_history_pos = historyPos;
                        if (data.EventKey == ImGuiKey.UpArrow)
                        {
                            if (historyPos == -1)
                                historyPos = ConsoleHistory.Count - 1;
                            else if (historyPos > 0)
                                historyPos--;
                        }
                        else if (data.EventKey == ImGuiKey.DownArrow)
                        {
                            if (historyPos != -1)
                                if (++historyPos >= ConsoleHistory.Count)
                                    historyPos = -1;
                        }

                        if (prev_history_pos != historyPos)
                        {
                            string history_str = (historyPos >= 0) ? ConsoleHistory[historyPos] : "";
                            data.DeleteChars(0, data.BufTextLen);
                            data.InsertChars(0, history_str);
                        }
                    }
                    else
                    {
                        int amount = 1;
                        if (InputSystem.IsKeyDownRaw(Keys.LeftShift) || InputSystem.IsKeyDownRaw(Keys.RightShift))
                            amount = 5;

                        if (argumentSymbols.Count <= 1)
                        {
                            if (data.EventKey == ImGuiKey.UpArrow)
                            {
                                currentCompletionCandidate -= cadidatesFlipped ? -amount : amount;
                            }
                            else if (data.EventKey == ImGuiKey.DownArrow)
                            {
                                currentCompletionCandidate += cadidatesFlipped ? -amount : amount;
                            }

                            if (currentCompletionCandidate + 5 < completionCenterView)
                                completionCenterView = currentCompletionCandidate + 5;
                            else if (currentCompletionCandidate - 4 > completionCenterView)
                                completionCenterView = currentCompletionCandidate - 4;
                        }
                        else
                        {
                            if (data.EventKey == ImGuiKey.UpArrow)
                            {
                                currentArgumentSymbolOverload += amount;
                            }
                            else if (data.EventKey == ImGuiKey.DownArrow)
                            {
                                currentArgumentSymbolOverload -= amount;
                            }

                            currentArgumentSymbolOverload %= argumentSymbols.Count;

                            if (currentArgumentSymbolOverload < 0)
                                currentArgumentSymbolOverload = argumentSymbols.Count + currentArgumentSymbolOverload;
                        }
                    }
                    break;
                }
            case ImGuiInputTextFlags.CallbackCompletion:
                {
                    if (completionCandidates.Count > 0)
                    {
                        if (currentCompletionCandidate >= completionCandidates.Count)
                            currentCompletionCandidate = completionCandidates.Count - 1;

                        string s = GetText();
                        int cursorPosition = data.CursorPos;
                        data.DeleteChars(0, data.BufTextLen);
                        data.InsertChars(0, Script.GetChangedText(s, completionCandidates[currentCompletionCandidate], cursorPosition, out int newCursorPosition));

                        data.CursorPos = newCursorPosition;
                    }

                    TextChanged(GetText(), data.CursorPos);

                    if (undoStackPointer < undoStack.Count)
                    {
                        undoStack.RemoveRange(undoStackPointer, undoStack.Count - 1 - undoStackPointer);
                    }

                    undoStack.Add(new UndoState(data.CursorPos, GetText()));
                    undoStackPointer++;

                    if (undoStack.Count > UndoStackSize)
                    {
                        undoStackPointer--;
                        undoStack.RemoveAt(0);
                    }
                    break;
                }
            case ImGuiInputTextFlags.CallbackEdit:
                {
                    TextChanged(GetText(), data.CursorPos);

                    if (undoStackPointer < undoStack.Count)
                    {
                        undoStack.RemoveRange(undoStackPointer, undoStack.Count - 1 - undoStackPointer);
                    }

                    undoStack.Add(new UndoState(data.CursorPos, GetText()));
                    undoStackPointer++;

                    if (undoStack.Count > UndoStackSize)
                    {
                        undoStackPointer--;
                        undoStack.RemoveAt(0);
                    }
                    break;
                }
            case ImGuiInputTextFlags.CallbackAlways:
                {
                    bool ctrl = InputSystem.IsKeyDownRaw(Keys.RightControl) || InputSystem.IsKeyDownRaw(Keys.LeftControl);

                    if (ctrl)
                    {
                        if (ImGui.IsKeyPressed(ImGuiKey.Z, true))
                        {
                            if (undoStackPointer > 1)
                            {
                                UndoState state = undoStack[undoStackPointer - 2];
                                undoStackPointer--;

                                ReplaceText(state.Text);
                                data.CursorPos = state.CursorPosition;

                                TextChanged(GetText(), data.CursorPos);
                            }
                        }

                        if (ImGui.IsKeyPressed(ImGuiKey.Y, true))
                        {
                            if (undoStackPointer > 1 && undoStackPointer < undoStack.Count)
                            {
                                UndoState state = undoStack[undoStackPointer];
                                undoStackPointer++;

                                ReplaceText(state.Text);
                                data.CursorPos = state.CursorPosition;

                                TextChanged(GetText(), data.CursorPos);
                            }
                        }
                    }

                    if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow) || ImGui.IsKeyPressed(ImGuiKey.RightArrow))
                    {
                        TextChanged(GetText(), data.CursorPos);
                    }

                    if (clickedScriptCandidate)
                    {
                        if (completionCandidates.Count > 0)
                        {
                            if (currentCompletionCandidate >= completionCandidates.Count)
                                currentCompletionCandidate = completionCandidates.Count - 1;

                            string s = GetText();
                            int cursorPosition = data.CursorPos;
                            data.DeleteChars(0, data.BufTextLen);
                            data.InsertChars(0, Script.GetChangedText(s, completionCandidates[currentCompletionCandidate], cursorPosition, out int newCursorPosition));

                            data.CursorPos = newCursorPosition;
                        }

                        if (currentCompletionCandidate + 5 < completionCenterView)
                            completionCenterView = currentCompletionCandidate + 5;
                        else if (currentCompletionCandidate - 4 > completionCenterView)
                            completionCenterView = currentCompletionCandidate - 4;

                        TextChanged(GetText(), data.CursorPos);

                        clickedScriptCandidate = false;
                    }
                    break;
                }
        }

        consoleFocus = true;

        return 0;
    }

    private void ExecuteMessage(string message)
    {
        historyPos = -1;
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
            string realCommand = message.Remove(0, 1);
            if (ConsoleCommands.TryGetValue(realCommand, out ConsoleCommand command))
            {
                command.CommandAction(new CmdStr(realCommand));
                return;
            }
        }

        object? expressionValue = Script.Eval(consoleInput);
        if (expressionValue is not null) WriteLine(Script.FormatObject(expressionValue));
    }
    public void WriteLine(string message, Color color)
    {
        lock (ConsoleLock)
        {
            if (ConsoleItems.Count > 0)
            {
                if (message == ConsoleItems[ConsoleItems.Count - 1].text)
                {
                    if (ConsoleItems[ConsoleItems.Count - 1].countAbove == 0)
                        ConsoleItems[ConsoleItems.Count - 1].countAbove++;
                    ConsoleItems[ConsoleItems.Count - 1].countAbove++;
                }
                else
                    ConsoleItems.Add(new ConsoleElement(message, color, 0));
            }
            else
                ConsoleItems.Add(new ConsoleElement(message, color, 0));
        }
    }
    public void WriteLine(string message)
    {
        WriteLine(message, Color.White);
    }
    public void WriteError(string error)
    {
        WriteLine($"[Error] {error}", Color.Red);
    }
    public void ClearConsole()
    {
        ConsoleItems.Clear();
    }
    public void AddCommand(string name, Action<CmdStr> action, string description = "No Description Given", Func<CmdStr, int, List<string>>? getCandidates = null)
    {
        if (!ConsoleCommands.ContainsKey(name))
            ConsoleCommands.Add(name, new ConsoleCommand(name, action, description, getCandidates));
        else
        {
            ConsoleCommand command = ConsoleCommands[name];
            command.CommandAction = action;
            command.CommandDescription = description;
            command.GetCandidates = getCandidates;
        }
    }

    private bool cadidatesFlipped = false;
    private bool clickedScriptCandidate = false;
    private void RenderScriptCandidates(ImGuiIOPtr io, Vector2 textboxMin, Vector2 textboxMax)
    {
        ImGuiStylePtr style = ImGui.GetStyle();

        currentCompletionCandidate = Utils.Clamp(currentCompletionCandidate, 0, completionCandidates.Count - 1);
        completionCenterView = Utils.Clamp(completionCenterView, 0, completionCandidates.Count - 1);
        Vector2 completionOrigin = Vector2.Zero;
        Vector2 completionSize = Vector2.Zero;

        if (completionCandidates.Any())
        {
            string GetCandidateIcon(int i)
            {
                if (completionCandidates[i].Tags.Length > 0)
                {
                    string tag = completionCandidates[i].Tags[0];
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
                if (completionCandidates[i].Tags.Length > 0)
                {
                    string tag = completionCandidates[i].Tags[0];
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
                return completionCandidates[i].DisplayText;
            }

            ImDrawListPtr drawList = ImGui.GetForegroundDrawList();


            float maxSize = 0f;
            float drawHeight = style.ItemSpacing.Y * 2f;

            int startCandidate = Utils.Clamp(completionCenterView - 5, 0, completionCandidates.Count);
            int endCandidate = Utils.Clamp(startCandidate + 10, 0, completionCandidates.Count);
            if ((endCandidate - startCandidate) < 10)
            {
                startCandidate = Utils.Clamp(endCandidate - 10, 0, completionCandidates.Count);
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
            cadidatesFlipped = false;
            if (origin.Y + size.Y > io.DisplaySize.Y)
            {
                cadidatesFlipped = true;
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
                    if (io.MouseClicked[0])
                    {
                        if (currentCompletionCandidate == i) clickedScriptCandidate = true;
                        currentCompletionCandidate = i;
                    }
                }

                if (i == currentCompletionCandidate)
                {
                    col = Color.White;
                }

                drawList.AddText(origin + new Vector2(style.ItemSpacing.X * 2f + 18f, offset), col.PackedValue, s);

                drawList.AddText(origin + new Vector2(style.ItemSpacing.X, offset), iconColor.PackedValue, GetCandidateIcon(i));
                offset += offsetOffset;
            }
            if (cadidatesFlipped)
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

                if (completionCenterView > 4 && completionCenterView < completionCandidates.Count - 4)
                    completionCenterView -= (int)io.MouseWheel;
                else
                {
                    if (completionCenterView <= 5) completionCenterView = 5;
                    else if (completionCenterView >= completionCandidates.Count - 4) completionCenterView = completionCandidates.Count - 5;
                }


                ImGui.SetItemDefaultFocus();
                ImGui.SetKeyboardFocusHere(-1);
            }
        }

        if (argumentSymbols.Any())
        {
            currentArgumentSymbolOverload = Math.Clamp(currentArgumentSymbolOverload, 0, argumentSymbols.Count - 1);
            ImDrawListPtr drawList = ImGui.GetForegroundDrawList();
            string currentText = (argumentSymbols.Count > 1 ? $"({currentArgumentSymbolOverload + 1}/{argumentSymbols.Count}) " : "") + argumentSymbols[currentArgumentSymbolOverload];
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
            if (cadidatesFlipped && completionCandidates.Any())
            {
                origin.Y = textboxMin.Y - style.ItemSpacing.Y * 2f - size.Y - completionSize.Y;
            }

            drawList.AddRectFilled(origin, origin + size, ImGui.GetColorU32(ImGuiCol.WindowBg));

            drawList.AddText(origin + style.ItemSpacing, Color.White.PackedValue, currentText);
        }
    }

    private Task TextChanged(string text, int cursorPosition)
    {
        return Task.Run(
            () =>
            {
                lock (CandidateLock)
                {
                    Script.SetText(text);
                    completionCandidates = Script.GetCompletionAsync(text, cursorPosition).Result;
                    argumentSymbols = Script.GetArgumentListCompletionSymbolsAsync(text, cursorPosition).Result;
                }
            });
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
        public string text;
        public uint color;
        public uint countAbove;

        public ConsoleElement(string text, Color color, int CountAboue)
        {
            this.text = text;
            this.color = color.PackedValue;
            this.countAbove = (uint)(CountAboue);
        }
    }
    public class ConsoleCommand
    {
        public string CommandName;
        public string CommandDescription;
        public Action<CmdStr> CommandAction;
        public Func<CmdStr, int, List<string>>? GetCandidates;

        public ConsoleCommand(string name, Action<CmdStr> function, string description = "No Description Given", Func<CmdStr, int, List<string>>? getCandidates = null)
        {
            this.CommandName = name;
            this.CommandAction = function;
            this.CommandDescription = description;
            this.GetCandidates = getCandidates;
        }
    }
    public struct UndoState
    {
        public int CursorPosition;
        public string Text;

        public UndoState(int cursorPosition, string text)
        {
            CursorPosition = cursorPosition;
            Text = text;
        }
    }
}

public class ConsoleSetup
{
    public static void SetConsoleInitialCommands(ConsoleWindow console)
    {
        console.AddCommand(
            "clear",
            (x) =>
            {
                console.ClearConsole();
            }, "Clears the console");
        console.AddCommand(
            "help",
            (x) =>
            {
                foreach (ConsoleWindow.ConsoleCommand command in console.ConsoleCommands.Values)
                {
                    console.WriteLine($"{command.CommandName}: {command.CommandDescription}");
                }
            }, "Prints help");
        console.WriteLine("Type #help for a list of commands");
    }
}