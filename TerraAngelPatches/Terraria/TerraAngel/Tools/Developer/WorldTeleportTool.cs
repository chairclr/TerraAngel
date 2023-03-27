namespace TerraAngel.Tools.Developer;

public class WorldTeleportTool : Tool
{
    public override void Update()
    {
        if (InputSystem.IsKeyPressed(ClientConfig.Settings.TeleportToCursor))
        {
            Main.LocalPlayer.velocity = Vector2.Zero;
            Main.LocalPlayer.Teleport(Util.ScreenToWorldWorld(InputSystem.MousePosition) - new Vector2(Main.LocalPlayer.width / 2f, Main.LocalPlayer.height), TeleportationStyleID.RodOfDiscord);

            NetMessage.SendData(MessageID.PlayerControls, number: Main.myPlayer);

            if (ClientConfig.Settings.TeleportSendRODPacket)
            {
                NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null,
                    0,
                    Main.LocalPlayer.whoAmI,
                    Main.LocalPlayer.position.X,
                    Main.LocalPlayer.position.Y,
                    TeleportationStyleID.RodOfDiscord);
            }
        }
    }
}
