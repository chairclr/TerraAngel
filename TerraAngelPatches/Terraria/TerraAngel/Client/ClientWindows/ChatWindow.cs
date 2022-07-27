using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TerraAngel.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameInput;
using Terraria.ID;
using NVector2 = System.Numerics.Vector2;
using Terraria.UI.Chat;

namespace TerraAngel.Client.ClientWindows
{
    public class ChatWindow : ClientWindow
    {
        public override bool DefaultEnabled => false;

        public override bool IsToggleable => false;

        public override bool IsPartOfGlobalUI => false;

        public override string Title => "Chat";

        public override bool IsEnabled => !Main.gameMenu;

        public override Keys ToggleKey => Keys.Enter;

        public bool IsChatting = false;
        public bool ScrollToBottom = false;

        private bool chatLocked = true;
        private bool justOpened = false;
        private bool justClosed = false;

        public string ChatText = "";

        private object ChatLock = new object();

        public List<ChatItem> ChatItems = new List<ChatItem>();

        public class ChatItem
        {
            public string Text;
            public uint Color;
            public uint CountAbove;
            public uint TimeMessageHasBeenVisible = 0;

            public ChatItem(string text, Color color, int CountAboue)
            {
                this.Text = text;
                this.Color = color.PackedValue;
                this.CountAbove = (uint)(CountAboue);
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

                if (Input.InputSystem.IsKeyDown(Keys.Escape))
                {
                    ClosePlayerChat();
                }
            }

            ImGui.PushFont(ClientAssets.GetTerrariaFont(20f));

            ImGuiStylePtr style = ImGui.GetStyle();

            RangeAccessor<System.Numerics.Vector4> colors = style.Colors;

            Color colorWithAlpha(ImGuiCol col, float a)
            {
                return new Color(colors[(int)col].X, colors[(int)col].Y, colors[(int)col].X, a);
            }

            float transperency = IsChatting ? ClientLoader.Config.ChatWindowTransperencyActive : ClientLoader.Config.ChatWindowTransperencyInactive;
            Color bgColor = colorWithAlpha(ImGuiCol.WindowBg, transperency);
            Color titleColorActive = colorWithAlpha(ImGuiCol.TitleBgActive, transperency);
            Color borderColor = colorWithAlpha(ImGuiCol.Border, transperency);
            Color scrollBgColor = colorWithAlpha(ImGuiCol.ScrollbarBg, transperency);
            Color scrollGrabColor = colorWithAlpha(ImGuiCol.ScrollbarGrab, transperency);
            Color scrollGrabActiveColor = colorWithAlpha(ImGuiCol.ScrollbarGrabActive, transperency);
            Color scrollGrabHoveredColor = colorWithAlpha(ImGuiCol.ScrollbarGrabHovered, transperency);

            NVector2 windowSize = new NVector2(io.DisplaySize.X - 690, 240);
            NVector2 windowPosition = new NVector2(88f, io.DisplaySize.Y - windowSize.Y);

            ImGui.SetNextWindowPos(windowPosition, ImGuiCond.Appearing);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Appearing);

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
                ImGui.TextUnformatted("Chat");
                if (ImGui.Button($"{(chatLocked ? ClientAssets.IconFont.Lock : ClientAssets.IconFont.Unlock)}")) chatLocked = !chatLocked;
                ImGui.EndMenuBar();
            }

            float footerHeight = style.ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();

