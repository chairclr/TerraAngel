using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using TerraAngel.Hooks;
using Microsoft.Xna.Framework.Input;
using NVector2 = System.Numerics.Vector2;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace TerraAngel.Client.ClientWindows
{
    public class MainWindow : ClientWindow
    {
        public override Keys ToggleKey => Keys.OemTilde;

        public override bool DefaultEnabled => true;

        public override bool IsToggleable => true;

        public override string Title => "Main Window";

        private bool antiHurt = false;
        
        private static bool initHooks = false;

        public override void Init()
        {
            if (!initHooks)
            {
                initHooks = true;
                HookUtil.HookGen<Player>("Hurt", PlayerHurtHook);
                HookUtil.HookGen<Player>("KillMe", PlayerKillHook);
                HookUtil.HookGen<Player>("ResetEffects", PlayerResetEffectsHook);
            }
        }

        private double PlayerHurtHook(Func<Player, PlayerDeathReason, int, int, bool, bool, bool, int, double> orig, Player self, PlayerDeathReason damageSource, int Damage, int hitDirection, bool pvp, bool quiet, bool Crit, int cooldownCounter)
        {
            if (self.whoAmI == Main.myPlayer && antiHurt)
            {
                NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, self.whoAmI);
                return 0.0d;
            }

            return orig(self, damageSource, Damage, hitDirection, pvp, quiet, Crit, cooldownCounter);
        }
        private void PlayerKillHook(Action<Player, PlayerDeathReason, double, int, bool> orig, Player self, PlayerDeathReason damageSource, double dmg, int hitDirection, bool pvp)
        {
            if (self.whoAmI == Main.myPlayer && antiHurt)
            {
                NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, self.whoAmI);
                return;
            }
            orig(self, damageSource, dmg, hitDirection, pvp);
        }
        private void PlayerResetEffectsHook(Action<Player> orig, Player self)
        {
            orig(self);

            if (self.whoAmI == Main.myPlayer)
            {
                if (antiHurt)
                {
                    self.statLife = self.statLifeMax2;
                }
            }
        }

        public override void Draw(ImGuiIOPtr io)
        {
            ImGui.PushFont(ClientAssets.GetMonospaceFont(18));

            NVector2 windowSize = io.DisplaySize / new NVector2(3f, 2f);
            
            ImGui.SetNextWindowPos(new NVector2(0, io.DisplaySize.Y - windowSize.Y), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.FirstUseEver);

            ImGui.Begin("Main window");

            if (!Main.gameMenu && Main.CanUpdateGameplay)
            {
                DrawInGameWorld(io);
            }
            else
            {
                DrawInMenu(io);
            }


            ImGui.End();

            ImGui.PopFont();
        }

        public void DrawInGameWorld(ImGuiIOPtr io)
        {
            ImGui.Checkbox("Anti-Hurt", ref antiHurt);
        }

        public void DrawInMenu(ImGuiIOPtr io)
        {

        }
    }
}
