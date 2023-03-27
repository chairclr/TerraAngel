using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace TerraAngel.UI.TerrariaUI;

public class UIInputText : UITextPanel<string>
{
    public static UIInputText? EditingInputText;

    public int CursorPosition;

    private int FrameCount = 0;

    private char[]? FilterChars;

    private bool AllLowercase;

    public UIInputText(string defaultText, char[]? filterChars = null, bool allLowercase = false)
        : base(defaultText, 0.5f, true)
    {
        FilterChars = filterChars;

        CursorPosition = Text.Length;

        AllLowercase = allLowercase;

        TextInputEXT.TextInput += x =>
        {
            if (EditingInputText == this)
            {
                if (x == '\b')
                {
                    CursorPosition--;
                    CursorPosition = Math.Clamp(CursorPosition, 0, Text.Length);
                    if (Text.Length > 0)
                    {
                        SetText(Text.Remove(Math.Clamp(CursorPosition, 0, Text.Length)));
                        FrameCount = 10;
                    }
                }
                else if (x == '\u0016')
                {
                    string s = ImGui.GetClipboardText();

                    if (AllLowercase)
                    {
                        s = s.ToLowerInvariant();
                    }

                    if (FilterChars is not null)
                    {
                        foreach (char c in s)
                        {
                            if (!FilterChars.Contains(c))
                            {
                                return;
                            }
                        }

                        SetText(Text.Insert(Math.Clamp(CursorPosition, 0, Text.Length), s.ToString()));
                        CursorPosition += s.Length;
                        CursorPosition = Math.Clamp(CursorPosition, 0, Text.Length);
                        FrameCount = 10;
                    }
                    else
                    {
                        SetText(Text.Insert(Math.Clamp(CursorPosition, 0, Text.Length), s.ToString()));
                        CursorPosition += s.Length;
                        CursorPosition = Math.Clamp(CursorPosition, 0, Text.Length);
                        FrameCount = 10;
                    }
                }
                else
                {
                    if (AllLowercase)
                    {
                        x = char.ToLowerInvariant(x);
                    }

                    if (FilterChars is not null)
                    {
                        if (FilterChars.Contains(x))
                        {
                            CursorPosition++;
                            SetText(Text.Insert(Math.Clamp(CursorPosition, 0, Text.Length), x.ToString()));
                            CursorPosition = Math.Clamp(CursorPosition, 0, Text.Length);
                            FrameCount = 10;
                        }
                    }
                    else
                    {
                        CursorPosition++;
                        SetText(Text.Insert(Math.Clamp(CursorPosition, 0, Text.Length), x.ToString()));
                        CursorPosition = Math.Clamp(CursorPosition, 0, Text.Length);
                        FrameCount = 10;
                    }
                }
            }
        };

        base.OnLeftClick += (x, y) =>
        {
            EditingInputText = this;
        };
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        if (EditingInputText == this)
        {
            FrameCount++;
            if ((FrameCount %= 40) <= 20)
            {
                CalculatedStyle innerDimensions = GetInnerDimensions();
                Vector2 pos = innerDimensions.Position();
                Vector2 vector = new Vector2((IsLarge ? FontAssets.DeathText.Value : FontAssets.MouseText.Value).MeasureString(Text.Substring(0, CursorPosition)).X, IsLarge ? 32f : 16f) * TextScale;
                if (IsLarge)
                {
                    pos.Y -= 8f * TextScale;
                }
                else
                {
                    pos.Y -= 2f * TextScale;
                }

                pos.X += (innerDimensions.Width - TextSize.X) * TextHAlign + vector.X - (IsLarge ? 8f : 4f) * TextScale;
                if (IsLarge)
                {
                    Utils.DrawBorderStringBig(spriteBatch, "|", pos, TextColor, TextScale);
                }
                else
                {
                    Utils.DrawBorderString(spriteBatch, "|", pos, TextColor, TextScale);
                }
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (EditingInputText == this)
        {
            if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow, true))
            {
                CursorPosition--;
                FrameCount = 10;
            }

            if (ImGui.IsKeyPressed(ImGuiKey.RightArrow, true))
            {
                CursorPosition++;
                FrameCount = 10;
            }

            CursorPosition = Math.Clamp(CursorPosition, 0, Text.Length);
        }
    }

    public static void UUpdate()
    {
        if (Main.gameMenu && InputSystem.LeftMouseDown)
        {
            EditingInputText = null;
        }
    }
}