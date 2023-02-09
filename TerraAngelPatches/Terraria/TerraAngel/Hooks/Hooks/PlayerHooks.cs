using System;
using TerraAngel.Net;
using Terraria.DataStructures;

namespace TerraAngel.Hooks.Hooks;

public class PlayerHooks
{
    public static void PlayerItemCheckHook(Action<Player, int> orig, Player self, int i)
    {
        if (self.whoAmI == Main.myPlayer && CringeManager.GetCringe<AutoAimCringe>().Enabled)
        {

            int mx = Main.mouseX;
            int my = Main.mouseY;


            if (CringeManager.GetCringe<AutoAimCringe>().LockedOnToTarget)
            {
                Vector2 point = Util.WorldToScreenWorld(CringeManager.GetCringe<AutoAimCringe>().TargetPoint);

                CringeManager.GetCringe<AutoAimCringe>().LockedOnToTarget = false;
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
            if (CringeManager.GetCringe<AntiHurtCringe>().Enabled)
            {
                self.statLife = self.statLifeMax2;
            }

            if (CringeManager.GetCringe<InfiniteMinionCringe>().Enabled)
            {
                self.maxMinions = int.MaxValue - 100000;
            }

            if (CringeManager.GetCringe<InfiniteManaCringe>().Enabled)
            {
                self.statMana = self.statLifeMax2;
                self.manaCost = 0.0f;
            }

            if (CringeManager.GetCringe<InfiniteReachCringe>().Enabled)
            {
                Player.tileRangeX = Main.screenWidth / 32 + 8;
                Player.tileRangeY = Main.screenHeight / 32 + 8;
            }

            if (CringeManager.GetCringe<AntiHurtCringe>().FramesSinceLastLifePacket > 0)
                CringeManager.GetCringe<AntiHurtCringe>().FramesSinceLastLifePacket--;

            NoClipCringe noClip = CringeManager.GetCringe<NoClipCringe>();
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
