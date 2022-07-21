using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using TerraAngel.UI;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.ID;
using TerraAngel.Input;
using System.Diagnostics;
using TerraAngel;
using TerraAngel.Client.Config;

namespace TerraAngel.Plugin
{
	public class PluginUI : UIState, IHaveBackButtonCommand
	{
		private UIElement element;
		private UIPanel panel;
		private UIList pluginList;
		private List<UIElement> pluginObjectList = new List<UIElement>();
		private UIAutoScaleTextTextPanel<string> backButton;
		private UIAutoScaleTextTextPanel<string> reloadPluginsButton;
		private UIAutoScaleTextTextPanel<string> openPluginsFolderButton;

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



			pluginList = new UIList
			{
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f },
				Top = { Pixels = 8 },
				ListPadding = 5f
			};

			panel.Append(pluginList);

			UIScrollbar settingsScrollbar = new UIScrollbar
			{
				Height = { Pixels = -12f, Percent = 1f },
				Top = { Pixels = 12 },
				HAlign = 1f,
				BarColor = UIUtil.ScrollbarColor,
			}.WithView(100f, 1000f);

			panel.Append(settingsScrollbar);

			pluginList.SetScrollbar(settingsScrollbar);



			backButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back"))
			{
				Width = new StyleDimension(-10f, 1f / 3f),
				Height = { Pixels = 40 },
				Top = { Pixels = -65 },
				BackgroundColor = UIUtil.ButtonColor * 0.98f,
				VAlign = 1f,
			}.WithFadedMouseOver();

			backButton.OnClick += BackButton;

			reloadPluginsButton = new UIAutoScaleTextTextPanel<string>("Reload Plugins")
			{
				Width = new StyleDimension(-10f, 1f / 3f),
				Height = { Pixels = 40 },
				Top = { Pixels = -65 },
				BackgroundColor = UIUtil.ButtonColor * 0.98f,
				VAlign = 1f,
				HAlign = 0.5f
			}.WithFadedMouseOver();

			reloadPluginsButton.OnClick += ReloadButton;

			openPluginsFolderButton = new UIAutoScaleTextTextPanel<string>("Open Plugins Folder")
			{
				Width = new StyleDimension(-10f, 1f / 3f),
				Height = { Pixels = 40 },
				Top = { Pixels = -65 },
				BackgroundColor = UIUtil.ButtonColor * 0.98f,
				VAlign = 1f,
				HAlign = 1f
			}.WithFadedMouseOver();

			openPluginsFolderButton.OnClick += OpenButton;


			element.Append(backButton);
			element.Append(reloadPluginsButton);
			element.Append(openPluginsFolderButton);


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
				pluginList.Clear();
				pluginObjectList.Clear();

				pluginObjectList = PluginLoader.GetPluginUIObjects();

				foreach (UIElement text in pluginObjectList)
				{
					pluginList.Add(text);
				}
			}

			base.Update(gameTime);

		}

		void BackButton(UIMouseEvent evt, UIElement listeningElement)
		{
			HandleBackButtonUsage();
		}
		public void HandleBackButtonUsage()
		{
			Terraria.Main.menuMode = 0;
			SoundEngine.PlaySound(SoundID.MenuClose);
		}

		void ReloadButton(UIMouseEvent evt, UIElement listeningElement)
		{
			ClientConfig.WriteToFile(ClientLoader.Config);
			PluginLoader.UnloadPlugins();
			PluginLoader.LoadPlugins();
			NeedsUpdate = true;
		}

		void OpenButton(UIMouseEvent evt, UIElement listeningElement)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				Arguments = ClientLoader.PluginsPath,
				FileName = "explorer.exe",
			};

			Process.Start(startInfo);
		}
	}
}
