using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameInput;

using Terraria.UI.Chat;

namespace TerraAngel.Client.ClientWindows;

public class ChatWindow : ClientWindow
{
    public override bool DefaultEnabled => false;

    public override bool IsToggleable => false;

    public override bool IsPartOfGlobalUI => false;

    public override string Title => "Chat";

    public override bool IsEnabled => !Main.gameMenu;

    public override Keys ToggleKey => Keys.Enter;

    public ChatWindow()
    {
        Main.instance.Activated += (o, args) =>
        {
            if (IsChatting)
                ReclaimFocus = true;
        };
    }

    public bool IsChatting = false;
    public bool ScrollToBottom = false;
    public bool ReclaimFocus = false;

    private bool chatLocked = true;
    private bool justOpened = false;
    private bool justClosed = false;
    private bool resetPosition = false;
    private bool autoScrollFix = false;
    private float autoScrollFixMaxY = 0;
    private float autoScrollFixPrevMaxY = 0;
    public string ChatText = "";
    private string textToAppend = "";

    private object ChatLock = new object();

    public List<ChatItem> ChatItems = new List<ChatItem>(ClientConfig.Settings.ChatMessageLimit);
    public List<string> ChatHistory = new List<string>(100);
    private int historyPos = -1;

    public class ChatItem
    {
        public string OriginalText;
        public List<TextSnippet> TextSnippets;
        public uint Color;
        public uint CountAbove;
        public uint TimeMessageHasBeenVisible = 0;

        public ChatItem(string text, List<TextSnippet> snippets, Color color, int CountAboue)
        {
            this.OriginalText = text;
            this.Color = color.PackedValue;
            this.CountAbove = (uint)(CountAboue);
            TextSnippets = snippets;
            TextSnippets.Add(new TextSnippet(""));
        }
    }


