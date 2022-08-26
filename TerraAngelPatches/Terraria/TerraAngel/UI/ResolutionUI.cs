using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;
using ReLogic.OS;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
namespace TerraAngel.UI
{
    public class ResolutionUI : UIState, IHaveBackButtonCommand
    {
        private UIElement element;
        private UIAutoScaleTextTextPanel<string> backButton;
        private UIAutoScaleTextTextPanel<string> changeStateButton;
        private UITextBox widthTextBox;
        private UITextBox heightTextBox;
        private UIText widthText;
        private UIText heightText;

        public void HandleBackButtonUsage()
        {
            Terraria.Main.menuMode = MenuID.VideoSettings;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        char c = '\0';

        public override void OnInitialize()
        {
            TextInputEXT.TextInput += (x) =>
            {
                c = x;
            };

            element = new UIElement
            {
                Width = { Percent = 0.8f },
                MaxWidth = new StyleDimension(600, 0),
                Top = { Pixels = 220 },
                Height = { Pixels = -220, Percent = 1f },
                HAlign = 0.5f
            };

            backButton = new UIAutoScaleTextTextPanel<string>("Back")
            {
                Width = new StyleDimension(-10f, 1f / 3f),
                Height = { Pixels = 40 },
                Top = { Pixels = -65 },
                BackgroundColor = UIUtil.ButtonColor * 0.98f,
                VAlign = 1f,
                HAlign = 0.5f
            }.WithFadedMouseOver();

            backButton.OnClick += (x, y) => HandleBackButtonUsage();


            widthTextBox = new UITextBox(Main.screenWidth.ToString())
            {
                HAlign = 0.7f,
                VAlign = 0.2f,
                BackgroundColor = UIUtil.ButtonColor * 0.98f,
                MinWidth = { Percent = 0.2f },
                Width = { Percent = 0.2f },
            }.WithFadedMouseOver();
            widthTextBox.ShowInputTicker = false;
            widthTextBox.OnClick += (x, y) =>
            {
                widthTextBox.ShowInputTicker = true;
                heightTextBox.ShowInputTicker = false;

            };
            widthText = new UIText("Width:", 1.2f)
            {
                HAlign = 0.35f,
                VAlign = 0.206f,
            };

            heightTextBox = new UITextBox(Main.screenHeight.ToString())
            {
                HAlign = 0.7f,
                VAlign = 0.2f,
                Top = { Pixels = 50 },
                BackgroundColor = UIUtil.ButtonColor * 0.98f,
                MinWidth = { Percent = 0.2f },
                Width = { Percent = 0.2f },
            }.WithFadedMouseOver();
            heightTextBox.OnClick += (x, y) =>
            {
                widthTextBox.ShowInputTicker = false;
                heightTextBox.ShowInputTicker = true;
            };
            heightTextBox.ShowInputTicker = false;
            heightText = new UIText("Height:", 1.2f)
            {
                HAlign = 0.35f,
                VAlign = 0.206f,
                Top = { Pixels = 50 },
            };


            changeStateButton = new UIAutoScaleTextTextPanel<string>($"Go {GetNextStateString()}")
            {
                Width = new StyleDimension(-10f, 1f / 2.75f),
                Height = { Pixels = 40 },
                Top = { Pixels = -65 },
                BackgroundColor = UIUtil.ButtonColor * 0.98f,
                VAlign = 0.475f,
                HAlign = 0.5f,

            }.WithFadedMouseOver();

            changeStateButton.OnClick += (x, y) =>
            {
                SetState(GetNextState());
                changeStateButton.SetText($"Go {GetNextStateString()}");
            };

            element.Append(widthTextBox);
            element.Append(widthText);
            element.Append(heightTextBox);
            element.Append(heightText);
            element.Append(backButton);
            element.Append(changeStateButton);

            OnClick += (x, y) =>
            {
                TrySetReso();
                if (x.Target == widthTextBox || x.Target == heightTextBox)
                    return;
                widthTextBox.ShowInputTicker = false;
                heightTextBox.ShowInputTicker = false;
            };

            Append(element);
        }

        Regex onlyNumbers = new Regex(@"^\d$");

        public string GetInputText(string old)
        {
            if (!Main.hasFocus)
                return old;

            if ((InputSystem.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || InputSystem.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl)) && InputSystem.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.V))
            {
                old += Platform.Get<IClipboard>().Value;
            }

