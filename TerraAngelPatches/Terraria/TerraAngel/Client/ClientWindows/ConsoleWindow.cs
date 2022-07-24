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
        private int currentCandidate = 0;
        private bool consoleFocus = false;


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
            lock (ConsoleLock)
            {
                ImGui.PushTextWrapPos();
                for (int i = 0; i < ConsoleItems.Count; i++)
                {
                    ConsoleElement item = ConsoleItems[i];
                    ImGui.PushStyleColor(ImGuiCol.Text, item.color);
                    if (item.countAbove > 0)
                    {
                        string s = $"{item.text} ({item.countAbove})";
                        ImGui.TextUnformatted(s);
                    }
                    else
                    {
                        ImGui.TextUnformatted(item.text);
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
                if (ImGui.InputText("##consoleInput", ref consoleInput, 2048, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackAlways | ImGuiInputTextFlags.CallbackEdit | ImGuiInputTextFlags.CallbackCompletion, (x) => TextEditCallback(x)))
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


            // 🤓 code ahead
            if (consoleFocus && candidates.Count > 0)
            {
                ImDrawListPtr drawList = ImGui.GetForegroundDrawList();
                float maxSize = 0f;
                float drawHeight = style.ItemSpacing.Y * 2f;
                for (int i = 0; i < candidates.Count; i++)
                {
                    string s = candidates[i];
                    NVector2 textSize = ImGui.CalcTextSize(s);
                    if (textSize.X > maxSize)
                        maxSize = textSize.X + style.ItemSpacing.Y;
                    drawHeight += textSize.Y + style.ItemSpacing.Y;
                }

                NVector2 origin = new NVector2(minInput.X, maxInput.Y + style.ItemSpacing.Y);
                NVector2 size = new NVector2(maxSize + style.ItemSpacing.Y * 3f, drawHeight);

                if (origin.X + size.X > io.DisplaySize.X)
                {
                    origin.X -= (origin.X + size.X) - io.DisplaySize.X;
                }

                if (origin.Y + size.Y > io.DisplaySize.Y)
                {
                    origin.Y -= (origin.Y + size.Y) - io.DisplaySize.Y;
                }

                drawList.AddRectFilled(origin, origin + size, ImGui.GetColorU32(ImGuiCol.WindowBg));
                float offset = style.ItemSpacing.Y;
                for (int i = 0; i < candidates.Count; i++)
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

        unsafe int TextEditCallback(ImGuiInputTextCallbackDataPtr data)
        {
            switch (data.EventFlag)
            {
                case ImGuiInputTextFlags.CallbackHistory:
                    {
                        if (!candidates.Any())
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
                            if (data.EventKey == ImGuiKey.UpArrow)
                            {
                                currentCandidate--;
                                currentCandidate = Utils.Clamp(currentCandidate, 0, candidates.Count - 1);
                            }
                            else if (data.EventKey == ImGuiKey.DownArrow)
                            {
                                currentCandidate++;
                                currentCandidate = Utils.Clamp(currentCandidate, 0, candidates.Count - 1);
                            }
                        }
                        break;
                    }
                case ImGuiInputTextFlags.CallbackCompletion:
                    {
                        if (candidates.Count > 0)
                        {
                            if (currentCandidate >= candidates.Count)
                                currentCandidate = candidates.Count - 1;

                            char* wordEnd = (char*)data.Buf + data.CursorPos;
                            char* wordStart = wordEnd;

                            while (wordStart > (char*)data.Buf)
                            {
                                char c = wordStart[-1];

                                if (c == ' ')
                                {
                                    break;
                                }

                                wordStart--;
                            }

                            data.DeleteChars((int)(wordStart - (char*)data.Buf), (int)(wordEnd - wordStart));
                            data.InsertChars(data.CursorPos, candidates[currentCandidate]);
                            data.InsertChars(data.CursorPos, " ");
                        }
                        break;
                    }
                case ImGuiInputTextFlags.CallbackAlways:
                    {
                        break;
                    }
            }

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
