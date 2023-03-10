using System;
using TerraAngel.Net;
using Terraria.DataStructures;

namespace TerraAngel.Hooks;

public class PlayerHooks
{
    public static void PlayerItemCheckHook(Action<Player, int> orig, Player self, int i)
    {
        if (self.whoAmI == Main.myPlayer && ToolManager.GetTool<AutoAimTool>().Enabled)
        {

            int mx = Main.mouseX;
            int my = Main.mouseY;


            if (ToolManager.GetTool<AutoAimTool>().LockedOnToTarget)
            {
                Vector2 point = Util.WorldToScreenWorld(ToolManager.GetTool<AutoAimTool>().TargetPoint);

                ToolManager.GetTool<AutoAimTool>().LockedOnToTarget = false;
                Main.mouseX = (int)point.X;
                Main.mouseY = (int)point.Y;
            }

            orig(self, i);

            Main.mouseX = mx;
            Main.mouseY = my;

            return;
        }
        orig(self, i);
    }
    public static int presenceUpdateCount = 0;
    public static void PlayerResetEffectsHook(Action<Player> orig, Player self)
    {
        orig(self);

        if (self.active && !self.dead)
        {
            self.legPosition = Vector2.Zero;
            self.headPosition = Vector2.Zero;
            self.bodyPosition = Vector2.Zero;

            self.legRotation = 0f;
            self.headRotation = 0f;
            self.bodyRotation = 0f;
        }

        if (self.whoAmI == Main.myPlayer)
        {
            if (ToolManager.GetTool<AntiHurtTool>().Enabled)
            {
                self.statLife = self.statLifeMax2;
            }

            if (ToolManager.GetTool<InfiniteMinionTool>().Enabled)
            {
                self.maxMinions = int.MaxValue - 100000;
            }

            if (ToolManager.GetTool<InfiniteManaTool>().Enabled)
            {
                self.statMana = self.statLifeMax2;
                self.manaCost = 0.0f;
            }

            if (ToolManager.GetTool<InfiniteReachTool>().Enabled)
            {
                Player.tileRangeX = Main.screenWidth / 32 + 8;
                Player.tileRangeY = Main.screenHeight / 32 + 8;
            }

            if (ToolManager.GetTool<AntiHurtTool>().FramesSinceLastLifePacket > 0)
                ToolManager.GetTool<AntiHurtTool>().FramesSinceLastLifePacket--;

            NoClipTool noClip = ToolManager.GetTool<NoClipTool>();
            if (noClip.Enabled)
            {
                if (Main.GameUpdateCount % noClip.NoClipPlayerSyncTime == 0)
                {
                    SpecialNetMessage.SendData(MessageID.PlayerControls, null, self.whoAmI, self.position.X, self.position.Y, (float)self.selectedItem);
                }
            }

            if (ClientConfig.Settings.BroadcastPresence)
            {
                presenceUpdateCount++;
                if (presenceUpdateCount == 60)
                {
                    NetMessage.SendPlayerHurt(self.whoAmI, PlayerDeathReason.ByNPC(203), 1, 0, false, false, 0);
                    NetMessage.SendData(MessageID.PlayerLife, -1, -1, null, self.whoAmI);
                }
            }
        }
    }

}
