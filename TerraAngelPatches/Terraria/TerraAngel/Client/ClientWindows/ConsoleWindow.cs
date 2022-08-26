using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Tags;
using Microsoft.Win32.SafeHandles;
using Microsoft.Xna.Framework.Input;
using TerraAngel.Scripting;

namespace TerraAngel.Client.ClientWindows
{
    public class ConsoleWindow : ClientWindow
    {
        public static readonly int MaxUndoSize = 3000;



        public override bool DefaultEnabled => true;

        public override bool IsEnabled => ClientConfig.Settings.ShowConsoleWindow;

        public override bool IsToggleable => true;

        public override string Title => "Console";

        public Dictionary<string, ConsoleCommand> ConsoleCommands = new Dictionary<string, ConsoleCommand>();
        public List<ConsoleElement> ConsoleItems = new List<ConsoleElement>();
        public ref List<string> ConsoleHistory => ref ClientConfig.Settings.ConsoleHistory;
        private List<UndoState> undoStack = new List<UndoState>() { new UndoState(0, "") };
        private int undoStackPointer = 1;

        public ref bool ScriptMode => ref ClientConfig.Settings.ConsoleInReplMode;

        private object ConsoleLock = new object();
        public bool ScrollToBottom = false;
        private string consoleInput = "";
        private int historyPos = -1;
        private List<string> candidates = new List<string>();

        public CSharpScriptEnvironment Script = new CSharpScriptEnvironment();
        private List<CompletionItem> scriptCandidates = new List<CompletionItem>();
        private int currentCandidate = 0;
        private int centerViewCandidate = 0;
        private bool consoleFocus = false;
        private object CandidateLock = new object();


        public ConsoleWindow()
        {
            Script.Warmup();
        }


        public override void Draw(ImGuiIOPtr io)
        {
            ImGui.PushFont(ClientAssets.GetMonospaceFont(16f));

            ImGuiStylePtr style = ImGui.GetStyle();

            NVector2 windowSize = io.DisplaySize / new NVector2(2.8f, 1.9f);
            ImGui.SetNextWindowPos(new NVector2(io.DisplaySize.X - windowSize.X, io.DisplaySize.Y - windowSize.Y), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.FirstUseEver);

            ImGui.SetNextWindowBgAlpha(0.85f);

            if (!ImGui.Begin("Console"))
            {
                ImGui.End();
                return;
            }

            float footerHeight = style.ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            ImGui.BeginChild("ConsoleScrollingRegion", new NVector2(0, -footerHeight), false, ImGuiWindowFlags.HorizontalScrollbar);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new NVector2(4, 1)); // Tighten spacing
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

