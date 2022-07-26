using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using NVector2 = System.Numerics.Vector2;
using TerraAngel.Utility;
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
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Tags;

namespace TerraAngel.Client.ClientWindows
{
    public class ConsoleWindow : ClientWindow
    {
        public override bool DefaultEnabled => true;

        public override bool IsEnabled => ClientLoader.Config.ShowConsoleWindow;

        public override bool IsToggleable => true;

        public override string Title => "Console";

        public Dictionary<string, ConsoleCommand> ConsoleCommands = new Dictionary<string, ConsoleCommand>();
        public List<string> ConsoleHistory = new List<string>();
        public List<ConsoleElement> ConsoleItems = new List<ConsoleElement>();

        public bool REPLMode = false;

        private object ConsoleLock = new object();
        private string consoleInput = "";
        private bool ScrollToBottom = false;
        private bool AutoScroll = true;
        private int historyPos = -1;
        private List<string> candidates = new List<string>();
        private List<CompletionItem> replCandidates = new List<CompletionItem>();
        private int currentCandidate = 0;
        private bool consoleFocus = false;
        private object CandidateLock = new object();

        public ConsoleWindow()
        {
            CSharpREPL.Warmup();
        }


        public override void Draw(ImGuiIOPtr io)
        {
            ImGui.PushFont(ClientAssets.GetMonospaceFont(18));

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
            float wrapPos = ImGui.GetContentRegionAvail().X;
            lock (ConsoleLock)
            {
                ImGui.PushTextWrapPos(wrapPos);
                for (int i = 0; i < ConsoleItems.Count; i++)
                {
                    ConsoleElement item = ConsoleItems[i];
                    ImGui.PushStyleColor(ImGuiCol.Text, item.color);
                    string text = "";
                    if (item.countAbove > 0) text = $"{item.text} ({item.countAbove})";
                    else text = item.text;

                    NVector2 textSize = ImGui.CalcTextSize(text, wrapPos);
                    if (ImGui.IsRectVisible(textSize))
                    {
                        if (ImGui.Selectable(text))
                        {
                            ImGui.SetClipboardText(item.text);
                        }
                    }
                    else
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textSize.Y + 1f);
                    }

                    ImGui.PopStyleColor();
                }
                ImGui.PopTextWrapPos(); 
            }

            if (ScrollToBottom || (AutoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY()))
                ImGui.SetScrollHereY(1.0f);

            ScrollToBottom = false;

            ImGui.PopStyleVar();
            ImGui.EndChild();

