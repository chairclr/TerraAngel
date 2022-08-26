using System.Collections.Generic;

namespace TerraAngel.Cheat
{
    public class ItemBrowser
    {
        private static NVector2 itemDrawSize = new NVector2(32, 32);
        private static string itemSearch = "";
        private static List<string> itemNames = new List<string>(ItemID.Count);
        private static string[] ItemGiveModeNames = Util.EnumFancyNames<ItemGiveMode>();
        private static int currentItemGiveMode = 0;

        public static void DrawBrowser()
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            ImGui.TextUnformatted("Search"); ImGui.SameLine();
            ImGui.InputText("##ItemSearch", ref itemSearch, 64);
            ImGui.TextUnformatted("Give Type"); ImGui.SameLine();
            ImGui.Combo("##ItemGiveType", ref currentItemGiveMode, ItemGiveModeNames, ItemGiveModeNames.Length);
            bool searchEmpty = itemSearch.Length == 0;
            if (ImGui.BeginChild("ItemBrowserScrolling"))
            {
                float windowMaxX = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new NVector2(4f));
                for (int i = 1; i < ItemID.Count; i++)
                {
                    if (searchEmpty || itemNames[i].ToLower().Contains(itemSearch.ToLower()))
                    {
                        if (ImGuiUtil.ItemButton(i, $"ibi{i}", true, true))
                        {
                            switch ((ItemGiveMode)currentItemGiveMode)
                            {
                                case ItemGiveMode.InMouse:
                                    Main.playerInventory = true;
                                    if (Main.mouseItem.type == 0)
                                    {
                                        Main.mouseItem.SetDefaults(i);
                                        Main.mouseItem.stack = Main.mouseItem.maxStack;
                                    }
                                    break;
                                case ItemGiveMode.DropLocal:
                                    {
                                        Item local = Main.item[Item.NewItem(new Terraria.DataStructures.EntitySource_Sync(), Main.LocalPlayer.position, Width: Main.LocalPlayer.width, Height: Main.LocalPlayer.height, i, Stack: 999, true, 0, true)];
                                        local.stack = Utils.Clamp(local.stack, 1, local.maxStack);
                                    }
                                    break;
                                case ItemGiveMode.DropServerSide:
                                    {
                                        Item local = Main.item[Item.NewItem(new Terraria.DataStructures.EntitySource_Sync(), Main.LocalPlayer.position, Width: Main.LocalPlayer.width, Height: Main.LocalPlayer.height, i, Stack: 999, false, 0, true)];
                                        local.stack = Utils.Clamp(local.stack, 1, local.maxStack);
                                    }
                                    break;
                            }
                        }

                        // Maximize number of item buttons you can see
                        float nextButtonX = ImGui.GetItemRectMax().X + style.ItemSpacing.X + itemDrawSize.X;
                        if (i + 1 < Terraria.ID.ItemID.Count && nextButtonX < windowMaxX)
                            ImGui.SameLine();
                    }
                }
                ImGui.PopStyleVar();
                ImGui.EndChild();
            }
        }

        public static void Init()
        {
            for (int i = 0; i < Terraria.ID.ItemID.Count; i++)
            {
                itemNames.Add(Lang.GetItemName(i).ToString());
            }
        }

        public enum ItemGiveMode
        {
            InMouse,
            DropLocal,
            DropServerSide,
        }
    }
}