            if (ImGui.BeginChild("##ChatScrolling", new NVector2(0, -footerHeight), false, IsChatting ? ImGuiWindowFlags.None : (ImGuiWindowFlags.NoInputs)))
            {
                NVector2 tip = style.ItemSpacing;
                style.ItemSpacing = new NVector2(4f, 1f);
                float wrapWidth = ImGui.GetContentRegionAvail().X;

                lock (ChatLock)
                {
                    for (int i = 0; i < ChatItems.Count; i++)
                    {
                        ChatItem item = ChatItems[i];

                        string text = "";
                        if (item.CountAbove > 0) text = $"{item.Text} ({item.CountAbove})";
                        else text = item.Text;

                        NVector2 textSize = ImGui.CalcTextSize(text, wrapWidth);

                        bool visibleAtAll = false;
                        if (item.TimeMessageHasBeenVisible < ClientLoader.Config.framesForMessageToBeVisible + 60)
                        {
                            visibleAtAll = true;
                            item.TimeMessageHasBeenVisible++;
                        }

                        if (ImGui.IsRectVisible(textSize) && (IsChatting || visibleAtAll))
                        {
                            if (!IsChatting && visibleAtAll && item.TimeMessageHasBeenVisible > (ClientLoader.Config.framesForMessageToBeVisible))
                            {
                                float t = 1.0f - ((float)(item.TimeMessageHasBeenVisible - ClientLoader.Config.framesForMessageToBeVisible) / 60f);
                                Color c = new Color();
                                c.PackedValue = item.Color;
                                c.A = (byte)(255f * t);
                                ImGui.PushStyleColor(ImGuiCol.Text, c.PackedValue);
                                if (ImGuiUtil.WrappedSelectableWithTextBorder(text, wrapWidth, new Color(0f, 0f, 0f, t)))
                                {
                                    ImGui.SetClipboardText(item.Text);
                                }
                                ImGui.PopStyleColor();
                            }
                            else
                            {
                                ImGui.PushStyleColor(ImGuiCol.Text, item.Color);
                                if (ImGuiUtil.WrappedSelectableWithTextBorder(text, wrapWidth, Color.Black))
                                {
                                    ImGui.SetClipboardText(item.Text);
                                }
                                ImGui.PopStyleColor();
                            }
                        }
                        else
                        {
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textSize.Y + style.ItemSpacing.Y * 2f);
                        }
                    }
                }


                style.ItemSpacing = tip;
                if (ScrollToBottom || (ClientLoader.Config.ConsoleAutoScroll && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())) || !IsChatting || justOpened)
                    ImGui.SetScrollY(ImGui.GetScrollMaxY());
                ScrollToBottom = false;
                ImGui.EndChild();
            }

            bool chatBoxFocus = false;
            if (IsChatting)
            {
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X);
                unsafe { ImGui.InputText("##consoleInput", ref ChatText, 512, ImGuiInputTextFlags.CallbackAlways, (x) => { chatBoxFocus = true; return 0; }); }
                ImGui.PopItemWidth();

                if (justOpened)
                {
                    ImGui.SetItemDefaultFocus();
                    ImGui.SetKeyboardFocusHere(-1);
                }
            }

            ImGui.End();
            ImGui.PopStyleColor(7);

            ImGui.PopFont();

            if (IsChatting)
            {
                if (!justOpened && chatBoxFocus && Input.InputSystem.IsKeyPressed(Keys.Enter))
                {
                    ClosePlayerChat();
                    if (!string.IsNullOrEmpty(ChatText))
                    {
                        ChatHelper.SendChatMessageFromClient(new ChatMessage(ChatText));
                        ChatText = "";
                        ScrollToBottom = true;
                    }
                }
            }
        }

        public override void Update()
        {
            justOpened = false;
            if (!ImGui.GetIO().WantCaptureKeyboard 
                && Input.InputSystem.IsKeyPressed(ToggleKey) 
                && !Input.InputSystem.IsKeyDown(Keys.LeftAlt) 
                && !Input.InputSystem.IsKeyDown(Keys.RightAlt) 
                && Main.hasFocus
                && !Main.editSign 
                && !Main.editChest 
                && !Main.gameMenu 
                && !Input.InputSystem.IsKeyDown(Keys.Escape)
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
        }

        public string ParseOutSomeTags(string s, Color color)
        {
            List<TextSnippet> snippets = ChatManager.ParseMessage(s, color);

            string output = "";
            for (int i = 0; i < snippets.Count; i++)
            {
                output += snippets[i].Text;
            }

            return output;
        }

        public void WriteLine(string message, Color color)
        {
            // i WILL be adding the tags back into the game at SOME POINT but today is not that day
            message = ParseOutSomeTags(message, color);


            lock (ChatLock)
            {
                if (ChatItems.Count > 0)
                {
                    ChatItem above = ChatItems[ChatItems.Count - 1];
                    if (message == above.Text && above.Color == color.PackedValue)
                    {
                        if (above.CountAbove == 0)
                            above.CountAbove++;
                        above.CountAbove++;
                        above.TimeMessageHasBeenVisible = 0;
                    }
                    else
                    {
                        ChatItems.Add(new ChatItem(message, color, 0));
                    }
                }
                else
                {
                    ChatItems.Add(new ChatItem(message, color, 0));
                }
            }
        }

        public void WriteLine(string message)
        {
            WriteLine(message, Color.White);
        }

    }
}
