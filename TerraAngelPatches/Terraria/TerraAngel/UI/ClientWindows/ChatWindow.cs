using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameInput;
using Terraria.UI.Chat;

namespace TerraAngel.UI.ClientWindows;

public class ChatWindow : ClientWindow
{
    public override bool DefaultEnabled => false;

    public override bool IsToggleable => false;

    public override bool IsGlobalToggle => false;

    public override string Title => "Chat";

    public override bool IsEnabled => !Main.gameMenu;

    public override Keys ToggleKey => Keys.Enter;

    public ChatWindow()
    {
        Main.instance.Activated += (o, args) =>
        {
            if (IsChatting)
            {
                ReclaimFocus = true;
            }
        };
    }

    public string ChatText = "";

    public bool IsChatting = false;

    public bool ScrollToBottom = false;

    public bool ReclaimFocus = false;

    private bool IsLocked = true;

    private bool OpenedThisFrame = false;

    private bool ClosedThisFrame = false;

    private bool ResetPosition = false;

    private bool AutoScrollFix = false;

    private float AutoScrollFixMaxY = 0;

    private float AutoScrollFixPrevMaxY = 0;

    private string TextToAppend = "";

    private object ChatLock = new object();

    public List<ChatItem> ChatItems = new List<ChatItem>(ClientConfig.Settings.ChatMessageLimit);

    public List<string> ChatHistory = new List<string>(100);

    private int HistoryPosition = -1;

    public override void Draw(ImGuiIOPtr io)
    {
        if (IsChatting)
        {
            if (Main.CurrentInputTextTakerOverride != null)
            {
                CloseChat();
            }

            if (Main.editSign)
            {
                CloseChat();
            }

            if (PlayerInput.UsingGamepad)
            {
                CloseChat();
            }

            if (InputSystem.IsKeyDownRaw(Keys.Escape))
            {
                CloseChat();

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

        ImGui.PushFont(ClientAssets.GetTerrariaFont(22f));

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

        if (ResetPosition)
        {
            ResetPosition = false;
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
        if (IsLocked) flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;

        ImGui.Begin("ChatWindow", flags);

        if (IsChatting && ImGui.BeginMenuBar())
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
            ImGui.TextUnformatted("Chat");
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 1f));

            if (ImGui.Button($"{(IsLocked ? Icon.Lock : Icon.Unlock)}")) IsLocked = !IsLocked;
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text($"Chat {(IsLocked ? "Locked" : "Unlocked")}");
                ImGui.EndTooltip();
            }

            if (ImGui.Button($"{Icon.ClearAll}")) ChatItems.Clear();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Clear Chat");
                ImGui.EndTooltip();
            }

            if (ImGui.Button($"{Icon.Refresh}")) ResetPosition = true;
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

        if (ImGui.BeginChild("##ChatScrolling", new Vector2(0, -footerHeight), false, IsChatting ? ImGuiWindowFlags.None : ImGuiWindowFlags.NoInputs))
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(drawList.GetClipRectMin(), drawList.GetClipRectMax() + new Vector2(0, 5f));
            Vector2 tip = style.ItemSpacing;
            float wrapWidth = ImGui.GetContentRegionAvail().X;

