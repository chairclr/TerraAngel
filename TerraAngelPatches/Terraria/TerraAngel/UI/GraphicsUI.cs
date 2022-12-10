using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;
using ReLogic.OS;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Graphics.Light;
using Terraria.UI;
namespace TerraAngel.UI;

public class GraphicsUI : UIState, IHaveBackButtonCommand
{
    private UIElement? element;
    private UIAutoScaleTextTextPanel<string>? backButton;
    private UIAutoScaleTextTextPanel<string>? changeStateButton;
    private UIPanel? changeResolution;
    private UIText? resolutionText;
    private UIText? resolutionLeftText;
    private UIText? resolutionRightText;

    private UITextSliderInt? changeFramerate;

    private UITextCheckbox? vsyncCheckbox;

    private UITextSliderInt? changeLightingPasses;

    private List<Vector2i> validWindowSizes = new List<Vector2i>();
    private int currentWindowSizeIndex = 0;

    public void HandleBackButtonUsage()
    {
        Main.menuMode = MenuID.VideoSettings;
        SoundEngine.PlaySound(SoundID.MenuClose);
    }

    public override void OnInitialize()
    {
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

        backButton.OnLeftClick += (x, y) => HandleBackButtonUsage();

        SetValidWindowSizes();

        changeStateButton = new UIAutoScaleTextTextPanel<string>($"Go {GetNextStateString()}")
        {
            Width = new StyleDimension(-10f, 1f / 2.5f),
            Height = { Pixels = 40 },
            Top = { Pixels = 10f },
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            VAlign = 0.5f,
            HAlign = 0.5f,
        }.WithFadedMouseOver();
        changeStateButton.OnLeftClick += (x, y) =>
        {
            ClientLoader.WindowManager!.State = GetNextState();
            changeStateButton.SetText($"Go {GetNextStateString()}");
        };

        changeResolution = new UIPanel()
        {
            Width = { Pixels = -10, Percent = 0.6f },
            Height = { Pixels = 40 },
            BorderColor = Color.Black,
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            Top = { Pixels = -40 },
            VAlign = 0.5f,
            HAlign = 0.5f,
        };
        resolutionText = new UIText($"{ClientLoader.WindowManager!.Width} x {ClientLoader.WindowManager!.Height}")
        {
            HAlign = 0.5f,
            VAlign = 0.5f
        };
        resolutionLeftText = new UIText("<")
        {
            Width = { Pixels = 30, },
            Height = { Pixels = 40, },
            HAlign = 0f,
            VAlign = 0.5f,
        };
        resolutionLeftText.OnMouseOver += (x, y) => 
        {
            if (IsSmallerResoAvailable()) resolutionLeftText.TextColor = Color.Yellow;
            SoundEngine.PlaySound(SoundID.MenuTick);
        };
        resolutionLeftText.OnMouseOut += (x, y) =>
        {
            resolutionLeftText.TextColor = IsSmallerResoAvailable() ? Color.White : Color.Gray;
            SoundEngine.PlaySound(SoundID.MenuTick);
        };
        resolutionLeftText.OnLeftClick += (x, y) =>
        {
            if (IsSmallerResoAvailable())
            {
                currentWindowSizeIndex--;
                ClientLoader.WindowManager!.Size = validWindowSizes[currentWindowSizeIndex];
                ClientLoader.WindowManager.CenterWindow();
                resolutionRightText!.TextColor = IsLargerResoAvailable() ? Color.White : Color.Gray;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            else
            {
                resolutionLeftText.TextColor = Color.Gray;
            }
        };
        resolutionRightText = new UIText(">")
        {
            Width = { Pixels = 30, },
            Height = { Pixels = 40, },
            HAlign = 1f,
            VAlign = 0.5f,
        };
        resolutionRightText.OnMouseOver += (x, y) =>
        {
            if (IsLargerResoAvailable()) resolutionRightText.TextColor = Color.Yellow;
            SoundEngine.PlaySound(SoundID.MenuTick);
        };
        resolutionRightText.OnMouseOut += (x, y) =>
        {
            resolutionRightText.TextColor = IsLargerResoAvailable() ? Color.White : Color.Gray;
            SoundEngine.PlaySound(SoundID.MenuTick);
        };
        resolutionRightText.OnLeftClick += (x, y) => 
        {
            if (IsLargerResoAvailable())
            {
                currentWindowSizeIndex++;
                ClientLoader.WindowManager!.Size = validWindowSizes[currentWindowSizeIndex];
                ClientLoader.WindowManager.CenterWindow();
                resolutionLeftText.TextColor = IsSmallerResoAvailable() ? Color.White : Color.Gray;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            else
            {
                resolutionRightText.TextColor = Color.Gray;
            }
        };

        changeResolution.Append(resolutionText);
        changeResolution.Append(resolutionRightText);
        changeResolution.Append(resolutionLeftText);

        changeFramerate = new UITextSliderInt(30, 201, () => (ClientLoader.WindowManager.CapFPS ? 201 : ClientLoader.WindowManager.FPSCap), x => { if (x > 200) { ClientLoader.WindowManager.CapFPS = false; } else { ClientLoader.WindowManager.CapFPS = true; ClientLoader.WindowManager.FPSCap = x; } }, () => $"FPS Cap: {GetFramerateText()}")
        {
            Width = { Pixels = -10, Percent = 0.8f },
            Height = { Pixels = 40 },
            BorderColor = Color.Black,
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            Top = { Pixels = -90 },
            VAlign = 0.5f,
            HAlign = 0.5f,
        };

        changeLightingPasses = new UITextSliderInt(1, 8, () => Lighting.NewEngine.BlurPassCount, x => { Lighting.NewEngine.BlurPassCount = x; }, () => $"Light Passes: {GetLightingPassesText()}")
        {
            Width = { Pixels = -10, Percent = 0.8f },
            Height = { Pixels = 40 },
            BorderColor = Color.Black,
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            Top = { Pixels = -190 },
            VAlign = 0.5f,
            HAlign = 0.5f,
        };

        vsyncCheckbox = new UITextCheckbox("Vsync", () => ClientLoader.WindowManager.Vsync, x => ClientLoader.WindowManager.Vsync = x, 1f)
        {
            Width = { Pixels = -10, Percent = 0.8f },
            Height = { Pixels = 40 },
            Top = { Pixels = -140 },
            VAlign = 0.5f,
            HAlign = 0.5f,
            Colorize = false,
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
        };


        

        element.Append(backButton);
        element.Append(changeStateButton);
        element.Append(changeResolution);
        element.Append(changeFramerate);
        element.Append(vsyncCheckbox);
        element.Append(changeLightingPasses);

        Append(element);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        resolutionText!.SetText($"{ClientLoader.WindowManager!.Width} x {ClientLoader.WindowManager!.Height}");

        if (InputSystem.IsKeyDown(Keys.Escape))
            HandleBackButtonUsage();
    }

    public void SetValidWindowSizes()
    {
        float mxDist = float.MaxValue;
        int i = 0;
        foreach (DisplayMode supportedDisplayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
        {
            validWindowSizes.Add(new Vector2i(supportedDisplayMode.Width, supportedDisplayMode.Height));

            float d = ((Vector2)validWindowSizes.Last()).Distance(ClientLoader.WindowManager!.Size);
            if (d < mxDist)
            {
                mxDist = d;
                currentWindowSizeIndex = i;
            }
            i++;
        }
    }

    public bool IsLargerResoAvailable()
    {
        if (currentWindowSizeIndex + 1 >= validWindowSizes.Count)
            return false;
        return true;
    }
    public bool IsSmallerResoAvailable()
    {
        if (currentWindowSizeIndex <= 0)
            return false;
        return true;
    }

    public WindowManager.WindowState GetNextState()
    {
        return (WindowManager.WindowState)(((int)ClientLoader.WindowManager!.State + 1) % 3);
    }

    public string GetNextStateString()
    {
        return GetNextState() == WindowManager.WindowState.Windowed ? "Windowed" : (GetNextState() == WindowManager.WindowState.BorderlessFullscreen ? "Borderless Fullscreen" : "Fullscreen");
    }

    public string GetFramerateText()
    {
        return ClientLoader.WindowManager!.CapFPS ? $"{ClientLoader.WindowManager!.FPSCap}" : "None";
    }
    public string GetLightingPassesText()
    {
        return Lighting.NewEngine.BlurPassCount.ToString();
    }
}