            bool reclaim_focus = false;
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X);
            NVector2 minInput;
            NVector2 maxInput;
            consoleFocus = false;
            unsafe
            {
                if (ImGui.InputText("##consoleInput", ref consoleInput, 2048, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackAlways | ImGuiInputTextFlags.CallbackEdit | ImGuiInputTextFlags.CallbackCompletion, (x) => { lock (CandidateLock) { return TextEditCallback(x); } }))
                {
                    if (consoleInput.Length > 0)
                    {
                        WriteLine(">> " + consoleInput, new Color(0, 255, 0, 255));
                        ExecuteAndParseCommand(consoleInput);
                        consoleInput = "";
                    }

                    reclaim_focus = true;
                }
                 minInput = ImGui.GetItemRectMin();
                 maxInput = ImGui.GetItemRectMax();
            }

            ImGui.SetItemDefaultFocus();
            if (reclaim_focus)
                ImGui.SetKeyboardFocusHere(-1);

            ImGui.End();


            lock (CandidateLock)
            {
                // 🤓 code ahead
                if (consoleFocus)
                {
                    if (REPLMode)
                    {
                        if (consoleInput.Trim().Length == 0)
                            replCandidates.Clear();
                        RenderREPLCandidates(io, minInput, maxInput);
                    }
                    else
                    {
                        RenderConsoleCandidates(io, minInput, maxInput);
                    }
                }
            }

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
            ConsoleHistory.Add(message);

            if (REPLMode)
            {
                string trimmed = consoleInput.Trim();
                if (trimmed == "#exit")
                {
                    REPLMode = false;
                    return;
                }
                else if (trimmed == "#clear")
                {
                    ClearConsole();
                    return;
                }
                CSharpREPL.Execute(consoleInput);
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
        public void AddCommand(string name, Action<CmdStr> action, string description = "No Description Given")
        {
            if (!ConsoleCommands.ContainsKey(name))
                ConsoleCommands.Add(name, new ConsoleCommand(name, action, description));
            else
            {
                ConsoleCommand command = ConsoleCommands[name];
                command.CommandAction = action;
                command.CommandDescription = description;
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
        private void RenderREPLCandidates(ImGuiIOPtr io, NVector2 textboxMin, NVector2 textboxMax)
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            currentCandidate = Utils.Clamp(currentCandidate, 0, replCandidates.Count - 1);
            if (replCandidates.Any())
            {
                string GetCandidateIcon(int i)
                {
                    if (replCandidates[i].Tags.Length > 0)
                    {
                        string tag = replCandidates[i].Tags[0];
                        switch (tag)
                        {
                            case WellKnownTags.Field:
                                return ClientAssets.IconFont.SymbolField;
                            case WellKnownTags.Method:
                            case WellKnownTags.ExtensionMethod:
                                return ClientAssets.IconFont.SymbolMethod;
                            case WellKnownTags.Namespace:
                                return ClientAssets.IconFont.SymbolNamespace;
                            case WellKnownTags.Class:
                                return ClientAssets.IconFont.SymbolClass;
                            case WellKnownTags.Structure:
                                return ClientAssets.IconFont.SymbolStructure;
                            case WellKnownTags.Enum:
                                return ClientAssets.IconFont.SymbolEnum;
                            case WellKnownTags.Interface:
                                return ClientAssets.IconFont.SymbolInterface;
                            case WellKnownTags.Event:
                                return ClientAssets.IconFont.SymbolEvent;
                            case WellKnownTags.Property:
                                return ClientAssets.IconFont.SymbolProperty;
                            case WellKnownTags.Constant:
                                return ClientAssets.IconFont.SymbolConstant;
                            case WellKnownTags.Delegate:
                                return ClientAssets.IconFont.TypeHierarchySub;
                            case WellKnownTags.EnumMember:
                                return ClientAssets.IconFont.SymbolEnumMember;
                            case WellKnownTags.Keyword:
                                return ClientAssets.IconFont.SymbolKeyword;

                        }
                    }
                    return ClientAssets.IconFont.SymbolMisc;
                }
                Color GetIconColor(int i)
                {
                    if (replCandidates[i].Tags.Length > 0)
                    {
                        string tag = replCandidates[i].Tags[0];
                        switch (tag)
                        {
                            case WellKnownTags.Field:
                            case WellKnownTags.Interface:
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
                    return replCandidates[i].DisplayText;
                }


                ImDrawListPtr drawList = ImGui.GetForegroundDrawList();
                float maxSize = 0f;
                float drawHeight = style.ItemSpacing.Y * 2f;

                int startCandidate = Utils.Clamp(currentCandidate - 5, 0, replCandidates.Count);
                int endCandidate = Utils.Clamp(startCandidate + 10, 0, replCandidates.Count);
                if ((endCandidate - startCandidate) < 10)
                {
                    startCandidate = Utils.Clamp(endCandidate - 10, 0, replCandidates.Count);
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
                float offset = style.ItemSpacing.Y;



                void RenderCandidate(int i)
                {
                    string s = GetCandidateText(i);
                    Color iconColor = GetIconColor(i);
                    Color col = Color.Gray;

                    if (i == currentCandidate)
                    {
                        col = Color.White;
                    }

                    drawList.AddText(origin + new NVector2(style.ItemSpacing.X * 2f + 18f, offset), col.PackedValue, s);

                    drawList.AddText(origin + new NVector2(style.ItemSpacing.X, offset + 4f), iconColor.PackedValue, GetCandidateIcon(i));

                    offset += ImGui.CalcTextSize(s).Y + style.ItemSpacing.Y;
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
            }
        }

        unsafe int TextEditCallback(ImGuiInputTextCallbackDataPtr data)
        {
            switch (data.EventFlag)
            {
                case ImGuiInputTextFlags.CallbackHistory:
                    {
                        if (((!candidates.Any() || REPLMode) && !replCandidates.Any()) || (Input.InputSystem.IsKeyDown(Keys.RightControl) || Input.InputSystem.IsKeyDown(Keys.LeftControl)))
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
                            if (Input.InputSystem.IsKeyDown(Keys.LeftShift) || Input.InputSystem.IsKeyDown(Keys.RightShift))
                                amount = 5;
                            if (data.EventKey == ImGuiKey.UpArrow)
                            {
                                currentCandidate -= cadidatesFlipped ? -amount : amount;
                            }
                            else if (data.EventKey == ImGuiKey.DownArrow)
                            {
                                currentCandidate += cadidatesFlipped ? -amount : amount;
                            }
                        }
                        break;
                    }
                case ImGuiInputTextFlags.CallbackCompletion:
                    {
                        if (REPLMode)
                        {
                            if (replCandidates.Count > 0)
                            {
                                if (currentCandidate >= replCandidates.Count)
                                    currentCandidate = replCandidates.Count - 1;

                                //TextSpan s = replCandidates[currentCandidate].Span;
                                //
                                //data.DeleteChars(s.Start, s.End - s.Start);
                                //data.InsertChars(s.Start, replCandidates[currentCandidate].DisplayText);

                                string s = Encoding.UTF8.GetString((byte*)data.Buf, data.BufTextLen);
                                data.DeleteChars(0, data.BufTextLen);
                                data.InsertChars(0, CSharpREPL.GetChangedText(s, replCandidates[currentCandidate]));
                                
                            }
                            CSharpREPL.GetCompletionAsync(Encoding.UTF8.GetString((byte*)data.Buf, data.BufTextLen), data.CursorPos, (x) => replCandidates = x);
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

                        
                        break;
                    }
                case ImGuiInputTextFlags.CallbackEdit:
                    {
                        if (REPLMode)
                        {
                            CSharpREPL.GetCompletionAsync(Encoding.UTF8.GetString((byte*)data.Buf, data.BufTextLen), data.CursorPos, (x) => replCandidates = x);
                        }
                        break;
                    }
                case ImGuiInputTextFlags.CallbackAlways:
                    {
                        break;
                    }
            }

            if (!REPLMode)
            {
                candidates.Clear();
                if ((data.CursorPos <= consoleInput.IndexOf(' ') || consoleInput.IndexOf(' ') == -1) && !REPLMode)
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
            }
            else
            {
                if (data.EventKey == ImGuiKey.Enter)
                    CSharpREPL.GetCompletionAsync(Encoding.UTF8.GetString((byte*)data.Buf, data.BufTextLen), data.CursorPos, (x) => replCandidates = x);
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

            public ConsoleCommand(string name, Action<CmdStr> function, string description = "No Description Given")
            {
                this.CommandName = name;
                this.CommandAction = function;
                this.CommandDescription = description;
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
                    CSharpREPL.Execute(x.FullArgs);
                }, "Executes arbitrary c# code");
            console.AddCommand(
                "ea",
                (x) =>
                {
                    CSharpREPL.ExecuteAsync(x.FullArgs);
                }, "Executes arbitrary c# code");
            console.AddCommand(
                "repl",
                (x) =>
                {
                    console.REPLMode = true;
                    if (console.REPLMode)
                        console.WriteLine("Entering C# REPL mode\nType #exit to exit REPL mode");
                    else
                        console.WriteLine("Exiting C# REPL mode");
                });
        }
    }
}
