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

namespace TerraAngel.Client.ClientWindows
{
    public class ConsoleWindow : ClientWindow
    {
        public override bool DefaultEnabled => true;

        public override bool IsToggleable => true;

        public override string Title => "Console";

        private object ConsoleLock = new object();

        internal Dictionary<string, ConsoleCommand> ConsoleCommands = new Dictionary<string, ConsoleCommand>();
        private List<string> ConsoleHistory = new List<string>();
        private List<ConsoleElement> ConsoleItems = new List<ConsoleElement>();

        private string consoleInput = "";

        private bool ScrollToBottom = false;

        private bool AutoScroll = true;

        private int historyPos = -1;

        public override void Draw(ImGuiIOPtr io)
        {
            ImGui.PushFont(ClientAssets.GetMonospaceFont(18));

            NVector2 windowSize = io.DisplaySize / new NVector2(2.8f, 1.9f);
            ImGui.SetNextWindowPos(new NVector2(io.DisplaySize.X - windowSize.X, io.DisplaySize.Y - windowSize.Y), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.FirstUseEver);

            ImGui.SetNextWindowBgAlpha(0.7f);
            //ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0f, 0.2f, 0.45f, 0.9f));
            //ImGui.PushStyleColor(ImGuiCol.FrameBg, new System.Numerics.Vector4(0.1f, 0.1f, 0.25f, 0.9f));

            if (!ImGui.Begin("Console"))
            {
                ImGui.End();
                ImGui.PopStyleColor(2);
                return;
            }


            float footer_height_to_reserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            ImGui.BeginChild("ConsoleScrollingRegion", new System.Numerics.Vector2(0, -footer_height_to_reserve), false, ImGuiWindowFlags.HorizontalScrollbar);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(4, 1)); // Tighten spacing
            lock (ConsoleLock)
            {
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
            }

            if (ScrollToBottom || (AutoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY()))
                ImGui.SetScrollHereY(1.0f);

            ScrollToBottom = false;

            ImGui.PopStyleVar();
            ImGui.EndChild();

            bool reclaim_focus = false;
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X);

            unsafe
            {
                if (ImGui.InputText("##consoleInput", ref consoleInput, 2048, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackAlways, (x) => TextEditCallback(x)))
                {
                    if (consoleInput.Length > 0)
                    {
                        WriteLine(">> " + consoleInput, new Color(0, 255, 0, 255));
                        ExecuteAndParseCommand(consoleInput);
                        consoleInput = "";
                    }
                    reclaim_focus = true;
                }
            }

            ImGui.SetItemDefaultFocus();
            if (reclaim_focus)
                ImGui.SetKeyboardFocusHere(-1);

            ImGui.End();
            //ImGui.PopStyleColor(2);

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
                    break;
                }
            }
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

        }
    }
}
