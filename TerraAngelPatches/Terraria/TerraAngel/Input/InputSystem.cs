using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.NetModules;
using Terraria.Localization;
using Terraria.Graphics;
using Terraria.GameContent;
using Terraria.ID;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TerraAngel.Input
{
    public class InputSystem
    {
        public static KeyboardState keyboardState;
        public static KeyboardState lastKeyboardState;
        public static MouseState mouseState;
        public static MouseState lastMouseState;

        public static void UpdateInput()
        {
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();
        }
        public static void EndUpdateInput()
        {
            lastMouseState = mouseState;
            lastKeyboardState = keyboardState;
        }

        /// <returns>Whether or not a key is currently down</returns>
        public static bool IsKeyDown(Keys key)
        {
            if (!Main.instance.IsActive)
                return false;
            return keyboardState.IsKeyDown(key);
        }
        /// <returns>Whether or not a key is currently up</returns>
        public static bool IsKeyUp(Keys key)
        {
            if (!Main.instance.IsActive)
                return false;
            return !keyboardState.IsKeyDown(key);
        }
        /// <returns>Whether or not a key was pressed this frame</returns>
        public static bool IsKeyPressed(Keys key)
        {
            if (!Main.instance.IsActive)
                return false;
            return keyboardState.IsKeyDown(key) && !lastKeyboardState.IsKeyDown(key);
        }
        /// <returns>Whether or not a key was released this frame</returns>
        public static bool IsKeyReleased(Keys key)
        {
            if (!Main.instance.IsActive)
                return false;
            return !keyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyDown(key);
        }

        public static Vector2 MousePosition
        {
            get
            {
                return new Vector2(mouseState.X, mouseState.Y);
            }
        }
        public static bool LeftMouseDown
        {
            get
            {
                if (!Main.instance.IsActive)
                    return false;
                return mouseState.LeftButton == ButtonState.Pressed;
            }
        }
        public static bool LeftMousePressed
        {
            get
            {
                if (!Main.instance.IsActive)
                    return false;
                return mouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released;
            }
        }
        public static bool LeftMouseReleased
        {
            get
            {
                if (!Main.instance.IsActive)
                    return false;
                return mouseState.LeftButton == ButtonState.Released && lastMouseState.LeftButton == ButtonState.Pressed;
            }
        }
        public static bool RightMouseDown
        {
            get
            {
                if (!Main.instance.IsActive)
                    return false;
                return mouseState.RightButton == ButtonState.Pressed;
            }
        }
        public static bool RightMousePressed
        {
            get
            {
                if (!Main.instance.IsActive)
                    return false;
                return mouseState.RightButton == ButtonState.Pressed && lastMouseState.RightButton == ButtonState.Released;
            }
        }
        public static bool RightMouseReleased
        {
            get
            {
                if (!Main.instance.IsActive)
                    return false;
                return mouseState.RightButton == ButtonState.Released && lastMouseState.RightButton == ButtonState.Pressed;
            }
        }
        public static bool MiddleMouseDown
        {
            get
            {
                if (!Main.instance.IsActive)
                    return false;
                return mouseState.MiddleButton == ButtonState.Pressed;
            }
        }
        public static bool MiddleMousePressed
        {
            get
            {
                if (!Main.instance.IsActive)
                    return false;
                return mouseState.MiddleButton == ButtonState.Pressed && lastMouseState.MiddleButton == ButtonState.Released;
            }
        }
        public static bool MiddleMouseReleased
        {
            get
            {
                if (!Main.instance.IsActive)
                    return false;
                return mouseState.MiddleButton == ButtonState.Released && lastMouseState.MiddleButton == ButtonState.Pressed;
            }
        }
        public static bool KeyCtrl
        {
            get
            {
                return InputSystem.IsKeyDown(Keys.LeftControl) | InputSystem.IsKeyDown(Keys.RightControl);
            }
        }
        public static bool KeyAlt
        {
            get
            {
                return InputSystem.IsKeyDown(Keys.LeftAlt) | InputSystem.IsKeyDown(Keys.RightAlt);
            }
        }
        public static bool KeyShift
        {
            get
            {
                return InputSystem.IsKeyDown(Keys.LeftShift) | InputSystem.IsKeyDown(Keys.RightShift);
            }
        }

        public static int ScrollDelta
        {
            get
            {
                if (!Main.instance.IsActive)
                    return 0;
                if (mouseState.ScrollWheelValue > lastMouseState.ScrollWheelValue)
                    return 1;
                if (mouseState.ScrollWheelValue < lastMouseState.ScrollWheelValue)
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
                if (mouseState.ScrollWheelValue > lastMouseState.ScrollWheelValue)
                    return 1;
                if (mouseState.ScrollWheelValue < lastMouseState.ScrollWheelValue)
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
                if (mouseState.ScrollWheelValue > lastMouseState.ScrollWheelValue)
                    return 1;
                if (mouseState.ScrollWheelValue < lastMouseState.ScrollWheelValue)
                    return -1;
                return 0;
            }
        }
    }
}
