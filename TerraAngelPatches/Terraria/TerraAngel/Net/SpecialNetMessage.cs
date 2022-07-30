using System;
using System.IO;
using Ionic.Zlib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Social;
using Terraria;
using TerraAngel.ID;
using TerraAngel.Utility;

namespace TerraAngel.Net
{
    public class SpecialNetMessage
    {
        /// <summary>
        /// Sends custom data to the server
        /// </summary>
        /// <param name="number">13, 5: Player index</param>
        /// <param name="number2">13: Player X, 5: Slot index</param>
        /// <param name="number3">13: Player Y, 5: Stack</param>
        /// <param name="number4">13: Selected item, 5: Prefix</param>
        /// <param name="number5">5: NetID</param>
        /// <param name="number6"></param>
        /// <param name="number7"></param>
        public static void SendData(int msgType, NetworkText text = null, int number = 0, float number2 = 0f, float number3 = 0f, float number4 = 0f, int number5 = 0, int number6 = 0, int number7 = 0)
        {
            if (Main.netMode == 0)
            {
                return;
            }

            int bufferIndex = 256;
            if (text == null)
            {
                text = NetworkText.Empty;
            }

            lock (NetMessage.buffer[bufferIndex])
            {
                BinaryWriter writer = NetMessage.buffer[bufferIndex].writer;
                if (writer == null)
                {
                    NetMessage.buffer[bufferIndex].ResetWriter();
                    writer = NetMessage.buffer[bufferIndex].writer;
                }

                writer.BaseStream.Position = 0L;
                long position = writer.BaseStream.Position;
                writer.BaseStream.Position += 2L;
                writer.Write((byte)msgType);

                switch (msgType)
                {
                    case 13:
                        Player player6 = Main.player[number];
                        writer.Write((byte)number);
                        BitsByte bitsByte9 = (byte)0;
                        bitsByte9[0] = false;
                        bitsByte9[1] = false;
                        bitsByte9[2] = false;
                        bitsByte9[3] = false;
                        bitsByte9[4] = false;
                        bitsByte9[5] = player6.controlUseItem;
                        bitsByte9[6] = player6.direction == 1;
                        writer.Write(bitsByte9);
                        BitsByte bitsByte10 = (byte)0;
                        bitsByte10[0] = false;
                        bitsByte10[1] = false;
                        bitsByte10[2] = player6.velocity != Vector2.Zero;
                        bitsByte10[3] = false;
                        bitsByte10[4] = player6.gravDir == 1f;
                        bitsByte10[5] = player6.shieldRaised;
                        bitsByte10[6] = false;
                        writer.Write(bitsByte10);
                        BitsByte bitsByte11 = (byte)0;
                        bitsByte11[0] = false;
                        bitsByte11[1] = false;
                        bitsByte11[2] = player6.sitting.isSitting;
                        bitsByte11[3] = player6.downedDD2EventAnyDifficulty;
                        bitsByte11[4] = player6.isPettingAnimal;
                        bitsByte11[5] = player6.isTheAnimalBeingPetSmall;
                        bitsByte11[6] = player6.PotionOfReturnOriginalUsePosition.HasValue;
                        bitsByte11[7] = false;
                        writer.Write(bitsByte11);
                        BitsByte bitsByte12 = (byte)0;
                        bitsByte12[0] = player6.sleeping.isSleeping;
                        writer.Write(bitsByte12);
                        writer.Write((byte)number4);
                        writer.WriteVector2(new Vector2(number2, number3));
                        if (bitsByte10[2])
                        {
                            writer.WriteVector2(Vector2.Zero);
                        }

                        if (bitsByte11[6])
                        {
                            writer.WriteVector2(player6.PotionOfReturnOriginalUsePosition.Value);
                            writer.WriteVector2(player6.PotionOfReturnHomePosition.Value);
                        }
                        break;
                    case 5:
                    {
                        // player id
                        writer.Write((byte)number);
                        // slot
                        writer.Write((short)number2);
                        // stack
                        writer.Write((short)number3);
                        // Prefix
                        writer.Write((byte)number4);
                        // NetID
                        writer.Write((short)number5);
                    }
                    break;
                }

                int packetLength = (int)writer.BaseStream.Position;
                writer.BaseStream.Position = position;
                writer.Write((short)packetLength);
                writer.BaseStream.Position = packetLength;
                if (Main.netMode == 1)
                {
                    if (Netplay.Connection.Socket.IsConnected())
                    {
                        try
                        {
                            NetMessage.buffer[bufferIndex].spamCount++;
                            Main.ActiveNetDiagnosticsUI.CountSentMessage(msgType, packetLength);
                            Netplay.Connection.Socket.AsyncSend(NetMessage.buffer[bufferIndex].writeBuffer, 0, packetLength, Netplay.Connection.ClientWriteCallBack);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public static void SendPlayerControl(Vector2 position, int selectedIndex = -1)
        {
            int trueSelectedIndex = (selectedIndex == -1) ? Main.LocalPlayer.selectedItem : selectedIndex;
            SendData(MessageID.PlayerControls, number: Main.myPlayer, number2: position.X, number3: position.Y, number4: trueSelectedIndex);
        }

        public static void SendInventorySlot(int slot, int itemId, int stack = 1, int prefix = 0)
        {
            SendData(MessageID.SyncEquipment, number: Main.myPlayer, number2: slot, number3: stack, number4: prefix, number5: itemId);
        }

        public static void SendPlaceTile(int x, int y, int tile, int useSlot = 0, bool resetToNormal = true)
        {
            int itemId = TileUtil.TileToItem[tile];
            if (itemId == -1)
                itemId = 0;
            SendPlayerControl(new Vector2(x * 16f, y * 16f), 0);
            SendInventorySlot(useSlot, itemId);
            NetMessage.SendData(MessageID.TileManipulation, number: TileManipulationID.PlaceTile, number2: x, number3: y, number4: tile);

            if (resetToNormal)
            {
                NetMessage.SendData(MessageID.PlayerControls, number: Main.myPlayer);
                NetMessage.SendData(MessageID.SyncEquipment, number: Main.myPlayer, number2: Main.LocalPlayer.selectedItem, number3: Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].prefix);
            }
        }

        public static void SendPlaceWall(int x, int y, int wall, int useSlot = 0, bool resetToNormal = true)
        {
            int itemId = TileUtil.WallToItem[wall];
            if (itemId == -1)
                itemId = 0;
            SendPlayerControl(new Vector2(x * 16f, y * 16f), 0);
            SendInventorySlot(useSlot, itemId);
            NetMessage.SendData(MessageID.TileManipulation, number: TileManipulationID.PlaceWall, number2: x, number3: y, number4: wall);

            if (resetToNormal)
            {
                NetMessage.SendData(MessageID.PlayerControls, number: Main.myPlayer);
                NetMessage.SendData(MessageID.SyncEquipment, number: Main.myPlayer, number2: Main.LocalPlayer.selectedItem, number3: Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].prefix);
            }
        }
    }
}
