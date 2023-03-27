namespace TerraAngel.UI.ClientWindows;

public class QuickItemBrowserWindow : ClientWindow
{
    public bool ShowQuickItemBrowser = false;

    public bool JustOpenedQuickBrowser = false;

    public string QuickItemSearch = "";

    public int QuickSelectedIndex = 0;

    public int QuickSelectedItem = 1;

    public override void Draw(ImGuiIOPtr io)
    {
        ImGuiStylePtr style = ImGui.GetStyle();

        JustOpenedQuickBrowser = false;

        if (!io.WantCaptureKeyboard)
        {
            if (InputSystem.Ctrl && InputSystem.IsKeyPressed(ClientConfig.Settings.OpenFastItemBrowser))
            {
                ShowQuickItemBrowser = !ShowQuickItemBrowser;

                if (ShowQuickItemBrowser)
                {
                    JustOpenedQuickBrowser = true;
                }
            }
        }

        if (ShowQuickItemBrowser)
        {
            ImGui.PushFont(ClientAssets.GetMonospaceFont(18f));

            if (JustOpenedQuickBrowser)
            {
                QuickItemSearch = "";
            }

            ImGui.SetNextWindowSizeConstraints(new Vector2(350f, 250f), new Vector2(float.MaxValue, 500f));

            ImGui.Begin("Items", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove);

            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);

            string lastQuickSearch = QuickItemSearch;

            bool hitEnter = false;

            if (ImGui.InputText("###quick-search", ref QuickItemSearch, 256, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                ShowQuickItemBrowser = false;
                hitEnter = true;
            }

            bool textChanged = lastQuickSearch != QuickItemSearch;

            if (JustOpenedQuickBrowser)
            {
                ImGui.SetItemDefaultFocus();
                ImGui.SetKeyboardFocusHere(-1);
            }

            string lowerSearch = QuickItemSearch.ToLower();

            if (ImGui.BeginChild("QuickItemBrowserScrolling"))
            {
                DrawItemList(style, textChanged, lowerSearch);
            }

            float windowSize = ImGui.GetWindowSize().X;

            ImGui.End();

            ImGui.SetWindowPos("Items", new Vector2(io.DisplaySize.X - windowSize - style.ItemSpacing.X, 0f));

            ImGui.PopFont();

            if (hitEnter)
            {
                if (QuickSelectedIndex > -1)
                {
                    if (InputSystem.Ctrl)
                    {
                        ItemSpawner.SpawnItemInSelected(QuickSelectedItem, 9999, ToolManager.GetTool<ItemBrowserTool>().SyncWithServer);
                    }
                    else
                    {
                        ItemSpawner.SpawnItemInMouse(QuickSelectedItem, 9999, ToolManager.GetTool<ItemBrowserTool>().SyncWithServer);
                    }
                }
            }
        }
    }

    private void DrawItemList(ImGuiStylePtr style, bool textChanged, string lowerSearch)
    {
        if (QuickItemSearch.Length > 0)
        {
            if (textChanged)
            {
                bool foundItem = false;
                int totalFoundIndex = 0;
                for (int i = 1; i < ItemID.Count; i++)
                {
                    if (Lang.GetItemName(i).Value.ToLower().Contains(lowerSearch))
                    {
                        if (i == QuickSelectedItem)
                        {
                            QuickSelectedIndex = totalFoundIndex;
                            foundItem = true;
                            totalFoundIndex++;
                            break;
                        }

                        totalFoundIndex++;
                    }

                }

                if (!foundItem)
                {
                    QuickSelectedIndex = 0;
                }
                if (totalFoundIndex == 0)
                {
                    QuickSelectedIndex = -1;
                }
            }

            if (QuickSelectedIndex > -1)
            {
                bool scrolledUp = false;
                bool scrolledDown = false;

                if (ImGui.IsKeyPressed(ImGuiKey.UpArrow, true))
                {
                    QuickSelectedIndex--;
                    scrolledUp = true;
                }

                if (ImGui.IsKeyPressed(ImGuiKey.DownArrow, true))
                {
                    QuickSelectedIndex++;
                    scrolledDown = true;
                }

                if (QuickSelectedIndex < 0)
                    QuickSelectedIndex = 0;
                if (QuickSelectedIndex >= ItemID.Count)
                    QuickSelectedIndex = ItemID.Count - 1;

                int totalFound = 0;
                for (int i = 1; i < ItemID.Count; i++)
                {
                    if (Lang.GetItemName(i).Value.ToLower().Contains(lowerSearch))
                    {
                        totalFound++;
                    }
                }

                if (QuickSelectedIndex > totalFound)
                    QuickSelectedIndex = totalFound;

                int totalIndex = 0;

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();

                for (int i = 1; i < ItemID.Count; i++)
                {
                    if (Lang.GetItemName(i).Value.ToLower().Contains(lowerSearch))
                    {
                        if (totalIndex == QuickSelectedIndex)
                        {
                            QuickSelectedItem = i;
                        }

                        if (QuickSelectedItem == i)
                        {
                            drawList.AddRectFilled(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + new Vector2(ImGui.GetContentRegionAvail().X - style.ItemSpacing.X, 32f + style.ItemSpacing.Y), Color.White.WithAlpha(0.5f).PackedValue);
                        }

                        ImGuiUtil.ItemButton(i, $"qiiv{i}");
                        ImGui.SameLine();
                        ImGui.Text(Lang.GetItemName(i).Value);

                        if (QuickSelectedItem == i)
                        {
                            if ((scrolledDown || textChanged) && ImGui.GetCursorPosY() > ImGui.GetScrollY() + ImGui.GetWindowSize().Y)
                            {
                                ImGui.SetScrollY(ImGui.GetCursorPosY() - ImGui.GetWindowSize().Y);
                            }
                            if ((scrolledUp || textChanged) && ImGui.GetCursorPosY() < ImGui.GetScrollY() + (32f + style.ItemSpacing.Y))
                            {
                                ImGui.SetScrollY(ImGui.GetCursorPosY() - (32f + style.ItemSpacing.Y * 2f));
                            }
                        }

                        totalIndex++;
                    }
                }
            }
        }
        else
        {
            QuickSelectedIndex = -1;
            QuickSelectedItem = 1;
        }
    }
}
