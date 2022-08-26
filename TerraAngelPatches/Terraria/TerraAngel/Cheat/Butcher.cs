using System;
using TerraAngel.Net;
using Terraria.DataStructures;

namespace TerraAngel.Cheat
{
    public class Butcher
    {
        public static void ButcherAllHostileNPCs(int damage = 1000, int hitCount = -1)
        {
            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.type != NPCID.TargetDummy)
                {
                    ButcherNPC(npc, damage, hitCount);
                }
            }
        }
        public static void ButcherAllFriendlyNPCs(int damage = 1000, int hitCount = -1)
        {
            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.friendly && npc.type != NPCID.TargetDummy)
                {
                    ButcherNPC(npc, damage, hitCount);
                }
            }
        }
        public static void ButcherAllNPCs(int damage = 1000, int hitCount = -1)
        {
            ButcherAllHostileNPCs(damage, hitCount);
            ButcherAllFriendlyNPCs(damage, hitCount);
        }
        public static void ButcherNPC(NPC npc, int damage = 1000, int hitCount = -1)
        {
            int trueHitCount = hitCount;
            if (hitCount == -1)
            {
                trueHitCount = (int)Math.Ceiling((float)(npc.life + (int)Math.Ceiling(npc.defense / 2f)) / damage);
            }
            SpecialNetMessage.SendData(MessageID.PlayerControls, null, Main.myPlayer, npc.position.X, npc.position.Y, (float)Main.LocalPlayer.selectedItem);
            for (int j = 0; j < trueHitCount; j++)
            {
                npc.StrikeNPCNoInteraction(damage, 0f, 0, crit: true, noEffect: false, fromNet: false);
                NetMessage.SendData(28, -1, -1, null, npc.whoAmI, damage, 1, 1, 1, 1, 1);
            }
            NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.myPlayer);
        }

        public static void ButcherAllPlayers(int damage = 1000, int hitCount = -1)
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                Player player = Main.player[i];
                if (player.whoAmI != Main.myPlayer && player.hostile)
                {
                    ButcherPlayer(player, damage, hitCount);
                }
            }
        }
        public static void ButcherPlayer(Player player, int damage = 1000, int hitCount = -1)
        {
            int trueHitCount = hitCount;
            if (hitCount == -1)
            {
                trueHitCount = (int)Math.Ceiling((float)(player.statLife + player.statDefense / 2) / (float)damage);
            }
            SpecialNetMessage.SendData(MessageID.PlayerControls, null, Main.myPlayer, player.position.X, player.position.Y, (float)Main.LocalPlayer.selectedItem);
            for (int j = 0; j < trueHitCount; j++)
            {
                NetMessage.SendPlayerHurt(player.whoAmI, PlayerDeathReason.ByPlayer(Main.myPlayer), damage, 0, critical: true, pvp: true, 0);
            }
            NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.myPlayer);
        }
    }
}
