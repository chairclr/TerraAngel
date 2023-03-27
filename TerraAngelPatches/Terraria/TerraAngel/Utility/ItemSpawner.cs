namespace TerraAngel.Utility;

public static class ItemSpawner
{
    public static Item SpawnItemInMouse(int type, int stack = 1, bool syncWithServer = true, bool overrideItem = false)
    {
        Main.playerInventory = true;
        if (Main.mouseItem.type == 0 || overrideItem)
        {
            Main.mouseItem.SetDefaults(type);
            Main.mouseItem.stack = Main.mouseItem.maxStack;
            Main.mouseItem.stack = Utils.Clamp(stack, 1, Main.mouseItem.maxStack);

            if (syncWithServer)
            {
                Main.LocalPlayer.inventory[58] = Main.mouseItem.Clone();
                NetMessage.SendData(MessageID.SyncEquipment, number: Main.myPlayer, number2: 58);
            }
        }

        return Main.mouseItem;
    }

    public static Item SpawnItemInWorld(Vector2 position, int id, Vector2 velocity = default, int stack = 1, bool syncWithServer = true)
    {
        int itemIndex = Item.NewItem(null, (int)position.X, (int)position.Y, 1, 1, id, stack, !syncWithServer, 0, true);

        Item item = Main.item[itemIndex];

        item.velocity = velocity;
        item.newAndShiny = false;
        item.stack = Utils.Clamp(stack, 1, item.maxStack);

        if (syncWithServer)
        {
            NetMessage.SendData(MessageID.SyncItem, number: itemIndex, number2: item.netID);
        }

        return item;
    }

    public static Item? SpawnItemInSelected(int type, int stack = 1, bool syncWithServer = true)
    {
        if (Main.LocalPlayer.selectedItem > 0 && Main.LocalPlayer.selectedItem < 10)
        {
            Item item = Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem];
            item.SetDefaults(type);
            item.stack = Utils.Clamp(stack, 1, item.maxStack);

            if (syncWithServer)
            {
                NetMessage.SendData(MessageID.SyncEquipment, number: Main.myPlayer, number2: Main.LocalPlayer.selectedItem);
            }

            return item;
        }

        return null;
    }
}
