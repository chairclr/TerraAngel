using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using Terraria;
using ImGuiNET;
using ReLogic.OS;
using Terraria.DataStructures;
using Terraria.ID;
using TerraAngel.Cheat;
using TerraAngel.Hooks;
using System.IO;
using TerraAngel;
using Terraria.Localization;
using System.Diagnostics;

namespace TerraAngel.Hooks.Hooks
{
    public class MiscHooks
    {
        public static void Generate()
        {
            HookUtil.HookGen<Main>("SetTitle", SetTitleHook);
            //HookUtil.HookGen(typeof(SDL2.SDL).Assembly.GetType("Microsoft.Xna.Framework.SDL2_FNAPlatform"), "OnIsMouseVisibleChanged", OnIsMouseVisibleChangedHook);
            HookUtil.HookGen<Game>("set_IsMouseVisible", set_IsMouseVisibleHook);
            //HookUtil.HookGen(Collision.EmptyTile, EmptyTileHook);
            //HookUtil.HookGen(Collision.TileCollision, TileCollisionHook);
            //HookUtil.HookGen<Terraria.UI.Gamepad.UILinkPointNavigator>("Update", UILinkPointNavigatorUpdateHook);
            HookUtil.HookGen(NetMessage.DecompressTileBlock_Inner, DecompressTileBlock_InnerHook);
            HookUtil.HookGen(NetMessage.SendData, SendDataHook);
            HookUtil.HookGen<MessageBuffer>("GetData", GetDataHook);
        }
        public static void set_IsMouseVisibleHook(Action<Game, bool> orig, Game self, bool visible)
        {
            if (ClientLoader.SetupRenderer && ImGui.GetIO().WantCaptureMouse)
                visible = true;
            orig(self, visible);
        }
        public static void SetTitleHook(Action<Main> orig, Main self)
        {
            
            SDL2.SDL.SDL_SetWindowTitle(self.Window.Handle, "TerraAngel");
            //orig(self);
        }
        // Justification for patching out: its literally useless who tf uses up and down to navigate the menu headass
        public static void UILinkPointNavigatorUpdateHook(Action orig)
        {
            //orig();
        }
        public static void DecompressTileBlock_InnerHook(Action<BinaryReader, int, int, int, int> orig, BinaryReader reader, int xStart, int yStart, int width, int height)
        {
            if (CringeManager.LoadedTileSections == null)
                CringeManager.LoadedTileSections = new bool[Main.maxSectionsX, Main.maxSectionsY];

            if (xStart % Main.sectionWidth == 0 && yStart % Main.sectionHeight == 0 && width == Main.sectionWidth && height == Main.sectionHeight)
            {
                CringeManager.LoadedTileSections[xStart / Main.sectionWidth, yStart / Main.sectionHeight] = true;
            }

            orig(reader, xStart, yStart, width, height);
        }
        public static void SendDataHook(Action<int, int, int, NetworkText, int, float, float, float, int, int, int> orig, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
        {
            if (Client.ClientWindows.NetMessageWindow.LoggingMessages)
            {
                string stackTrace = "";
                if (Client.ClientWindows.NetMessageWindow.MessagesToLogTraces.Contains(msgType))
                {
                    stackTrace = new StackTrace(2, true).ToString();
                    
                }
                Client.ClientWindows.NetMessageWindow.AddPacket(new Client.ClientWindows.NetPacketInfo(msgType, true, number, number2, number3, number4, number5, number6, number7, stackTrace));
            }

            orig(msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
        }

        public delegate void GetDataDesc(MessageBuffer self, int start, int length, out int messageType);

        public static void GetDataHook(GetDataDesc orig, MessageBuffer self, int start, int length, out int messageType)
        {
            orig(self, start, length, out messageType);

            if (Client.ClientWindows.NetMessageWindow.LoggingMessages)
            {
                Client.ClientWindows.NetMessageWindow.AddPacket(new Client.ClientWindows.NetPacketInfo(messageType, false, 0, 0, 0, 0, 0, 0, 0));
            }
        }
        //public static Vector2 TileCollisionHook(Func<Vector2, Vector2, int, int, bool, bool, int, Vector2> orig, Vector2 Position, Vector2 Velocity, int Width, int Height, bool fallThrough = false, bool fall2 = false, int gravDir = 1)
        //{
        //    return orig(Position, Velocity, Width, Height, fallThrough, fall2, gravDir);
        //}
        //public static bool EmptyTileHook(Func<int, int, bool, bool> orig, int i, int j, bool ignoreTiles)
        //{
        //    if (GlobalCheatManager.NoClip)
        //        return true;
        //    return orig(i, j, ignoreTiles);
        //}
        //public static void OnIsMouseVisibleChangedHook(Action<bool> orig, bool visible)
        //{
        //    if (ClientLoader.SetupRenderer && ImGui.GetIO().WantCaptureMouse)
        //    {
        //        Main.instance.IsMouseVisible
        //        visible = true;
        //    }
        //    orig(visible);
        //}
    }
}
