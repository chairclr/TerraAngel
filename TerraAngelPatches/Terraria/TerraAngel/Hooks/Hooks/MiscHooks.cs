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
        //foreach (PropertyInfo prop in typeof(TriggersSet).GetProperties())
        //{
        //    if (prop.CanRead && prop.PropertyType == typeof(bool) && prop.GetMethod is not null)
        //    {
        //        HookUtil.HookGen(prop.GetMethod, TriggerSetGetHook1);
        //    }
        //}
    }

    public static bool TriggerSetGetHook1(Func<TriggersSet, bool> orig, TriggersSet self)
    {
        if (ClientLoader.MainRenderer is not null && ImGui.GetIO().WantCaptureKeyboard)
            return false;
        return orig(self);
    }

    public static int lastCursorOverride = -1;

    public static bool ShouldMouseBeVisible(bool visible)
    {
        if (ClientLoader.MainRenderer is not null && (ImGui.GetIO().WantCaptureMouse || (ImGui.GetIO().WantCaptureKeyboard && !(ImGui.GetIO().KeyAlt && (ClientLoader.ChatWindow?.IsChatting ?? false) && lastCursorOverride == 2))))
            visible = true;
        if (!Main.instance?.IsActive ?? false)
            visible = true;

        return visible;
    }

    public static bool framingDisabled = false;

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
}
