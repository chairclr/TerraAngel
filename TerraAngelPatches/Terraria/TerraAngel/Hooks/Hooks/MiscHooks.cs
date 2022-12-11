using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.Localization;

namespace TerraAngel.Hooks.Hooks;

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
        HookUtil.HookGen(WorldGen.TileFrame, TileFrameHook);
        HookUtil.HookGen<PlayerInput>("MouseInput", MouseInputHook);
        HookUtil.HookGen<PlayerInput>("UpdateInput", UpdateInputHook);
        HookUtil.HookGen<Main>("DrawInterface_41_InterfaceLogic4", InterfaceLogic4Hook);

        foreach (PropertyInfo prop in typeof(TriggersSet).GetProperties())
        {
            if (prop.CanRead && prop.PropertyType == typeof(bool) && prop.GetMethod is not null)
            {
                HookUtil.HookGen(prop.GetMethod, TriggerSetGetHook1);
            }
        }
    }

    public static bool TriggerSetGetHook1(Func<TriggersSet, bool> orig, TriggersSet self)
    {
        if (ClientLoader.MainRenderer is not null && ImGui.GetIO().WantCaptureKeyboard)
            return false;
        return orig(self);
    }
    public static bool TriggerSetGetHook2(Func<TriggersSet, bool> orig, TriggersSet self)
    {
        if (ClientLoader.MainRenderer is not null && (ImGui.GetIO().WantCaptureKeyboard || ImGui.GetIO().WantCaptureMouse))
            return false;
        return orig(self);
    }

    private static int lastCursorOverride = -1;
    public static void InterfaceLogic4Hook(Action orig)
    {
        lastCursorOverride = Main.cursorOverride;
        orig();
    }
    public static void set_IsMouseVisibleHook(Action<Game, bool> orig, Game self, bool visible)
    {
        if (ClientLoader.MainRenderer is not null && (ImGui.GetIO().WantCaptureMouse || (ImGui.GetIO().WantCaptureKeyboard && !(ImGui.GetIO().KeyAlt && (ClientLoader.ChatWindow?.IsChatting ?? false) && lastCursorOverride == 2))))
            visible = true;
        if (!Main.instance?.IsActive ?? false)
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
        if (xStart % Main.sectionWidth == 0 && yStart % Main.sectionHeight == 0 && width == Main.sectionWidth && height == Main.sectionHeight)
        {
            Main.tile.LoadedTileSections[xStart / Main.sectionWidth, yStart / Main.sectionHeight] = true;
        }

        orig(reader, xStart, yStart, width, height);
    }
    public static void SendDataHook(Action<int, int, int, NetworkText, int, float, float, float, int, int, int> orig, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
    {
        if (NetMessageWindow.LoggingMessages)
        {
            string stackTrace = "";
            if (NetMessageWindow.MessagesToLogTraces.Contains(msgType))
            {
                stackTrace = new StackTrace(2, true).ToString();

            }
            NetMessageWindow.AddPacket(new NetPacketInfo(msgType, true, number, number2, number3, number4, number5, number6, number7, stackTrace));
        }

        orig(msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
    }

    public delegate void GetDataDesc(MessageBuffer self, int start, int length, out int messageType);

    public static void GetDataHook(GetDataDesc orig, MessageBuffer self, int start, int length, out int messageType)
    {
        orig(self, start, length, out messageType);

        if (NetMessageWindow.LoggingMessages)
        {
            NetMessageWindow.AddPacket(new NetPacketInfo(messageType, false, 0, 0, 0, 0, 0, 0, 0));
        }
    }

    public static bool framingDisabled = false;
    public static void TileFrameHook(Action<int, int, bool, bool> orig, int i, int j, bool resetFrame = false, bool noBreak = false)
    {
        if (framingDisabled)
            return;

        orig(i, j, resetFrame, noBreak);
    }
    private static int framesSinceStopped = 0;
    public static void MouseInputHook(Action orig)
    {
        if (ClientLoader.MainRenderer is not null && ImGui.GetIO().WantCaptureMouse)
        {
            PlayerInput.ScrollWheelDelta = 0;
            PlayerInput.ScrollWheelDeltaForUI = 0;
            PlayerInput.ScrollWheelValueOld = PlayerInput.ScrollWheelValue;
            PlayerInput.GamepadScrollValue = 0;
            framesSinceStopped = 0;
        }
        else
        {
            framesSinceStopped++;
            if (framesSinceStopped < 2)
            {
                PlayerInput.ScrollWheelDelta = 0;
                PlayerInput.ScrollWheelDeltaForUI = 0;
                PlayerInput.ScrollWheelValueOld = PlayerInput.ScrollWheelValue;
                PlayerInput.GamepadScrollValue = 0;
            }
            orig();
            if (framesSinceStopped < 2)
            {
                PlayerInput.ScrollWheelDelta = 0;
                PlayerInput.ScrollWheelDeltaForUI = 0;
                PlayerInput.ScrollWheelValueOld = PlayerInput.ScrollWheelValue;
                PlayerInput.GamepadScrollValue = 0;
            }
        }
    }
    public static void UpdateInputHook(Action orig)
    {
        if (!Main.instance.IsActive)
            return;
        if (ClientLoader.MainRenderer is not null)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            if (io.WantCaptureKeyboard && !io.WantCaptureMouse)
            {
                PlayerInput.MouseInfoOld = PlayerInput.MouseInfo;
                PlayerInput.MouseInfo = Mouse.GetState();
                PlayerInput.MouseX = (int)((float)PlayerInput.MouseInfo.X * PlayerInput.RawMouseScale.X);
                PlayerInput.MouseY = (int)((float)PlayerInput.MouseInfo.Y * PlayerInput.RawMouseScale.Y);
                PlayerInput.UpdateMainMouse();
                PlayerInput.CacheZoomableValues();
            }

            if (io.WantCaptureKeyboard)
                return;
        }
        orig();
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