                    NVector2 textSize = ImGui.CalcTextSize(text, wrapWidth);
                    if (ImGui.IsRectVisible(textSize))
                    {
                        if (ImGuiUtil.WrappedSelectable(text, wrapWidth))
                        {
                            ImGui.SetClipboardText(item.text);
                        }
                    }
                    else
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textSize.Y + (i + 1 < ConsoleItems.Count ? 1f : 0f));
                    }

                    ImGui.PopStyleColor();
                }
            }
            ImGui.PopStyleVar();
            if (ScrollToBottom || (ClientConfig.Settings.ConsoleAutoScroll && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())))
                ImGui.SetScrollY(ImGui.GetScrollMaxY() * 2);

            ImGui.EndChild();

            ScrollToBottom = false;


            bool reclaimFocus = false;
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X);
            NVector2 minInput;
            NVector2 maxInput;
            consoleFocus = false;
            unsafe
            {
                if (ImGui.InputText("##consoleInput", ref consoleInput, 2048, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackAlways | ImGuiInputTextFlags.CallbackEdit | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.NoUndoRedo, (x) => { lock (CandidateLock) { return TextEditCallback(x); } }))
                {
                    if (consoleInput.Length > 0)
                    {
                        WriteLine(">> " + consoleInput, new Color(0, 255, 0, 255));
                        ExecuteAndParseCommand(consoleInput);
                        consoleInput = "";

                        if (ClientConfig.Settings.ConsoleAutoScroll) ScrollToBottom = true;
                    }

                    reclaimFocus = true;
                }
                minInput = ImGui.GetItemRectMin();
                maxInput = ImGui.GetItemRectMax();
            }

            ImGui.SetItemDefaultFocus();
            if (reclaimFocus)
                ImGui.SetKeyboardFocusHere(-1);



            lock (CandidateLock)
            {
                if (consoleFocus)
                {
                    if (ScriptMode)
                    {
                        if (consoleInput.Trim().Length == 0)
                            scriptCandidates.Clear();
                        RenderScriptCandidates(io, minInput, maxInput);
                    }
                    else
                    {
                        RenderConsoleCandidates(io, minInput, maxInput);
                    }
                }
            }

            ImGui.End();
            ImGui.PopFont();
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

        private void ExecuteAndParseCommand(string message)
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

            if (ScriptMode)
            {
                string trimmed = consoleInput.Trim();
                if (trimmed == "#exit")
                {
                    ScriptMode = false;
                    return;
                }
                else if (trimmed == "#clear")
                {
                    ClearConsole();
                    return;
                }
                object? expressionValue = Script.Eval(consoleInput);
                if (expressionValue is not null) WriteLine(Script.FormatObject(expressionValue));
            }
            else
            {
                CmdStr command = new CmdStr(message);
                if (ConsoleCommands.ContainsKey(command.Command))
                {
                    ConsoleCommand aacmd = ConsoleCommands[command.Command];
                    aacmd?.CommandAction(command);
                }
                else
                {
                    WriteError($"Could not find command {command.Command}");
                }
            }
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
        public void ClearConsole()
        {
            ConsoleItems.Clear();
        }
        public void ResetConsole()
        {
            ConsoleCommands.Clear();
            ConsoleHistory.Clear();
            ConsoleItems.Clear();
        }

        private bool cadidatesFlipped = false;
        private void RenderConsoleCandidates(ImGuiIOPtr io, NVector2 textboxMin, NVector2 textboxMax)
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            currentCandidate = Utils.Clamp(currentCandidate, 0, candidates.Count - 1);
            if (candidates.Any())
            {
                ImDrawListPtr drawList = ImGui.GetForegroundDrawList();
                float maxSize = 0f;
                float drawHeight = style.ItemSpacing.Y * 2f;

                int startCandidate = Utils.Clamp(currentCandidate - 5, 0, candidates.Count);
                int endCandidate = Utils.Clamp(startCandidate + 10, 0, candidates.Count);

                if ((endCandidate - startCandidate) < 10)
                {
                    startCandidate = Utils.Clamp(endCandidate - 10, 0, candidates.Count);
                }

                for (int i = startCandidate; i < endCandidate; i++)
                {
                    string s = candidates[i];
                    NVector2 textSize = ImGui.CalcTextSize(s);
                    if (textSize.X > maxSize)
                        maxSize = textSize.X + style.ItemSpacing.Y;
                    drawHeight += textSize.Y + style.ItemSpacing.Y;
                }

                NVector2 origin = new NVector2(textboxMin.X, textboxMax.Y + style.ItemSpacing.Y);
                NVector2 size = new NVector2(maxSize + style.ItemSpacing.Y * 3f, drawHeight);

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

                if (ImGui.IsMouseHoveringRect(origin, origin + size))
                {
                    io.WantCaptureMouse = true;

                    io.MouseDown[0] = false;
                    io.MouseDown[1] = false;
                    io.MouseDown[2] = false;

                    ImGui.SetItemDefaultFocus();
                    ImGui.SetKeyboardFocusHere(-1);
                }

                float offset = style.ItemSpacing.Y;
                if (cadidatesFlipped)
                {
                    for (int i = endCandidate - 1; i > startCandidate - 1; i--)
                    {
                        string s = candidates[i];
                        if (i != currentCandidate)
                        {
                            drawList.AddText(origin + new NVector2(style.ItemSpacing.X, offset), Color.Gray.PackedValue, s);
                        }
                        else
                        {
                            drawList.AddText(origin + new NVector2(style.ItemSpacing.X, offset), Color.White.PackedValue, s);
                        }
                        offset += ImGui.CalcTextSize(s).Y + style.ItemSpacing.Y;
                    }
                }
                else
                {
                    for (int i = startCandidate; i < endCandidate; i++)
                    {
                        string s = candidates[i];
                        if (i != currentCandidate)
                        {
                            drawList.AddText(origin + new NVector2(style.ItemSpacing.X, offset), Color.Gray.PackedValue, s);
                        }
                        else
                        {
                            drawList.AddText(origin + new NVector2(style.ItemSpacing.X, offset), Color.White.PackedValue, s);
                        }
                        offset += ImGui.CalcTextSize(s).Y + style.ItemSpacing.Y;
                    }
                }
            }

        }

        private bool clickedScriptCandidate = false;
        private void RenderScriptCandidates(ImGuiIOPtr io, NVector2 textboxMin, NVector2 textboxMax)
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            currentCandidate = Utils.Clamp(currentCandidate, 0, scriptCandidates.Count - 1);
            centerViewCandidate = Utils.Clamp(centerViewCandidate, 0, scriptCandidates.Count - 1);
            // wip method signature code -chair
            //if (extraCandidateText != "")
            //{
            //    ImDrawListPtr drawList = ImGui.GetForegroundDrawList();
            //    NVector2 textSize = ImGui.CalcTextSize(extraCandidateText);

            //    NVector2 size = new NVector2(textSize.X + style.ItemSpacing.X * 2f, textSize.Y + style.ItemSpacing.Y * 2f);
            //    NVector2 origin = new NVector2(textboxMin.X, textboxMax.Y + style.ItemSpacing.Y);
            //    if (origin.X + size.X > io.DisplaySize.X)
            //    {
            //        origin.X -= (origin.X + size.X) - io.DisplaySize.X;
            //    }
            //    cadidatesFlipped = false;
            //    if (origin.Y + size.Y > io.DisplaySize.Y)
            //    {
            //        cadidatesFlipped = true;
            //        origin.Y = textboxMin.Y - style.ItemSpacing.Y - size.Y;
            //    }

            //    origin.X -= size.X + style.ItemSpacing.X;
            //    drawList.AddRectFilled(origin, origin + size, ImGui.GetColorU32(ImGuiCol.WindowBg));

            //    drawList.AddText(origin + style.ItemSpacing, Color.White.PackedValue, extraCandidateText);
            //}
            if (scriptCandidates.Any())
            {
                string GetCandidateIcon(int i)
                {
                    if (scriptCandidates[i].Tags.Length > 0)
                    {
                        string tag = scriptCandidates[i].Tags[0];
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
                    if (scriptCandidates[i].Tags.Length > 0)
                    {
                        string tag = scriptCandidates[i].Tags[0];
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
                    return scriptCandidates[i].DisplayText;
                }

                ImDrawListPtr drawList = ImGui.GetForegroundDrawList();
                

                float maxSize = 0f;
                float drawHeight = style.ItemSpacing.Y * 2f;

                int startCandidate = Utils.Clamp(centerViewCandidate - 5, 0, scriptCandidates.Count);
                int endCandidate = Utils.Clamp(startCandidate + 10, 0, scriptCandidates.Count);
                if ((endCandidate - startCandidate) < 10)
                {
                    startCandidate = Utils.Clamp(endCandidate - 10, 0, scriptCandidates.Count);
                }

                for (int i = startCandidate; i < endCandidate; i++)
                {
                    string s = GetCandidateText(i);
                    NVector2 textSize = ImGui.CalcTextSize(s);
                    if (textSize.X > maxSize)
                        maxSize = textSize.X;
                    drawHeight += textSize.Y + style.ItemSpacing.Y;
                }
                maxSize += 18f + style.ItemSpacing.X * 3.75f;
                NVector2 origin = new NVector2(textboxMin.X, textboxMax.Y + style.ItemSpacing.Y);
                NVector2 size = new NVector2(maxSize + style.ItemSpacing.X * 2f, drawHeight);
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
                    if (Util.IsMouseHoveringRect(origin + new NVector2(style.ItemSpacing.X, offset + 1f), origin + new NVector2(maxSize, offset + offsetOffset - 1f)))
                    {
                        col = Color.Gray * 1.45f;
                        if (io.MouseClicked[0])
                        {
                            if (currentCandidate == i) clickedScriptCandidate = true;
                            currentCandidate = i;
                        }
                    }

                    if (i == currentCandidate)
                    {
                        col = Color.White;
                    }

                    drawList.AddText(origin + new NVector2(style.ItemSpacing.X * 2f + 18f, offset), col.PackedValue, s);

                    drawList.AddText(origin + new NVector2(style.ItemSpacing.X, offset), iconColor.PackedValue, GetCandidateIcon(i));
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

                if (Util.IsMouseHoveringRect(origin, origin + size))
                {

                    io.WantCaptureMouse = true;

                    io.MouseDown[0] = false;
                    io.MouseDown[1] = false;
                    io.MouseDown[2] = false;

                    io.MouseClicked[0] = false;
                    io.MouseClicked[1] = false;
                    io.MouseClicked[2] = false;

                    if (centerViewCandidate > 4 && centerViewCandidate < scriptCandidates.Count - 4)
                        centerViewCandidate -= (int)io.MouseWheel;
                    else
                    {
                        if (centerViewCandidate <= 5) centerViewCandidate = 5;
                        else if (centerViewCandidate >= scriptCandidates.Count - 4) centerViewCandidate = scriptCandidates.Count - 5;
                    }


                    ImGui.SetItemDefaultFocus();
                    ImGui.SetKeyboardFocusHere(-1);
                }
            }
        }
        private Task GetScriptCandidates(string text, int cursorPosition)
        {
            return Task.Run(
                async () =>
                {
                    scriptCandidates = await Script.GetCompletionAsync(text, cursorPosition);
                });
        }



        unsafe int TextEditCallback(ImGuiInputTextCallbackDataPtr data)
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
                        if (((!candidates.Any() || ScriptMode) && !scriptCandidates.Any()) || (InputSystem.IsKeyDown(Keys.RightControl) || InputSystem.IsKeyDown(Keys.LeftControl)))
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
                            if (InputSystem.IsKeyDown(Keys.LeftShift) || InputSystem.IsKeyDown(Keys.RightShift))
                                amount = 5;
                            if (data.EventKey == ImGuiKey.UpArrow)
                            {
                                currentCandidate -= cadidatesFlipped ? -amount : amount;
                            }
                            else if (data.EventKey == ImGuiKey.DownArrow)
                            {
                                currentCandidate += cadidatesFlipped ? -amount : amount;
                            }

                            if (currentCandidate + 5 < centerViewCandidate)
                                centerViewCandidate = currentCandidate + 5;
                            else if (currentCandidate - 4 > centerViewCandidate)
                                centerViewCandidate = currentCandidate - 4;
                        }

                        break;
                    }
                case ImGuiInputTextFlags.CallbackCompletion:
                    {
                        if (ScriptMode)
                        {
                            if (scriptCandidates.Count > 0)
                            {
                                if (currentCandidate >= scriptCandidates.Count)
                                    currentCandidate = scriptCandidates.Count - 1;

                                string s = GetText();
                                int cursorPosition = data.CursorPos;
                                data.DeleteChars(0, data.BufTextLen);
                                data.InsertChars(0, Script.GetChangedText(s, scriptCandidates[currentCandidate], cursorPosition, out int newCursorPosition));

                                data.CursorPos = newCursorPosition;
                            }

                            GetScriptCandidates(GetText(), data.CursorPos);
                        }
                        else
                        {
                            if (candidates.Count > 0)
                            {
                                if (currentCandidate >= candidates.Count)
                                    currentCandidate = candidates.Count - 1;

                                data.DeleteChars(0, data.BufTextLen);
                                data.InsertChars(data.CursorPos, candidates[currentCandidate]);
                                data.InsertChars(data.CursorPos, " ");
                            }
                        }

                        if (undoStackPointer < undoStack.Count)
                        {
                            undoStack.RemoveRange(undoStackPointer, undoStack.Count - 1 - undoStackPointer);
                        }

                        undoStack.Add(new UndoState(data.CursorPos, GetText()));
                        undoStackPointer++;

                        if (undoStack.Count > MaxUndoSize)
                        {
                            undoStackPointer--;
                            undoStack.RemoveAt(0);
                        }
                        break;
                    }
                case ImGuiInputTextFlags.CallbackEdit:
                    {
                        if (ScriptMode)
                        {
                            GetScriptCandidates(GetText(), data.CursorPos);
                        }

                        if (undoStackPointer < undoStack.Count)
                        {
                            undoStack.RemoveRange(undoStackPointer, undoStack.Count - 1 - undoStackPointer);
                        }

                        undoStack.Add(new UndoState(data.CursorPos, GetText()));
                        undoStackPointer++;

                        if (undoStack.Count > MaxUndoSize)
                        {
                            undoStackPointer--;
                            undoStack.RemoveAt(0);
                        }
                        break;
                    }
                case ImGuiInputTextFlags.CallbackAlways:
                    {
                        bool ctrl = InputSystem.IsKeyDown(Keys.RightControl) || InputSystem.IsKeyDown(Keys.LeftControl);

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

                                    if (ScriptMode)
                                    {
                                        GetScriptCandidates(GetText(), data.CursorPos);
                                    }
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

                                    if (ScriptMode)
                                    {
                                        GetScriptCandidates(GetText(), data.CursorPos);
                                    }
                                }
                            }
                        }

                        if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow) || ImGui.IsKeyPressed(ImGuiKey.RightArrow))
                        {
                            GetScriptCandidates(GetText(), data.CursorPos);
                        }

                        if (ScriptMode && clickedScriptCandidate)
                        {
                            if (scriptCandidates.Count > 0)
                            {
                                if (currentCandidate >= scriptCandidates.Count)
                                    currentCandidate = scriptCandidates.Count - 1;

                                string s = GetText();
                                int cursorPosition = data.CursorPos;
                                data.DeleteChars(0, data.BufTextLen);
                                data.InsertChars(0, Script.GetChangedText(s, scriptCandidates[currentCandidate], cursorPosition, out int newCursorPosition));

                                data.CursorPos = newCursorPosition;
                            }

                            if (currentCandidate + 5 < centerViewCandidate)
                                centerViewCandidate = currentCandidate + 5;
                            else if (currentCandidate - 4 > centerViewCandidate)
                                centerViewCandidate = currentCandidate - 4;

                            GetScriptCandidates(GetText(), data.CursorPos);

                            clickedScriptCandidate = false;
                        }
                        break;
                    }
            }

            if (!ScriptMode)
            {
                candidates.Clear();
                if ((data.CursorPos <= consoleInput.IndexOf(' ') || consoleInput.IndexOf(' ') == -1))
                {
                    string s = consoleInput.Trim();
                    int firstSpace = s.IndexOf(' ');
                    if (firstSpace > 0)
                        s = s.Substring(0, firstSpace);

                    foreach (ConsoleCommand command in ConsoleCommands.Values)
                    {
                        if (command.CommandName.ToLower().Contains(s))
                        {
                            candidates.Add(command.CommandName);
                        }
                    }
                }
                else
                {
                    CmdStr command = new CmdStr(GetText());
                    if (ConsoleCommands.ContainsKey(command.Command))
                    {
                        ConsoleCommand aacmd = ConsoleCommands[command.Command];

                        candidates = aacmd.GetCandidates?.Invoke(command, data.CursorPos) ?? new List<string>();
                    }
                }
            }
            consoleFocus = true;

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
                    this.FullArgs = toParse.Substring(firstSplit + 1);
                    this.Command = toParse.Substring(0, firstSplit);
                    string s = toParse.Substring(firstSplit);
                    this.Args = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else
                {
                    this.Command = toParse;
                }
                this.FullMessage = toParse;
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

        struct UndoState
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
            console.AddCommand(
                "reset",
                (x) =>
                {
                    console.ResetConsole();
                    console.AddCommand(
                        "reinit",
                        (x) =>
                        {
                            SetConsoleInitialCommands(console);
                        }, "Re-initializes the console");
                }, "Resets the console");
            console.AddCommand(
                "reinit",
                (x) =>
                {
                    SetConsoleInitialCommands(console);
                }, "Re-initializes the console");
            console.AddCommand(
                "echo",
                (x) =>
                {
                    console.WriteLine(x.FullArgs);
                }, "Echos the input back to the console");
            console.AddCommand(
                "e",
                (x) =>
                {
                    console.WriteLine(console.Script.FormatObject(console.Script.Eval(x.FullArgs)));
                }, "Executes arbitrary c# code");
            console.AddCommand(
                "ea",
                (x) =>
                {
                    console.WriteLine(console.Script.FormatObject(console.Script.EvalAsync(x.FullArgs)));
                }, "Executes arbitrary c# code");
            console.AddCommand(
                "repl",
                (x) =>
                {
                    console.ScriptMode = true;
                    if (console.ScriptMode)
                        console.WriteLine("Entering C# REPL mode\nType #exit to exit REPL mode");
                    else
                        console.WriteLine("Exiting C# REPL mode");
                });
        }

        public static bool TryConvertValue(Type targetType, string stringValue, out object? convertedValue)
        {
            if (targetType == typeof(string))
            {
                convertedValue = Convert.ChangeType(stringValue, targetType);
                return true;
            }
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (string.IsNullOrEmpty(stringValue))
                {
                    convertedValue = null;
                    return true;
                }
                targetType = new NullableConverter(targetType).UnderlyingType;
            }

            Type[] argTypes = { typeof(string), targetType.MakeByRefType() };
            MethodInfo? tryParseMethodInfo = targetType.GetMethod("TryParse", argTypes);
            if (tryParseMethodInfo == null)
            {
                convertedValue = null;
                return false;
            }

            object?[] args = { stringValue, null };
            bool successfulParse = (bool)tryParseMethodInfo?.Invoke(null, args);
            if (!successfulParse)
            {
                convertedValue = null;
                return false;
            }

            convertedValue = args[1];
            return true;
        }

    }
}