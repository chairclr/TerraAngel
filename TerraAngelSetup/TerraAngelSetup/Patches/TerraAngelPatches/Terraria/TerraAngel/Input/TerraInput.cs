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
    public class TerraInput
    {
        public static KeyboardState keyboardState;
        public static KeyboardState lastKeyboardState;
        public static MouseState mouseState;
        public static MouseState lastMouseState;

        public void UpdateInput()
        {
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();
        }
        public void EndUpdateInput()
        {
            lastMouseState = mouseState;
            lastKeyboardState = keyboardState;
        }

        /// <returns>Whether or not a key is currently down</returns>
        public bool IsKeyDown(Keys key)
        {
            return keyboardState.IsKeyDown(key);
        }
        /// <returns>Whether or not a key is currently up</returns>
        public bool IsKeyUp(Keys key)
        {
            return !keyboardState.IsKeyDown(key);
        }
        /// <returns>Whether or not a key was pressed this frame</returns>
        public bool IsKeyPressed(Keys key)
        {
            return keyboardState.IsKeyDown(key) && !lastKeyboardState.IsKeyDown(key);
        }
        /// <returns>Whether or not a key was released this frame</returns>
        public bool IsKeyReleased(Keys key)
        {
            return !keyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyDown(key);
        }

        public Vector2 MousePosition
        {
            get
            {
                return new Vector2(mouseState.X, mouseState.Y);
            }
        }
        public bool LeftMouseDown
        {
            get
            {
                return mouseState.LeftButton == ButtonState.Pressed;
            }
        }
        public bool LeftMousePressed
        {
            get
            {
                return mouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released;
            }
        }
        public bool LeftMouseReleased
        {
            get
            {
                return mouseState.LeftButton == ButtonState.Released && lastMouseState.LeftButton == ButtonState.Pressed;
            }
        }
        public bool RightMouseDown
        {
            get
            {
                return mouseState.RightButton == ButtonState.Pressed;
            }
        }
        public bool RightMousePressed
        {
            get
            {
                return mouseState.RightButton == ButtonState.Pressed && lastMouseState.RightButton == ButtonState.Released;
            }
        }
        public bool RightMouseReleased
        {
            get
            {
                return mouseState.RightButton == ButtonState.Released && lastMouseState.RightButton == ButtonState.Pressed;
            }
        }
        public bool MiddleMouseDown
        {
            get
            {
                return mouseState.MiddleButton == ButtonState.Pressed;
            }
        }
        public bool MiddleMousePressed
        {
            get
            {
                return mouseState.MiddleButton == ButtonState.Pressed && lastMouseState.MiddleButton == ButtonState.Released;
            }
        }
        public bool MiddleMouseReleased
        {
            get
            {
                return mouseState.MiddleButton == ButtonState.Released && lastMouseState.MiddleButton == ButtonState.Pressed;
            }
        }

        public int ScrollDelta
        {
            get
            {
                if (mouseState.ScrollWheelValue > lastMouseState.ScrollWheelValue)
                    return 1;
                if (mouseState.ScrollWheelValue < lastMouseState.ScrollWheelValue)
                    return -1;
                return 0;
            }
        }
        public int ScrollDeltaXNA
        {
            get
            {
                if (ImGui.GetIO().WantCaptureMouse)
                    return 0;
                if (mouseState.ScrollWheelValue > lastMouseState.ScrollWheelValue)
                    return 1;
                if (mouseState.ScrollWheelValue < lastMouseState.ScrollWheelValue)
                    return -1;
                return 0;
            }
        }
        public int ScrollDeltaImGui
        {
            get
            {
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
