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

    private UIPanel? changeFramerate;
    private UIText? framerateText;
    private UIColoredSlider? framerateSlider;
    private float framerateValue = 0f;

    private UITextCheckbox? vsyncCheckbox;

    private UIPanel? changeLightingPasses;
    private UIText? lightingPassesText;
    private UIColoredSlider? lightingPassesSlider;
    private float lightingPassesValue = 0f;

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

        backButton.OnClick += (x, y) => HandleBackButtonUsage();

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
        changeStateButton.OnClick += (x, y) =>
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
        resolutionLeftText.OnClick += (x, y) =>
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
        resolutionRightText.OnClick += (x, y) => 
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


        if (!ClientLoader.WindowManager!.CapFPS) framerateValue = 1f;
        else
        {
            framerateValue = Util.Lerp(0f, 1f, (ClientLoader.WindowManager!.FPSCap - 30f) / 300f);
        }

        changeFramerate = new UIPanel()
        {
            Width = { Pixels = -10, Percent = 0.6f },
            Height = { Pixels = 40 },
            BorderColor = Color.Black,
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            Top = { Pixels = -90 },
            VAlign = 0.5f,
            HAlign = 0.5f,
        };
        framerateText = new UIText($"FPS Cap: {GetFramerateText()}")
        {
            HAlign = 0f,
            VAlign = 0.5f,
        };
        framerateSlider = new UIColoredSlider(Terraria.Localization.LocalizedText.Empty, () => framerateValue, UpdateFramerateValue, () => { }, (x) => Color.White, Color.White)
        {
            HAlign = 1f,
            VAlign = -0.5f,
            Width = { Percent = 0.6f },
        };

        changeFramerate.Append(framerateText);
        changeFramerate.Append(framerateSlider);


        vsyncCheckbox = new UITextCheckbox("Vsync", () => ClientLoader.WindowManager.Vsync, x => ClientLoader.WindowManager.Vsync = x, UIUtil.ButtonColor * 0.98f, 1f)
        {
            Width = { Pixels = -10, Percent = 0.6f },
            Height = { Pixels = 40 },
            Top = { Pixels = -140 },
            VAlign = 0.5f,
            HAlign = 0.5f,
            Colorize = false,
        };


        changeLightingPasses = new UIPanel()
        {
            Width = { Pixels = -10, Percent = 0.6f },
            Height = { Pixels = 40 },
            BorderColor = Color.Black,
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            Top = { Pixels = -190 },
            VAlign = 0.5f,
            HAlign = 0.5f,
        };
        lightingPassesText = new UIText($"Light Passes: {GetLightingPassesText()}")
        {
            HAlign = 0f,
            VAlign = 0.5f,
        };
        lightingPassesSlider = new UIColoredSlider(Terraria.Localization.LocalizedText.Empty, () => lightingPassesValue, UpdateLightingPassesValue, () => { }, (x) => Color.White, Color.White)
        {
            HAlign = 1f,
            VAlign = -0.5f,
            Width = { Percent = 0.4f },
        };

        changeLightingPasses.Append(lightingPassesText);
        changeLightingPasses.Append(lightingPassesSlider);

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

    public void UpdateFramerateValue(float x)
    {
        framerateValue = x;
        if (x >= 1f)
        {
            ClientLoader.WindowManager!.CapFPS = false;
        }
        else
        {
            ClientLoader.WindowManager!.CapFPS = true;
            ClientLoader.WindowManager!.FPSCap = (int)MathF.Round(Util.Lerp(30, 200, x));
        }

        framerateText!.SetText($"FPS Cap: {GetFramerateText()}");
    }

    public void UpdateLightingPassesValue(float x)
    {
        lightingPassesValue = x;

        Lighting.NewEngine.BlurPassCount = ((int)MathF.Round(Util.Lerp(1, 8, x)));

        lightingPassesText!.SetText($"Light Passes: {GetLightingPassesText()}");
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
