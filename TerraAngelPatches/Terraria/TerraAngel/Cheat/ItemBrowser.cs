using System;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace TerraAngel.Cheat
{
    public class ItemBrowser
    {
        private static NVector2 itemDrawSize = new NVector2(32, 32);
        private static string itemSearch = "";
        private static List<string> itemNames = new List<string>(ItemID.Count);
        private static string[] ItemGiveModeNames = Util.EnumFancyNames<ItemGiveMode>();
        private static int currentItemGiveMode = 0;
        private static bool syncItemWithServer = true;

        public static void DrawBrowser()
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            ImGui.TextUnformatted("Search"); ImGui.SameLine();
            ImGui.InputText("##ItemSearch", ref itemSearch, 64);
            ImGui.TextUnformatted("Give Type"); ImGui.SameLine();
            ImGui.PushItemWidth(MathF.Max(ImGui.GetContentRegionAvail().X / 3.4f, ImGui.CalcTextSize(ItemGiveModeNames[currentItemGiveMode]).X + 30f));
            ImGui.Combo("##ItemGiveType", ref currentItemGiveMode, ItemGiveModeNames, ItemGiveModeNames.Length);
            ImGui.PopItemWidth();
            ImGui.SameLine();
            ImGui.Checkbox("Sync With Server", ref syncItemWithServer);
            bool searchEmpty = itemSearch.Length == 0;
            if (ImGui.BeginChild("ItemBrowserScrolling"))
            {
                float windowMaxX = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new NVector2(4f));
                for (int i = 1; i < ItemID.Count; i++)
                {
                    if (searchEmpty || itemNames[i].ToLower().Contains(itemSearch.ToLower()))
                    {
                        if (ImGuiUtil.ItemButton(i, $"ibi{i}", true))
                        {
                            switch ((ItemGiveMode)currentItemGiveMode)
                            {
                                case ItemGiveMode.InMouse:
                                    Main.playerInventory = true;
                                    if (Main.mouseItem.type == 0)
                                    {
                                        Main.mouseItem.SetDefaults(i);
                                        Main.mouseItem.stack = Main.mouseItem.maxStack;

                                        if (syncItemWithServer)
                                        {
                                            Main.LocalPlayer.inventory[58] = Main.mouseItem.Clone();
                                            NetMessage.SendData(MessageID.SyncEquipment, number: Main.myPlayer, number2: 58);
                                        }
                                    }
                                    break;
                                case ItemGiveMode.DropInWorld:
                                    {
                                        CreateItem(Main.LocalPlayer.Center, Vector2.Zero, i, 9999, !syncItemWithServer);
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
            DropInWorld,
        }

        public static int CreateItem(Vector2 position, Vector2 velocity, int id, int stack = 1, bool localOnly = false)
        {
            int itemIndex = Item.NewItem(null, (int)position.X, (int)position.Y, 1, 1, id, stack, localOnly, 0, true);
            Main.item[itemIndex].velocity = velocity;
            Main.item[itemIndex].newAndShiny = false;
            Main.item[itemIndex].stack = Utils.Clamp(stack, 1, Main.item[itemIndex].maxStack);
            if (!localOnly)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, (float)Main.item[itemIndex].netID);
            }
            return itemIndex;
        }
    }
}
