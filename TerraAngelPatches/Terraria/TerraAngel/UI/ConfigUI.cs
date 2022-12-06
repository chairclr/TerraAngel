using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace TerraAngel.UI;

public class ConfigUI : UIState, IHaveBackButtonCommand
{
    private UIElement element;
    private UIPanel panel;
    private UIList settingsList;
    private List<UIElement> settingsTextList = new List<UIElement>();
    private UIAutoScaleTextTextPanel<string> backButton;

    public bool NeedsUpdate = false;

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

        panel = new UIPanel
        {
            Width = { Percent = 1f },
            Height = { Pixels = -110, Percent = 1f },
            BackgroundColor = UIUtil.BGColor * 0.98f,
            PaddingTop = 0f
        };

        element.Append(panel);



        settingsList = new UIList
        {
            Width = { Pixels = -25, Percent = 1f },
            Height = { Percent = 1f },
            Top = { Pixels = 8 },
            ListPadding = 5f
        };

        settingsList.ManualSortMethod = (x) => { };

        panel.Append(settingsList);

        UIScrollbar settingsScrollbar = new UIScrollbar
        {
            Height = { Pixels = -12f, Percent = 1f },
            Top = { Pixels = 12 },
            HAlign = 1f,
            BarColor = UIUtil.ScrollbarColor,
        }.WithView(100f, 1000f);

        panel.Append(settingsScrollbar);

        settingsList.SetScrollbar(settingsScrollbar);



        backButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back"))
        {
            Width = new StyleDimension(0f, 1f),
            Height = { Pixels = 40 },
            Top = { Pixels = -65 },
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            VAlign = 1f,
        }.WithFadedMouseOver();

        backButton.OnLeftClick += BackButton;

        element.Append(backButton);

        Append(element);
    }

    public override void OnActivate()
    {
        base.OnActivate();

        backButton.BackgroundColor = UIUtil.ButtonColor * 0.98f;
        NeedsUpdate = true;

    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        if (InputSystem.IsKeyDown(Keys.Escape))
            ((IHaveBackButtonCommand)this).HandleBackButtonUsage();
    }

    public override void Update(GameTime gameTime)
    {
        if (NeedsUpdate)
        {
            NeedsUpdate = false;
            settingsList.Clear();
            settingsTextList.Clear();
            settingsTextList = ClientConfig.Settings.GetUITexts();

            for (int i = 0; i < settingsTextList.Count; i++)
            {
                UIElement text = settingsTextList[i];
                settingsList.Add(text);
            }
        }

        base.Update(gameTime);

    }

    void BackButton(UIMouseEvent evt, UIElement listeningElement)
    {
        ((IHaveBackButtonCommand)this).HandleBackButtonUsage();
    }

    void IHaveBackButtonCommand.HandleBackButtonUsage()
    {
        Terraria.Main.menuMode = 0;
        SoundEngine.PlaySound(SoundID.MenuClose);
    }
}
