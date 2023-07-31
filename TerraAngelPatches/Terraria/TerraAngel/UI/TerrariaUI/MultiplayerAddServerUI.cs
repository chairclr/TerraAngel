using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace TerraAngel.UI.TerrariaUI;

public class MultiplayerAddServerUI : UIState, IHaveBackButtonCommand
{
    public readonly UIElement RootElement;

    public readonly UIAutoScaleTextTextPanel<string> BackButton;

    public readonly UIAutoScaleTextTextPanel<string> AcceptButton;

    public readonly UIText ServerNameText;

    public readonly UIInputText ServerNameInput;

    public readonly UIText ServerIPText;

    public readonly UIInputText ServerIPInput;

    public readonly UIText ServerPortText;

    public readonly UIInputText ServerPortInput;

    public MultiplayerAddServerUI()
    {
        RootElement = new UIElement
        {
            Width = { Percent = 0.8f },
            MaxWidth = { Pixels = 600, Percent = 0f, },
            Top = { Pixels = 110 },
            Height = { Pixels = -110, Percent = 1f },
            HAlign = 0.5f
        };

        ServerNameText = new UIText("Server Name")
        {
            HAlign = 0.0f,
            Top = { Pixels = 110 }
        };

        RootElement.Append(ServerNameText);

        ServerNameInput = new UIInputText("")
        {
            Top = { Pixels = 110 + ServerNameText.GetDimensions().Height + 10f },
            Width = { Percent = 1.0f },
            HAlign = 0f,
            BackgroundColor = UIUtil.BGColor * 0.98f,
        };

        RootElement.Append(ServerNameInput);

        ServerIPText = new UIText("Server IP")
        {
            Top = { Pixels = 240f },
            HAlign = 0.0f,
        };

        RootElement.Append(ServerIPText);

        ServerIPInput = new UIInputText("", new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '-' }, true)
        {
            Top = { Pixels = 240f + ServerIPText.GetDimensions().Height + 10f },
            Width = { Percent = 0.7f, Pixels = -5f },
            HAlign = 0f,
            BackgroundColor = UIUtil.BGColor * 0.98f,
        };

        RootElement.Append(ServerIPInput);

        ServerPortText = new UIText("Server Port")
        {
            Top = { Pixels = 240f },
            HAlign = 1.0f,
            Left = { Percent = -0.3f, Pixels = FontAssets.MouseText.Value.MeasureString("Server Port").X }
        };

        RootElement.Append(ServerPortText);

        ServerPortInput = new UIInputText("", new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }, true)
        {
            Top = { Pixels = 240f + ServerIPText.GetDimensions().Height + 10f },
            Width = { Percent = 0.3f, Pixels = -5f },
            HAlign = 1.0f,
            BackgroundColor = UIUtil.BGColor * 0.98f,
        };

        RootElement.Append(ServerPortInput);

        AcceptButton = new UIAutoScaleTextTextPanel<string>("Accept")
        {
            Width = { Percent = 1f },
            Height = { Pixels = 40 },
            Top = { Pixels = 270f + ServerPortInput.GetDimensions().Height + 10f },
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
        }.WithFadedMouseOver();

        AcceptButton.OnLeftClick += (x, y) =>
        {
            if (ServerNameInput.Text.Length < 1)
            {
                UIInputText.EditingInputText = ServerNameInput;
                return;
            }

            if (ServerIPInput.Text.Length < 1)
            {
                UIInputText.EditingInputText = ServerIPInput;
                return;
            }

            if (ServerPortInput.Text.Length < 1)
            {
                UIInputText.EditingInputText = ServerPortInput;
                return;
            }

            if (int.Parse(ServerPortInput.Text) > ushort.MaxValue)
            {
                ServerPortInput.SetText(ushort.MaxValue.ToString());
            }

            ClientConfig.Settings.MultiplayerServers.Add(new MultiplayerServerInfo()
            {
                IP = ServerIPInput.Text,
                Port = ushort.Parse(ServerPortInput.Text),
                Name = ServerNameInput.Text
            });

            ServerNameInput.SetText("");
            ServerIPInput.SetText("");
            ServerPortInput.SetText("");

            ClientLoader.MultiplayerJoinUI!.NeedsUpdate = true;

            Main.MenuUI.SetState(ClientLoader.MultiplayerJoinUI!);
            SoundEngine.PlaySound(SoundID.MenuOpen);
        };

        RootElement.Append(AcceptButton);

        BackButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back"))
        {
            Width = { Percent = 1f },
            Height = { Pixels = 40 },
            Top = { Pixels = 320f + ServerPortInput.GetDimensions().Height + 10f },
            BackgroundColor = UIUtil.ButtonColor * 0.98f,
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

    public override void Update(GameTime gameTime)
    {
        if (ServerPortInput.Text.Length > 5)
        {
            ServerPortInput.SetText(ServerPortInput.Text.Substring(0, 5));
        }

        if (ServerPortInput.Text.Length > 0 && int.Parse(ServerPortInput.Text) > ushort.MaxValue)
        {
            ServerPortInput.SetText(ushort.MaxValue.ToString());
        }

        base.Update(gameTime);
    }

    public void HandleBackButtonUsage()
    {
        ServerNameInput.SetText("");
        ServerIPInput.SetText("");
        ServerPortInput.SetText("");
        Main.MenuUI.SetState(ClientLoader.MultiplayerJoinUI!);
        SoundEngine.PlaySound(SoundID.MenuClose);
    }
}
