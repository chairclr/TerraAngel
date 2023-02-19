using Microsoft.Xna.Framework.Input;

namespace TerraAngel.Input;

public class InputSystem
{
    public static KeyboardState KeyboardState;

    public static KeyboardState LastKeyboardState;

    public static MouseState MouseState;

    public static MouseState LastMouseState;

    public static void UpdateInput()
    {
        MouseState = Mouse.GetState();
        KeyboardState = Keyboard.GetState();
    }

    public static void EndUpdateInput()
    {
        LastMouseState = MouseState;
        LastKeyboardState = KeyboardState;
    }

    /// <returns>Whether or not a key is currently down</returns>
    public static bool IsKeyDown(Keys key)
    {
        if (!Main.instance.IsActive || key == Keys.None || (ClientLoader.MainRenderer is not null && ImGui.GetIO().WantTextInput))
            return false;
        return KeyboardState.IsKeyDown(key);
    }

    /// <returns>Whether or not a key is currently up</returns>
    public static bool IsKeyUp(Keys key)
    {
        if (!Main.instance.IsActive || key == Keys.None || (ClientLoader.MainRenderer is not null && ImGui.GetIO().WantTextInput))
            return false;
        return !KeyboardState.IsKeyDown(key);
    }

    /// <returns>Whether or not a key was pressed this frame</returns>
    public static bool IsKeyPressed(Keys key)
    {
        if (!Main.instance.IsActive || key == Keys.None || (ClientLoader.MainRenderer is not null && ImGui.GetIO().WantTextInput))
            return false;
        return KeyboardState.IsKeyDown(key) && !LastKeyboardState.IsKeyDown(key);
    }

    /// <returns>Whether or not a key was released this frame</returns>
    public static bool IsKeyReleased(Keys key)
    {
        if (!Main.instance.IsActive || key == Keys.None || (ClientLoader.MainRenderer is not null && ImGui.GetIO().WantTextInput))
            return false;
        return !KeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyDown(key);
    }

    /// <returns>Whether or not a key is currently down</returns>
    public static bool IsKeyDownRaw(Keys key)
    {
        if (!Main.instance.IsActive)
            return false;
        return KeyboardState.IsKeyDown(key);
    }

    /// <returns>Whether or not a key is currently up</returns>
    public static bool IsKeyUpRaw(Keys key)
    {
        if (!Main.instance.IsActive)
            return false;
        return !KeyboardState.IsKeyDown(key);
    }

    /// <returns>Whether or not a key was pressed this frame</returns>
    public static bool IsKeyPressedRaw(Keys key)
    {
        if (!Main.instance.IsActive)
            return false;
        return KeyboardState.IsKeyDown(key) && !LastKeyboardState.IsKeyDown(key);
    }

    /// <returns>Whether or not a key was released this frame</returns>
    public static bool IsKeyReleasedRaw(Keys key)
    {
        if (!Main.instance.IsActive)
            return false;
        return !KeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyDown(key);
    }

    public static Vector2 MousePosition
    {
        get
        {
            return new Vector2(MouseState.X, MouseState.Y);
        }
    }

    public static bool LeftMouseDown
    {
        get
        {
            if (!Main.instance.IsActive)
                return false;
            return MouseState.LeftButton == ButtonState.Pressed;
        }
    }

    public static bool LeftMousePressed
    {
        get
        {
            if (!Main.instance.IsActive)
                return false;
            return MouseState.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton == ButtonState.Released;
        }
    }

    public static bool LeftMouseReleased
    {
        get
        {
            if (!Main.instance.IsActive)
                return false;
            return MouseState.LeftButton == ButtonState.Released && LastMouseState.LeftButton == ButtonState.Pressed;
        }
    }

    public static bool RightMouseDown
    {
        get
        {
            if (!Main.instance.IsActive)
                return false;
            return MouseState.RightButton == ButtonState.Pressed;
        }
    }

    public static bool RightMousePressed
    {
        get
        {
            if (!Main.instance.IsActive)
                return false;
            return MouseState.RightButton == ButtonState.Pressed && LastMouseState.RightButton == ButtonState.Released;
        }
    }

    public static bool RightMouseReleased
    {
        get
        {
            if (!Main.instance.IsActive)
                return false;
            return MouseState.RightButton == ButtonState.Released && LastMouseState.RightButton == ButtonState.Pressed;
        }
    }

    public static bool MiddleMouseDown
    {
        get
        {
            if (!Main.instance.IsActive)
                return false;
            return MouseState.MiddleButton == ButtonState.Pressed;
        }
    }

    public static bool MiddleMousePressed
    {
        get
        {
            if (!Main.instance.IsActive)
                return false;
            return MouseState.MiddleButton == ButtonState.Pressed && LastMouseState.MiddleButton == ButtonState.Released;
        }
    }

    public static bool MiddleMouseReleased
    {
        get
        {
            if (!Main.instance.IsActive)
                return false;
            return MouseState.MiddleButton == ButtonState.Released && LastMouseState.MiddleButton == ButtonState.Pressed;
        }
    }

    public static bool Ctrl
    {
        get
        {
            return IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl);
        }
    }

    public static bool Alt
    {
        get
        {
            return IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt);
        }
    }

    public static bool Shift
    {
        get
        {
            return IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift);
        }
    }

    public static int ScrollDelta
    {
        get
        {
            if (!Main.instance.IsActive)
                return 0;
            if (MouseState.ScrollWheelValue > LastMouseState.ScrollWheelValue)
                return 1;
            if (MouseState.ScrollWheelValue < LastMouseState.ScrollWheelValue)
                return -1;
            return 0;
        }
    }
    public static int ScrollDeltaXNA
    {
        get
        {
            if (!Main.instance.IsActive)
                return 0;
            if (ImGui.GetIO().WantCaptureMouse)
                return 0;
            if (MouseState.ScrollWheelValue > LastMouseState.ScrollWheelValue)
                return 1;
            if (MouseState.ScrollWheelValue < LastMouseState.ScrollWheelValue)
                return -1;
            return 0;
        }
    }
    public static int ScrollDeltaImGui
    {
        get
        {
            if (!Main.instance.IsActive)
                return 0;
            if (!ImGui.GetIO().WantCaptureMouse)
                return 0;
            if (MouseState.ScrollWheelValue > LastMouseState.ScrollWheelValue)
                return 1;
            if (MouseState.ScrollWheelValue < LastMouseState.ScrollWheelValue)
                return -1;
            return 0;
        }
    }
}