            if (c == '\x8')
            {
                if (old.Length > 0)
                {
                    old = old.Remove(old.Length - 1);
                    c = '\0';
                }
            }
            else if (c != '\0')
            {
                old += c;
                c = '\0';
            }

            return old;
        }
        public void TrySetReso()
        {
            if (int.TryParse(widthTextBox.Text, out int w) && int.TryParse(heightTextBox.Text, out int h))
            {
                widthTextBox.SetText((w = Utils.Clamp(w, Main.minScreenW, 8192)).ToString());
                heightTextBox.SetText((h = Utils.Clamp(h, Main.minScreenH, 8192)).ToString());

                SDL2.SDL.SDL_SetWindowSize(Main.instance.Window.Handle, w, h);

                Main.SetResolution(w, h);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (widthTextBox.ShowInputTicker)
            {
                widthTextBox.SetText(GetInputText(widthTextBox.Text));



                if (InputSystem.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
                {
                    heightTextBox.ShowInputTicker = false;
                    TrySetReso();
                }
            }
            else
            {
                widthTextBox.SetText(Main.graphics.PreferredBackBufferWidth.ToString());
            }

            if (heightTextBox.ShowInputTicker)
            {
                heightTextBox.SetText(GetInputText(heightTextBox.Text));


                if (InputSystem.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
                {
                    heightTextBox.ShowInputTicker = false;
                    TrySetReso();
                }
            }
            else
            {
                heightTextBox.SetText(Main.graphics.PreferredBackBufferHeight.ToString());
            }

            if (InputSystem.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                HandleBackButtonUsage();
        }

        // windowed = 0
        // borderless windows = 1
        // fullscreen = 2
        public int GetNextState()
        {
            if (!Main.graphics.IsFullScreen && !Main.screenBorderless)
                return 1;
            if (Main.graphics.IsFullScreen)
                return 0;
            if (Main.screenBorderless)
                return 2;
            return 0;
        }

        public string GetNextStateString()
        {
            return GetNextState() == 0 ? "Windowed" : (GetNextState() == 1 ? "Borderless Fullscreen" : "Fullscreen");
        }

        public void SetState(int state)
        {
            if (state == 0)
            {
                Main.screenBorderless = false;
                Main.graphics.IsFullScreen = false;
                Main.SetResolution(Main.screenWidth, Main.screenHeight);
            }
            if (state == 1)
            {
                SDL2.SDL.SDL_GetDisplayBounds(SDL2.SDL.SDL_GetWindowDisplayIndex(Main.instance.Window.Handle), out SDL2.SDL.SDL_Rect bounds);
                Main.screenBorderless = true;
                Main.graphics.IsFullScreen = false;
                Main.graphics.PreferredBackBufferWidth = Main.screenWidth = bounds.w;
                Main.graphics.PreferredBackBufferHeight = Main.screenHeight = bounds.h;
                Main.screenBorderlessPendingResizes = 1;
                Main.SetResolution(bounds.w, bounds.h);
            }
            if (state == 2)
            {
                SDL2.SDL.SDL_GetDisplayBounds(SDL2.SDL.SDL_GetWindowDisplayIndex(Main.instance.Window.Handle), out SDL2.SDL.SDL_Rect bounds);
                Main.screenBorderless = false;
                Main.graphics.IsFullScreen = false;
                Main.graphics.PreferredBackBufferWidth = Main.screenWidth = bounds.w;
                Main.graphics.PreferredBackBufferHeight = Main.screenHeight = bounds.h;
                Main.SetDisplayMode(bounds.w, bounds.h, true);
            }
        }
    }
}
