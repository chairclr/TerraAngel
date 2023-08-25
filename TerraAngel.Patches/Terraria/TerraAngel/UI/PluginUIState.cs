using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace TerraAngel.UI;

internal class PluginUIState : UIState, IHaveBackButtonCommand
{
    public readonly UIElement RootElement;

    public PluginUIState()
    {
        RootElement = new UIElement
        {
            Width = { Percent = 0.8f },
            MaxWidth = new StyleDimension(600, 0),
            Top = { Pixels = 220 },
            Height = { Pixels = -220, Percent = 1f },
            HAlign = 0.5f
        };

        RootElement.Append(new UIPanel()
        {
            Width = { Percent = 1f },
            Height = { Pixels = -110, Percent = 1f },
            BackgroundColor = UIColors.BackgroundColor * 0.98f
        });

        UITextPanel<string> backButton = new UITextPanel<string>(Language.GetTextValue("UI.Back"))
        {
            Width = new StyleDimension(-5f, 1f / 3f),
            Height = { Pixels = 40 },
            Top = { Pixels = -65 },
            BackgroundColor = UIColors.BackgroundColor * 0.98f,
            VAlign = 1f,
            HAlign = 0.0f
        }.WithMouseEffects();

        backButton.OnLeftClick += (x, y) =>
        {
            HandleBackButtonUsage();
        };

        RootElement.Append(backButton);

        UITextPanel<string> reloadPluginsButton = new UITextPanel<string>("Reload Plugins")
        {
            Width = new StyleDimension(-5f, 1f / 3f),
            Height = { Pixels = 40 },
            Top = { Pixels = -65 },
            BackgroundColor = UIColors.BackgroundColor * 0.98f,
            VAlign = 1f,
            HAlign = 0.5f,
        }.WithMouseEffects();

        reloadPluginsButton.OnLeftClick += (x, y) =>
        {
            Console.WriteLine("Reload Plugins");
        };

        RootElement.Append(reloadPluginsButton);

        UITextPanel<string> openPluginsFolderButton = new UITextPanel<string>("Open Plugins Folder")
        {
            Width = new StyleDimension(-5f, 1f / 3f),
            Height = { Pixels = 40 },
            Top = { Pixels = -65 },
            BackgroundColor = UIColors.BackgroundColor * 0.98f,
            VAlign = 1f,
            HAlign = 1.0f
        }.WithMouseEffects();

        openPluginsFolderButton.OnLeftClick += (x, y) =>
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start("explorer.exe", PathService.PluginsRootFolder);
            }
        };

        RootElement.Append(openPluginsFolderButton);

        UITextPanel<string> openPluginsBrowserButton = new UITextPanel<string>("Open Plugins Browser")
        {
            Width = new StyleDimension(0f, 1f),
            Height = { Pixels = 40 },
            Top = { Pixels = -20 },
            BackgroundColor = UIColors.BackgroundColor * 0.98f,
            VAlign = 1f,
            HAlign = 0.5f
        }.WithMouseEffects();

        openPluginsBrowserButton.OnLeftClick += (x, y) =>
        {
            Console.WriteLine("Open Plugins Browser");
        };

        RootElement.Append(openPluginsBrowserButton);
    }

    public override void OnInitialize()
    {
        base.OnInitialize();

        Append(RootElement);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            HandleBackButtonUsage();
    }

    public void HandleBackButtonUsage()
    {
        Main.menuMode = 0;
        SoundEngine.PlaySound(SoundID.MenuClose);
    }
}
