using System.Collections.Generic;
using System.Diagnostics;
using TerraAngel.Plugin;
using TerraAngel.UI;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace TerraAngel.UI.TerrariaUI;

public class PluginUI : UIState, IHaveBackButtonCommand
{
    public readonly UIElement RootElement;

    public readonly UIPanel PluginPanel;

    public readonly UIList PluginListContainer;

    public readonly UIAutoScaleTextTextPanel<string> BackButton;

    public readonly UIAutoScaleTextTextPanel<string> ReloadPluginsButton;

    public readonly UIAutoScaleTextTextPanel<string> OpenPluginsFolderButton;

    public List<UIElement> PluginElementList = new List<UIElement>();

    public bool NeedsUpdate = false;

    public PluginUI()
    {
        RootElement = new UIElement
        {
            Width = { Percent = 0.8f },
            MaxWidth = new StyleDimension(600, 0),
            Top = { Pixels = 220 },
            Height = { Pixels = -220, Percent = 1f },
            HAlign = 0.5f
        };

        PluginPanel = new UIPanel
        {
            Width = { Percent = 1f },
            Height = { Pixels = -110, Percent = 1f },
            BackgroundColor = UIUtil.BGColor * 0.98f,
            PaddingTop = 0f
        };

        RootElement.Append(PluginPanel);

        PluginListContainer = new UIList
        {
            Width = { Pixels = -25, Percent = 1f },
            Height = { Percent = 1f },
            Top = { Pixels = 8 },
            ListPadding = 5f
        };

        PluginPanel.Append(PluginListContainer);

        UIScrollbar settingsScrollbar = new UIScrollbar
        {
            Height = { Pixels = -12f, Percent = 1f },
            Top = { Pixels = 12 },
            HAlign = 1f,
            BarColor = UIUtil.ScrollbarColor,
        }.WithView(100f, 1000f);

        PluginPanel.Append(settingsScrollbar);

        PluginListContainer.SetScrollbar(settingsScrollbar);

        BackButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back"))
        {
            Width = new StyleDimension(-10f, 1f / 3f),
            Height = { Pixels = 40 },
            Top = { Pixels = -65 },
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            VAlign = 1f,
        }.WithFadedMouseOver();

        BackButton.OnLeftClick += (x, y) =>
        {
            HandleBackButtonUsage();
        };

        ReloadPluginsButton = new UIAutoScaleTextTextPanel<string>("Reload Plugins")
        {
            Width = new StyleDimension(-10f, 1f / 3f),
            Height = { Pixels = 40 },
            Top = { Pixels = -65 },
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            VAlign = 1f,
            HAlign = 0.5f
        }.WithFadedMouseOver();

        ReloadPluginsButton.OnLeftClick += ReloadButton;

        OpenPluginsFolderButton = new UIAutoScaleTextTextPanel<string>("Open Plugins Folder")
        {
            Width = new StyleDimension(-10f, 1f / 3f),
            Height = { Pixels = 40 },
            Top = { Pixels = -65 },
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
            VAlign = 1f,
            HAlign = 1f
        }.WithFadedMouseOver();

        OpenPluginsFolderButton.OnLeftClick += (x, y) =>
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = ClientLoader.PluginsPath
            };

            Process.Start(startInfo);
        };

        RootElement.Append(BackButton);

        RootElement.Append(ReloadPluginsButton);

        RootElement.Append(OpenPluginsFolderButton);
    }

    public override void OnInitialize()
    {
        Append(RootElement);
    }

    public override void OnActivate()
    {
        base.OnActivate();

        BackButton.BackgroundColor = UIUtil.ButtonColor * 0.98f;
        NeedsUpdate = true;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        if (InputSystem.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            HandleBackButtonUsage();
    }

    int updateCount = 0;
    public override void Update(GameTime gameTime)
    {
        updateCount++;

        if (NeedsUpdate || updateCount % 60 == 0)
        {
            NeedsUpdate = false;
            PluginListContainer.Clear();
            PluginElementList.Clear();

            PluginElementList = PluginLoader.GetPluginUIObjects();

            foreach (UIElement text in PluginElementList)
            {
                PluginListContainer.Add(text);
            }
        }

        base.Update(gameTime);

    }

    public void HandleBackButtonUsage()
    {
        Main.menuMode = 0;
        SoundEngine.PlaySound(SoundID.MenuClose);
    }

    void ReloadButton(UIMouseEvent evt, UIElement listeningElement)
    {
        ClientConfig.WriteToFile();
        PluginLoader.UnloadPlugins();
        PluginLoader.LoadAndInitializePlugins();
        NeedsUpdate = true;
    }
}
