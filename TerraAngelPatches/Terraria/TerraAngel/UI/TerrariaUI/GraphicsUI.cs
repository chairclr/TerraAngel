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

namespace TerraAngel.UI.TerrariaUI;

public class GraphicsUI : UIState, IHaveBackButtonCommand
{
    public readonly UIElement RootElement;

    public readonly UIAutoScaleTextTextPanel<string> BackButton;

    public readonly UIAutoScaleTextTextPanel<string> ChangeStateButton;

    public readonly UIPanel ResolutionPanel;

    public readonly UIText ResolutionText;

    public readonly UIText ResolutionLeftText;

    public readonly UIText ResolutionRightText;

    public readonly UITextSliderInt FramerateSlider;

    public readonly UITextSliderInt FramerateUnfocusedSlider;

    public readonly UITextCheckbox VsyncCheckbox;

    public readonly UITextSliderInt LightingPassCountSlider;

    public List<Vector2i> WindowSizeCache = new List<Vector2i>();

    public int CurrentWindowSizeIndex = 0;

    public GraphicsUI()
    {
        CacheWindowSizes();

        RootElement = new UIElement
        {
            Width = { Percent = 0.8f },
            MaxWidth = new StyleDimension(600, 0),
            Top = { Pixels = 220 },
            Height = { Pixels = -220, Percent = 1f },
            HAlign = 0.5f
        };

        BackButton = new UIAutoScaleTextTextPanel<string>("Back")
        {
            Width = new StyleDimension(-10f, 1f / 3f),
            Height = { Pixels = 40 },
            Top = { Pixels = -65 },
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            VAlign = 1f,
            HAlign = 0.5f
        }.WithFadedMouseOver();

        BackButton.OnLeftClick += (x, y) => HandleBackButtonUsage();

        ChangeStateButton = new UIAutoScaleTextTextPanel<string>($"Go {GetNextWindowStateString()}")
        {
            Width = new StyleDimension(-10f, 1f / 2.5f),
            Height = { Pixels = 40 },
            Top = { Pixels = 10f },
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            VAlign = 0.5f,
            HAlign = 0.5f,
        }.WithFadedMouseOver();

        ChangeStateButton.OnLeftClick += (x, y) =>
        {
            ClientLoader.WindowManager!.State = GetNextState();
            ChangeStateButton.SetText($"Go {GetNextWindowStateString()}");
        };

        ResolutionPanel = new UIPanel()
        {
            Width = { Pixels = -10, Percent = 0.6f },
            Height = { Pixels = 40 },
            BorderColor = Color.Black,
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            Top = { Pixels = -40 },
            VAlign = 0.5f,
            HAlign = 0.5f,
        };

        ResolutionText = new UIText($"{ClientLoader.WindowManager!.Width} x {ClientLoader.WindowManager!.Height}")
        {
            HAlign = 0.5f,
            VAlign = 0.5f
        };

        ResolutionLeftText = new UIText("<")
        {
            Width = { Pixels = 30, },
            Height = { Pixels = 40, },
            HAlign = 0f,
            VAlign = 0.5f,
        };

        ResolutionLeftText.OnMouseOver += (x, y) =>
        {
            if (IsSmallerResolutionAvailable()) ResolutionLeftText.TextColor = Color.Yellow;
            SoundEngine.PlaySound(SoundID.MenuTick);
        };

        ResolutionLeftText.OnMouseOut += (x, y) =>
        {
            ResolutionLeftText.TextColor = IsSmallerResolutionAvailable() ? Color.White : Color.Gray;
            SoundEngine.PlaySound(SoundID.MenuTick);
        };

        ResolutionLeftText.OnLeftClick += (x, y) =>
        {
            if (IsSmallerResolutionAvailable())
            {
                CurrentWindowSizeIndex--;
                ClientLoader.WindowManager!.Size = WindowSizeCache[CurrentWindowSizeIndex];
                ClientLoader.WindowManager.CenterWindow();
                ResolutionRightText!.TextColor = IsLargerResolutionAvailable() ? Color.White : Color.Gray;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            else
            {
                ResolutionLeftText.TextColor = Color.Gray;
            }
        };

        ResolutionRightText = new UIText(">")
        {
            Width = { Pixels = 30, },
            Height = { Pixels = 40, },
            HAlign = 1f,
            VAlign = 0.5f,
        };

        ResolutionRightText.OnMouseOver += (x, y) =>
        {
            if (IsLargerResolutionAvailable()) ResolutionRightText.TextColor = Color.Yellow;
            SoundEngine.PlaySound(SoundID.MenuTick);
        };

        ResolutionRightText.OnMouseOut += (x, y) =>
        {
            ResolutionRightText.TextColor = IsLargerResolutionAvailable() ? Color.White : Color.Gray;
            SoundEngine.PlaySound(SoundID.MenuTick);
        };

        ResolutionRightText.OnLeftClick += (x, y) =>
        {
            if (IsLargerResolutionAvailable())
            {
                CurrentWindowSizeIndex++;
                ClientLoader.WindowManager!.Size = WindowSizeCache[CurrentWindowSizeIndex];
                ClientLoader.WindowManager.CenterWindow();
                ResolutionLeftText.TextColor = IsSmallerResolutionAvailable() ? Color.White : Color.Gray;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            else
            {
                ResolutionRightText.TextColor = Color.Gray;
            }
        };

        ResolutionPanel.Append(ResolutionText);
        ResolutionPanel.Append(ResolutionRightText);
        ResolutionPanel.Append(ResolutionLeftText);

        FramerateSlider = new UITextSliderInt(30, 201, () => (ClientLoader.WindowManager.CapFPS ? 201 : ClientLoader.WindowManager.FPSCap), x => { if (x > 200) { ClientLoader.WindowManager.CapFPS = false; } else { ClientLoader.WindowManager.CapFPS = true; ClientLoader.WindowManager.FPSCap = x; } }, () => $"FPS Cap: {GetFramerateText()}")
        {
            Width = { Pixels = -10, Percent = 0.8f },
            Height = { Pixels = 40 },
            BorderColor = Color.Black,
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            Top = { Pixels = -90 },
            VAlign = 0.5f,
            HAlign = 0.5f,
        };

        FramerateUnfocusedSlider = new UITextSliderInt(10, 201, () => (ClientLoader.WindowManager.CapFPSUnfocused ? 201 : ClientLoader.WindowManager.FPSCapUnfocused), x => { if (x > 200) { ClientLoader.WindowManager.CapFPSUnfocused = false; } else { ClientLoader.WindowManager.CapFPSUnfocused = true; ClientLoader.WindowManager.FPSCapUnfocused = x; } }, () => $"FPS Cap Unfocused: {GetFramerateTextUnfocused()}")
        {
            Width = { Pixels = -10, Percent = 0.8f },
            Height = { Pixels = 40 },
            BorderColor = Color.Black,
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            Top = { Pixels = -140 },
            VAlign = 0.5f,
            HAlign = 0.5f
        };

        VsyncCheckbox = new UITextCheckbox("Vsync", () => ClientLoader.WindowManager.Vsync, x => ClientLoader.WindowManager.Vsync = x, 1f)
        {
            Width = { Pixels = -10, Percent = 0.8f },
            Height = { Pixels = 40 },
            Top = { Pixels = -190 },
            VAlign = 0.5f,
            HAlign = 0.5f,
            Colorize = false,
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
        };

        LightingPassCountSlider = new UITextSliderInt(1, 8, () => Lighting.NewEngine.BlurPassCount, x => { Lighting.NewEngine.BlurPassCount = x; }, () => $"Light Passes: {GetLightingPassesText()}")
        {
            Width = { Pixels = -10, Percent = 0.8f },
            Height = { Pixels = 40 },
            BorderColor = Color.Black,
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            Top = { Pixels = -240 },
            VAlign = 0.5f,
            HAlign = 0.5f,
        };

        RootElement.Append(BackButton);

        RootElement.Append(ChangeStateButton);

        RootElement.Append(ResolutionPanel);

        RootElement.Append(FramerateSlider);

        RootElement.Append(FramerateUnfocusedSlider);

        RootElement.Append(VsyncCheckbox);

        RootElement.Append(LightingPassCountSlider);
    }

    public override void OnInitialize()
    {
        Append(RootElement);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        ResolutionText!.SetText($"{ClientLoader.WindowManager!.Width} x {ClientLoader.WindowManager!.Height}");

        if (InputSystem.IsKeyDown(Keys.Escape))
            HandleBackButtonUsage();
    }

    public void CacheWindowSizes()
    {
        float mxDist = float.MaxValue;
        int i = 0;
        foreach (DisplayMode supportedDisplayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
        {
            WindowSizeCache.Add(new Vector2i(supportedDisplayMode.Width, supportedDisplayMode.Height));

            float d = ((Vector2)WindowSizeCache.Last()).Distance(ClientLoader.WindowManager!.Size);
            if (d < mxDist)
            {
                mxDist = d;
                CurrentWindowSizeIndex = i;
            }
            i++;
        }
    }

    public bool IsLargerResolutionAvailable()
    {
        if (CurrentWindowSizeIndex + 1 >= WindowSizeCache.Count)
            return false;
        return true;
    }

    public bool IsSmallerResolutionAvailable()
    {
        if (CurrentWindowSizeIndex <= 0)
            return false;
        return true;
    }

    public WindowManager.WindowState GetNextState()
    {
        return (WindowManager.WindowState)(((int)ClientLoader.WindowManager!.State + 1) % 3);
    }

    public string GetNextWindowStateString()
    {
        return GetNextState() == WindowManager.WindowState.Windowed ? "Windowed" : (GetNextState() == WindowManager.WindowState.BorderlessFullscreen ? "Borderless Fullscreen" : "Fullscreen");
    }

    public string GetFramerateText()
    {
        return ClientLoader.WindowManager!.CapFPS ? $"{ClientLoader.WindowManager!.FPSCap}" : "None";
    }

    public string GetFramerateTextUnfocused()
    {
        return ClientLoader.WindowManager!.CapFPSUnfocused ? $"{ClientLoader.WindowManager!.FPSCapUnfocused}" : "None";
    }

    public string GetLightingPassesText()
    {
        return Lighting.NewEngine.BlurPassCount.ToString();
    }

    public void HandleBackButtonUsage()
    {
        Main.menuMode = MenuID.VideoSettings;
        SoundEngine.PlaySound(SoundID.MenuClose);
    }
}
