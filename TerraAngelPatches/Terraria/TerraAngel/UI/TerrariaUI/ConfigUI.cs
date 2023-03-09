using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace TerraAngel.UI.TerrariaUI;

public class ConfigUI : UIState, IHaveBackButtonCommand
{
    public readonly UIElement RootElement;

    public readonly UIPanel BackgroundPanel;

    public readonly UIList SettingsListContainer;

    public readonly UIAutoScaleTextTextPanel<string> BackButton;

    public List<UIElement> SettingsElementList = new List<UIElement>();

    public bool NeedsUpdate = false;

    public ConfigUI()
    {
        RootElement = new UIElement
        {
            Width = { Percent = 0.8f },
            MaxWidth = { Pixels = 600, Percent = 0f, },
            Top = { Pixels = 220 },
            Height = { Pixels = -220, Percent = 1f },
            HAlign = 0.5f
        };

        BackgroundPanel = new UIPanel
        {
            Width = { Percent = 1f },
            Height = { Pixels = -110, Percent = 1f },
            BackgroundColor = UIUtil.BGColor * 0.98f,
            PaddingTop = 0f
        };

        RootElement.Append(BackgroundPanel);

        SettingsListContainer = new UIList
        {
            Width = { Pixels = -25, Percent = 1f },
            Height = { Percent = 1f },
            Top = { Pixels = 8 },
            ListPadding = 5f
        };

        SettingsListContainer.ManualSortMethod = (x) => { };

        BackgroundPanel.Append(SettingsListContainer);

        UIScrollbar settingsScrollbar = new UIScrollbar
        {
            Height = { Pixels = -12f, Percent = 1f },
            Top = { Pixels = 12 },
            HAlign = 1f,
            BarColor = UIUtil.ScrollbarColor,
        }.WithView(100f, 1000f);

        BackgroundPanel.Append(settingsScrollbar);

        SettingsListContainer.SetScrollbar(settingsScrollbar);

        BackButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back"))
        {
            Width = new StyleDimension(0f, 1f),
            Height = { Pixels = 40 },
            Top = { Pixels = -65 },
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            VAlign = 1f,
        }.WithFadedMouseOver();

        BackButton.OnLeftClick += (x, y) =>
        {
            HandleBackButtonUsage();
        };

        RootElement.Append(BackButton);
    }

    public override void OnInitialize()
    {
        Append(RootElement);
    }

    public override void OnActivate()
    {
        BackButton.BackgroundColor = UIUtil.ButtonColor * 0.98f;
        NeedsUpdate = true;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        if (InputSystem.IsKeyDown(Keys.Escape))
            HandleBackButtonUsage();
    }

    public override void Update(GameTime gameTime)
    {
        if (NeedsUpdate)
        {
            NeedsUpdate = false;
            SettingsListContainer.Clear();
            SettingsElementList.Clear();
            SettingsElementList = ClientConfig.Settings.GetUITexts();

            for (int i = 0; i < SettingsElementList.Count; i++)
            {
                UIElement text = SettingsElementList[i];
                SettingsListContainer.Add(text);
            }
        }

        base.Update(gameTime);

    }

    public void HandleBackButtonUsage()
    {
        Main.menuMode = 0;
        SoundEngine.PlaySound(SoundID.MenuClose);
    }
}