            lock (ChatLock)
            {
                for (int i = 0; i < ChatItems.Count; i++)
                {
                    ChatItem item = ChatItems[i];

                    if (item.CountAbove > 0)
                    {
                        item.TextSnippets[^1].Text = $" ({item.CountAbove})";
                    }

                    style.ItemSpacing = new Vector2(4f, 1f);

                    if (item.LastWrapWidth != wrapWidth)
                    {
                        item.LastWrapWidth = wrapWidth;
                        item.CachedSize = ImGuiUtil.CalcTextSizeWithTags(item.TextSnippets, wrapWidth);
                    }

                    bool showMessageFade = false;
                    if (item.TimeMessageHasBeenVisible < ClientConfig.Settings.framesForMessageToBeVisible + 60)
                    {
                        showMessageFade = true;
                        item.TimeMessageHasBeenVisible++;
                    }

                    if (ImGui.IsRectVisible(item.CachedSize) && (IsChatting || showMessageFade))
                    {
                        if (!IsChatting && showMessageFade && item.TimeMessageHasBeenVisible > (ClientConfig.Settings.framesForMessageToBeVisible))
                        {
                            float alpha = 1.0f - ((float)(item.TimeMessageHasBeenVisible - ClientConfig.Settings.framesForMessageToBeVisible) / 60f);
                            if (ImGuiUtil.WrappedSelectableWithTextBorderWithTags($"CHID{i}", item.TextSnippets, wrapWidth, new Color(0f, 0f, 0f), item.CachedSize, alpha))
                            {
                                ImGui.SetClipboardText(item.OriginalText);
                            }
                        }
                        else
                        {
                            if (ImGuiUtil.WrappedSelectableWithTextBorderWithTags($"CHID{i}", item.TextSnippets, wrapWidth, Color.Black, item.CachedSize))
                            {
                                ImGui.SetClipboardText(item.OriginalText);
                            }
                        }
                    }
                    else
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + item.CachedSize.Y + (i + 1 < ChatItems.Count ? style.ItemSpacing.Y : 0f));
                        style.ItemSpacing.Y = 0f;
                        ImGui.Dummy(Vector2.Zero);
                    }
                }
            }

            AutoScrollFixPrevMaxY = AutoScrollFixMaxY;
            AutoScrollFixMaxY = ImGui.GetScrollMaxY();
            if (ScrollToBottom || (ClientConfig.Settings.ChatAutoScroll && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY() || (AutoScrollFix && AutoScrollFixMaxY > AutoScrollFixPrevMaxY))) || !IsChatting || OpenedThisFrame)
            {
                AutoScrollFix = false;
                if (ClientConfig.Settings.ChatAutoScroll && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY()))
                    AutoScrollFix = true;
                ImGui.SetScrollY(ImGui.GetScrollMaxY());
            }
            else
            {
                AutoScrollFix = false;
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
                                    int previousHistoryPosition = HistoryPosition;
                                    if (data.EventKey == ImGuiKey.UpArrow)
                                    {
                                        if (HistoryPosition == -1)
                                        {
                                            HistoryPosition = ChatHistory.Count - 1;
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
                                            if (HistoryPosition >= ChatHistory.Count)
                                            {
                                                HistoryPosition = -1;
                                            }
                                        }
                                    }

                                    if (previousHistoryPosition != HistoryPosition)
                                    {
                                        data.SetText((HistoryPosition >= 0) ? ChatHistory[HistoryPosition] : "");
                                    }
                                }
                                break;
                            case ImGuiInputTextFlags.CallbackAlways:
                                if (TextToAppend.Length > 0)
                                {
                                    lock (TextToAppend)
                                    {
                                        data.InsertChars(data.CursorPos, TextToAppend);
                                        TextToAppend = "";
                                    }
                                }
                                break;
                        }
                        chatBoxFocus = true;
                        return 0;
                    });
            }
            ImGui.PopItemWidth();

            if (OpenedThisFrame || ReclaimFocus)
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
            if (!OpenedThisFrame && InputSystem.IsKeyPressedRaw(Keys.Enter))
            {
                if (chatBoxFocus)
                {
                    Submit();
                    CloseChat();
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
        OpenedThisFrame = false;
        ClosedThisFrame = false;
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
         && !ClosedThisFrame)
        {
            OpenChat();
        }
    }

    public void OpenChat()
    {
        IsChatting = true;
        OpenedThisFrame = true;
        SoundEngine.PlaySound(SoundID.MenuOpen);
    }

    public void CloseChat()
    {
        IsChatting = false;
        ClosedThisFrame = true;
        SoundEngine.PlaySound(SoundID.MenuClose);

        if (ClientConfig.Settings.ClearChatInputOnClose)
        {
            ChatText = "";
        }
    }

    public void Submit()
    {
        if (string.IsNullOrEmpty(ChatText))
        {
            return;
        }

        ChatMessage message = ChatManager.Commands.CreateOutgoingMessage(StringExtensions.EscapeString(ChatText));

        if (Main.netMode == 1)
        {
            ChatHelper.SendChatMessageFromClient(message);
        }
        else if (Main.netMode == 0)
        {
            ChatManager.Commands.ProcessIncomingMessage(message, Main.myPlayer);
        }

        HistoryPosition = -1;
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

    public void WriteLine(string message)
    {
        WriteLine(message, Color.White);
    }

    public void WriteLine(string message, Color color)
    {
        List<TextSnippet> snippets = ChatManager.ParseMessage(message, color);

        lock (ChatLock)
        {
            if (ChatItems.Count > 0)
            {
                ChatItem above = ChatItems[^1];
                if (message == above.OriginalText && above.Color == color)
                {
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

    public void AppendText(string message)
    {
        if (IsChatting)
        {
            OpenedThisFrame = true;
            lock (TextToAppend)
            {
                TextToAppend += message;
            }
        }
    }

    public class ChatItem
    {
        public string OriginalText;

        public List<TextSnippet> TextSnippets;

        public Color Color;

        public uint CountAbove;

        public uint TimeMessageHasBeenVisible = 0;

        public Vector2 CachedSize = Vector2.Zero;

        public float LastWrapWidth = float.MinValue;

        public ChatItem(string text, List<TextSnippet> snippets, Color color, int CountAboue)
        {
            OriginalText = text;

            Color = color;

            CountAbove = (uint)(CountAboue);

            TextSnippets = snippets;

            TextSnippets.Add(new TextSnippet(""));
        }
    }
}