    public override void Draw(ImGuiIOPtr io)
    {
        if (IsChatting)
        {
            if (Main.CurrentInputTextTakerOverride != null)
            {
                ClosePlayerChat();
            }

            if (Main.editSign)
            {
                ClosePlayerChat();
            }

            if (PlayerInput.UsingGamepad)
            {
                ClosePlayerChat();
            }

            if (InputSystem.IsKeyDownRaw(Keys.Escape))
            {
                ClosePlayerChat();

                if (ClientConfig.Settings.ChatVanillaInvetoryBehavior)
                {
                    Main.playerInventory = !Main.playerInventory;
                    if (Main.playerInventory)
                    {
                        SoundEngine.PlaySound(SoundID.MenuOpen);
                    }
                }
            }
        }

        ImGui.PushFont(ClientAssets.GetMonospaceFont(22f));

        ImGuiStylePtr style = ImGui.GetStyle();

        RangeAccessor<Vector4> colors = style.Colors;

        Color colorWithAlpha(ImGuiCol col, float a)
        {
            return new Color(colors[(int)col].X, colors[(int)col].Y, colors[(int)col].X, a);
        }

        float transperency = IsChatting ? ClientConfig.Settings.ChatWindowTransperencyActive : ClientConfig.Settings.ChatWindowTransperencyInactive;
        Color bgColor = colorWithAlpha(ImGuiCol.WindowBg, transperency);
        Color titleColorActive = colorWithAlpha(ImGuiCol.TitleBgActive, transperency);
        Color borderColor = colorWithAlpha(ImGuiCol.Border, transperency);
        Color scrollBgColor = colorWithAlpha(ImGuiCol.ScrollbarBg, transperency);
        Color scrollGrabColor = colorWithAlpha(ImGuiCol.ScrollbarGrab, transperency);
        Color scrollGrabActiveColor = colorWithAlpha(ImGuiCol.ScrollbarGrabActive, transperency);
        Color scrollGrabHoveredColor = colorWithAlpha(ImGuiCol.ScrollbarGrabHovered, transperency);

        Vector2 windowSize = new Vector2(io.DisplaySize.X - 690, 240);
        Vector2 windowPosition = new Vector2(88f, io.DisplaySize.Y - windowSize.Y);

        ImGui.SetNextWindowPos(windowPosition, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(windowSize, ImGuiCond.FirstUseEver);

        if (resetPosition)
        {
            resetPosition = false;
            ImGui.SetNextWindowPos(windowPosition);
            ImGui.SetNextWindowSize(windowSize);
        }

        ImGui.PushStyleColor(ImGuiCol.WindowBg, bgColor.PackedValue);
        ImGui.PushStyleColor(ImGuiCol.MenuBarBg, titleColorActive.PackedValue);
        ImGui.PushStyleColor(ImGuiCol.Border, borderColor.PackedValue);

        ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, scrollBgColor.PackedValue);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, scrollGrabColor.PackedValue);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, scrollGrabActiveColor.PackedValue);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, scrollGrabHoveredColor.PackedValue);

        ImGuiWindowFlags flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.MenuBar;
        if (!IsChatting) flags |= ImGuiWindowFlags.NoMouseInputs;
        if (chatLocked) flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;
        ImGui.Begin("ChatWindow", flags);

        if (IsChatting && ImGui.BeginMenuBar())
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
            ImGui.TextUnformatted("Chat");
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 1f));
            if (ImGui.Button($"{(chatLocked ? Icon.Lock : Icon.Unlock)}")) chatLocked = !chatLocked;
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text($"Chat {(chatLocked ? "Locked" : "Unlocked")}");
                ImGui.EndTooltip();
            }

            if (ImGui.Button($"{Icon.ClearAll}")) ChatItems.Clear();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Clear Chat");
                ImGui.EndTooltip();
            }
            if (ImGui.Button($"{Icon.Refresh}")) resetPosition = true;
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Reset Position");
                ImGui.EndTooltip();
            }
            ImGui.PopStyleVar();
            ImGui.EndMenuBar();
        }

        float footerHeight = style.ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();

        if (ImGui.BeginChild("##ChatScrolling", new Vector2(0, -footerHeight), false, IsChatting ? ImGuiWindowFlags.None : (ImGuiWindowFlags.NoInputs)))
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(drawList.GetClipRectMin(), drawList.GetClipRectMax() + new Vector2(0, 5f));
            Vector2 tip = style.ItemSpacing;
            style.ItemSpacing = new Vector2(4f, 1f);
            float wrapWidth = ImGui.GetContentRegionAvail().X;

            lock (ChatLock)
            {
                for (int i = 0; i < ChatItems.Count; i++)
                {
                    ChatItem item = ChatItems[i];

                    if (item.CountAbove > 0) item.TextSnippets[item.TextSnippets.Count - 1].Text = $" ({item.CountAbove})";

                    Vector2 textSize = ImGuiUtil.CalcTextSizeWithTags(item.TextSnippets, wrapWidth);

                    bool showMessageFade = false;
                    if (item.TimeMessageHasBeenVisible < ClientConfig.Settings.framesForMessageToBeVisible + 60)
                    {
                        showMessageFade = true;
                        item.TimeMessageHasBeenVisible++;
                    }

                    if (ImGui.IsRectVisible(textSize) && (IsChatting || showMessageFade))
                    {
                        if (!IsChatting && showMessageFade && item.TimeMessageHasBeenVisible > (ClientConfig.Settings.framesForMessageToBeVisible))
                        {
                            float alpha = 1.0f - ((float)(item.TimeMessageHasBeenVisible - ClientConfig.Settings.framesForMessageToBeVisible) / 60f);
                            if (ImGuiUtil.WrappedSelectableWithTextBorderWithTags($"CHID{i}", item.TextSnippets, wrapWidth, new Color(0f, 0f, 0f), textSize, alpha))
                            {
                                ImGui.SetClipboardText(item.OriginalText);
                            }
                        }
                        else
                        {
                            if (ImGuiUtil.WrappedSelectableWithTextBorderWithTags($"CHID{i}", item.TextSnippets, wrapWidth, Color.Black, textSize))
                            {
                                ImGui.SetClipboardText(item.OriginalText);
                            }
                        }
                    }
                    else
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textSize.Y + (i + 1 < ChatItems.Count ? style.ItemSpacing.Y : 0f));
                    }
                }
            }

            autoScrollFixPrevMaxY = autoScrollFixMaxY;
            autoScrollFixMaxY = ImGui.GetScrollMaxY();
            if (ScrollToBottom || (ClientConfig.Settings.ChatAutoScroll && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY() || (autoScrollFix && autoScrollFixMaxY > autoScrollFixPrevMaxY))) || !IsChatting || justOpened)
            {
                autoScrollFix = false;
                if (ClientConfig.Settings.ChatAutoScroll && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY()))
                    autoScrollFix = true;
                ImGui.SetScrollY(ImGui.GetScrollMaxY());
            }
            else
            {
                autoScrollFix = false;
            }
            style.ItemSpacing = tip;
            ScrollToBottom = false;
            drawList.PopClipRect();
            ImGui.EndChild();
        }

        bool chatBoxFocus = false;
        if (IsChatting)
        {
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X);
            unsafe
            {
                ImGui.InputText("##consoleInput", ref ChatText, 512, ImGuiInputTextFlags.CallbackAlways | ImGuiInputTextFlags.CallbackHistory,
                    (x) =>
                    {
                        ImGuiInputTextCallbackDataPtr data = x;
                        switch (data.EventFlag)
                        {
                            case ImGuiInputTextFlags.CallbackHistory:
                                {
                                    int prev_history_pos = historyPos;
                                    if (data.EventKey == ImGuiKey.UpArrow)
                                    {
                                        if (historyPos == -1)
                                            historyPos = ChatHistory.Count - 1;
                                        else if (historyPos > 0)
                                            historyPos--;
                                    }
                                    else if (data.EventKey == ImGuiKey.DownArrow)
                                    {
                                        if (historyPos != -1)
                                            if (++historyPos >= ChatHistory.Count)
                                                historyPos = -1;
                                    }

                                    if (prev_history_pos != historyPos)
                                    {
                                        string history_str = (historyPos >= 0) ? ChatHistory[historyPos] : "";
                                        data.DeleteChars(0, data.BufTextLen);
                                        data.InsertChars(0, history_str);
                                    }
                                }
                                break;
                            case ImGuiInputTextFlags.CallbackAlways:
                                if (textToAppend.Length > 0)
                                {
                                    lock (textToAppend)
                                    {
                                        data.InsertChars(data.CursorPos, textToAppend);
                                        textToAppend = "";
                                    }
                                }
                                break;
                        }
                        chatBoxFocus = true;
                        return 0;
                    });
            }
            ImGui.PopItemWidth();

            if (justOpened || ReclaimFocus)
            {
                ReclaimFocus = false;
                ImGui.SetItemDefaultFocus();
                ImGui.SetKeyboardFocusHere(-1);
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(7);

        ImGui.PopFont();

        if (IsChatting)
        {
            if (!justOpened && InputSystem.IsKeyPressedRaw(Keys.Enter))
            {
                if (chatBoxFocus)
                {
                    if (!string.IsNullOrEmpty(ChatText))
                    {
                        ChatMessage message = ChatManager.Commands.CreateOutgoingMessage(Util.EscapeString(ChatText));

                        if (Main.netMode == 1)
                        {
                            ChatHelper.SendChatMessageFromClient(message);
                        }
                        else if (Main.netMode == 0)
                        {
                            ChatManager.Commands.ProcessIncomingMessage(message, Main.myPlayer);
                        }

                        historyPos = -1;
                        for (int i = ChatHistory.Count - 1; i >= 0; i--)
                        {
                            if (ChatHistory[i] == ChatText)
                            {
                                ChatHistory.RemoveAt(i);
                                break;
                            }
                        }

                        if (ChatHistory.Count > ClientConfig.Settings.ChatHistoryLimit)
                            ChatHistory.RemoveRange(0, ChatHistory.Count - ClientConfig.Settings.ChatHistoryLimit);

                        ChatHistory.Add(ChatText);
                        ChatText = "";
                        ScrollToBottom = true;
                    }
                    ClosePlayerChat();
                }
                else
                {
                    ReclaimFocus = true;
                }
            }
        }
    }

    public override void Update()
    {
        justOpened = false;
        if (!ImGui.GetIO().WantCaptureKeyboard
            && InputSystem.IsKeyPressedRaw(ToggleKey)
            && !InputSystem.IsKeyDownRaw(Keys.LeftAlt)
            && !InputSystem.IsKeyDownRaw(Keys.RightAlt)
            && Main.hasFocus
            && !Main.editSign
            && !Main.editChest
            && !Main.gameMenu
            && !InputSystem.IsKeyDownRaw(Keys.Escape)
            && Main.CurrentInputTextTakerOverride == null
            && !IsChatting
            && !justClosed)
        {
            OpenPlayerchat();
        }
        justClosed = false;
    }

    public void OpenPlayerchat()
    {
        IsChatting = true;
        justOpened = true;
        SoundEngine.PlaySound(SoundID.MenuOpen);
    }

    public void ClosePlayerChat()
    {
        IsChatting = false;
        justClosed = true;
        SoundEngine.PlaySound(SoundID.MenuClose);

        if (ClientConfig.Settings.ClearChatInputOnClose)
            ChatText = "";
    }

    public void WriteLine(string message, Color color)
    {
        List<TextSnippet> snippets = ChatManager.ParseMessage(message, color);

        lock (ChatLock)
        {
            if (ChatItems.Count > 0)
            {
                ChatItem above = ChatItems[ChatItems.Count - 1];
                if (message == above.OriginalText && above.Color == color.PackedValue)
                {
                    if (above.CountAbove == 0)
                        above.CountAbove++;
                    above.CountAbove++;
                    above.TimeMessageHasBeenVisible = 0;
                }
                else
                {
                    ChatItems.Add(new ChatItem(message, snippets, color, 0));
                }
            }
            else
            {
                ChatItems.Add(new ChatItem(message, snippets, color, 0));
            }

            if (ChatItems.Count > ClientConfig.Settings.ChatMessageLimit)
            {
                ChatItems.RemoveRange(0, ChatItems.Count - ClientConfig.Settings.ChatMessageLimit);
            }
        }
    }

    public void WriteLine(string message)
    {
        WriteLine(message, Color.White);
    }

    public void AddText(string message)
    {
        if (IsChatting)
        {
            justOpened = true;
            lock (textToAppend)
            {
                textToAppend += message;
            }
        }
    }
}
