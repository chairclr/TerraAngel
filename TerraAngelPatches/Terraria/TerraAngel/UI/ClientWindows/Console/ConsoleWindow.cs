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

namespace TerraAngel.UI.ClientWindows.Console;

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

    private ConsoleScriptWindow ScriptWindow;

    private readonly object ConsoleLock = new object();

    private bool ForceConsoleInputFocus = false;

    private bool ForecConsoleReclaimFocus = false;

    public bool ScrollToBottom = false;

    public ConsoleWindow()
    {
        ScriptWindow = new ConsoleScriptWindow();

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
            if (ImGui.InputText("##consoleInput", ref ConsoleInput, 2048, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackAlways | ImGuiInputTextFlags.CallbackEdit | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.NoUndoRedo, (x) => {  return TextEditCallback(x); }))
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

            minInput = ImGui.GetItemRectMin();
            maxInput = ImGui.GetItemRectMax();
        }

        ImGui.SetItemDefaultFocus();
        if (ForecConsoleReclaimFocus)
        {
            ForecConsoleReclaimFocus = false;
            ImGui.SetKeyboardFocusHere(-1);
        }

        ScriptWindow.Draw(minInput, maxInput);

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

        ScriptWindow.UpdateCompletionItems("", 0);

        ScriptWindow.RunScript(message);
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

            ScriptWindow.UpdateCompletionItems(state.Text, data.CursorPos);
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

            ScriptWindow.UpdateCompletionItems(state.Text, data.CursorPos);
        }
    }

    private unsafe int TextEditCallback(ImGuiInputTextCallbackDataPtr data)
    {
        switch (data.EventFlag)
        {
            case ImGuiInputTextFlags.CallbackHistory:
                {
                    if (!InputSystem.Ctrl)
                    {
                        if (ScriptWindow.ScrollThroughCompletions(data))
                        {
                            break;
                        }
                    }

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
                        PushEdit(data);

                        data.SetText((HistoryPosition >= 0) ? ConsoleHistory[HistoryPosition] : "");

                        ScriptWindow.UpdateCompletionItems(data.GetText(), data.CursorPos);

                    }
                    break;
                }
            case ImGuiInputTextFlags.CallbackCompletion:
                {
                    ScriptWindow.TriggerCompletion(data);

                    PushEdit(data);
                    break;
                }
            case ImGuiInputTextFlags.CallbackEdit:
                {
                    ScriptWindow.UpdateCompletionItems(data.GetText(), data.CursorPos);

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
                        ScriptWindow.UpdateCompletionItems(data.GetText(), data.CursorPos);
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