using System;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;

namespace TerraAngel.Hooks;

public class MiscHooks
{
    public static int LastCursorOverride = -1;

    public static bool ShouldMouseBeVisible(bool visible)
    {
        if (ClientLoader.MainRenderer is not null && (ImGui.GetIO().WantCaptureMouse || (ImGui.GetIO().WantCaptureKeyboard && !(ImGui.GetIO().KeyAlt && (ClientLoader.ChatWindow?.IsChatting ?? false) && LastCursorOverride == 2))))
            visible = true;
        if (!Main.instance?.IsActive ?? false)
            visible = true;

        return visible;
    }

    private static int FramesSinceStopped = 0;

    public static void MouseInputHook(Action orig)
    {
        if (!Main.instance.IsActive)
            return;
        if (ClientLoader.MainRenderer is not null && ImGui.GetIO().WantCaptureMouse)
        {
            PlayerInput.ScrollWheelDelta = 0;
            PlayerInput.ScrollWheelDeltaForUI = 0;
            PlayerInput.ScrollWheelValueOld = PlayerInput.ScrollWheelValue;
            PlayerInput.GamepadScrollValue = 0;
            FramesSinceStopped = 0;
        }
        else
        {
            FramesSinceStopped++;
            if (FramesSinceStopped < 2)
            {
                PlayerInput.ScrollWheelDelta = 0;
                PlayerInput.ScrollWheelDeltaForUI = 0;
                PlayerInput.ScrollWheelValueOld = PlayerInput.ScrollWheelValue;
                PlayerInput.GamepadScrollValue = 0;
            }
            orig();
            if (FramesSinceStopped < 2)
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
            {
                PlayerInput.ScrollWheelValueOld = PlayerInput.ScrollWheelValue;
                PlayerInput.ScrollWheelDelta = 0;
                PlayerInput.ScrollWheelDeltaForUI = 0;
                return;
            }
        }
        orig();
    }
}
