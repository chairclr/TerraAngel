using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework;
using TerraAngel.Graphics;
using Terraria;

namespace TerraAngel.Cheat
{
    public class ItemBrowser
    {
        private static System.Numerics.Vector2 itemDrawSize = new System.Numerics.Vector2(32, 32);
        private static string itemSearch = "";
        private static Type itemIdType = typeof(Terraria.ID.ItemID);
        private static List<string> itemNames = new List<string>(Terraria.ID.ItemID.Count);
        private static ItemGiveMode itemGiveMode = ItemGiveMode.InMouse;
        private static string[] ItemGiveModeNames = Utility.Util.EnumFancyNames<ItemGiveMode>();
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
                for (int i = 1; i < Terraria.ID.ItemID.Count; i++)
                {
                    if (searchEmpty || itemNames[i].ToLower().Contains(itemSearch.ToLower()))
                    {
                        if (ImGuiUtil.ItemButton(i, $"ibi{i}", out _, out Vector2 max, out _, true))
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
                        float nextButtonX = max.X + style.ItemSpacing.X + itemDrawSize.X;
                        if (i + 1 < Terraria.ID.ItemID.Count && nextButtonX < windowMaxX)
                            ImGui.SameLine();
                    }
                }
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
