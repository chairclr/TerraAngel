using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using TerraAngel.Graphics;
using TerraAngel.Hooks;
using TerraAngel.Input;
using TerraAngel;
using TerraAngel.UI;
using Terraria;
using TerraAngel.Utility;
using Microsoft.Xna.Framework;
using System.Reflection;
using TerraAngel.Client.Config;
using Microsoft.CodeAnalysis.Operations;

namespace TerraAngel.Client.ClientWindows
{
    public class StatsWindow : ClientWindow
    {

        public override Keys ToggleKey => Keys.None;

        public override bool IsToggleable => false;

        public override bool DefaultEnabled => true;

        public override bool IsEnabled { get => ClientConfig.Settings.ShowStatsWindow; }

        public override string Title => "Stat Window";
        public override bool IsPartOfGlobalUI => false;

        private static bool HooksGenerated = false;

        public override void Init()
        {
            if (!HooksGenerated)
            {
                HooksGenerated = true;
                // HookUtil.HookGen<Terraria.UI.NetDiagnosticsUI>("CountSentMessage", CountSentMessageHook);
                // HookUtil.HookGen<Terraria.UI.NetDiagnosticsUI>("CountReadMessage", CountReadMessageHook);
            }
        }

        private static int PacketsUpLastSecond = 0;
        private static int BytesUpLastSecond = 0;

        private static int PacketsDownLastSecond = 0;
        private static int BytesDownLastSecond = 0;

        private static int PacketsUpLastSecondCounting = 0;
        private static int BytesUpLastSecondCounting = 0;

        private static int PacketsDownLastSecondCounting = 0;
        private static int BytesDownLastSecondCounting = 0;

        //public static void CountSentMessageHook(Action<Terraria.UI.NetDiagnosticsUI, int, int> orig, Terraria.UI.NetDiagnosticsUI self, int messageId, int messageLength)
        //{
        //    orig(self, messageId, messageLength);
        //
        //    PacketsUpLastSecondCounting++;
        //    BytesUpLastSecondCounting += messageLength;
        //}
        //private static void CountReadMessageHook(Action<Terraria.UI.NetDiagnosticsUI, int, int> orig, Terraria.UI.NetDiagnosticsUI self, int messageId, int messageLength)
        //{
        //    orig(self, messageId, messageLength);
        //
        //    PacketsDownLastSecondCounting++;
        //    BytesDownLastSecondCounting += messageLength;
        //}
        public static void CountSentMessage(int len)
        {
            PacketsUpLastSecondCounting++;
            BytesUpLastSecondCounting += len;
        }
        public static void CountReadMessage(int len)
        {
            PacketsDownLastSecondCounting++;
            BytesDownLastSecondCounting += len;

        }


        private bool moveStatWindow = false;

        private bool decreaseTransperency = false;
        public override void Draw(ImGuiIOPtr io)
        {
            if (InputSystem.IsKeyPressed(ClientConfig.Settings.ToggleStatsWindowMovability))
                moveStatWindow = !moveStatWindow;

            ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize;

            if (!moveStatWindow)
                flags |= ImGuiWindowFlags.NoInputs;
            else
            {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, new System.Numerics.Vector4(0.3f, 0.5f, 0.3f, 0.8f));
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0.3f, 0.5f, 0.3f, 0.8f));
            }
            if (decreaseTransperency)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ClientConfig.Settings.StatsWindowHoveredTransperency);
            }


            bool isInMultiplayerGame = Main.netMode == 1 && Netplay.Connection.State != 0;

            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, io.DisplaySize.Y / 2.2f - 32f), ImGuiCond.FirstUseEver);
            ImGui.Begin("##StatWindow", flags);
            ImGui.PushFont(ClientAssets.GetMonospaceFont(16f));

            ImGui.TextUnformatted($"{Icon.CircleFilled} TerraAngel v2.1");

            ImGui.TextUnformatted($"FPS {io.Framerate:F1}");

            string packetsUpString = $"{PacketsUpLastSecond}";
            string packetsDownString = $"{PacketsDownLastSecond}";

            string kilobytesUpString = $"{Util.PrettyPrintBytes(BytesUpLastSecond)}";
            string kilobytesDownString = $"{Util.PrettyPrintBytes(BytesDownLastSecond)}";

            if (!isInMultiplayerGame) kilobytesDownString = kilobytesUpString = packetsDownString = packetsUpString = "N/A";


            ImGuiUtil.TextColored($"Packets\t{Icon.ArrowUp}{packetsUpString,7} / {Icon.ArrowDown}{packetsDownString,7}", !isInMultiplayerGame ? ImGui.GetColorU32(ImGuiCol.TextDisabled) : ImGui.GetColorU32(ImGuiCol.Text));

            ImGuiUtil.TextColored($"Bytes  \t{Icon.ArrowUp}{kilobytesUpString,7} / {Icon.ArrowDown}{kilobytesDownString,7}", !isInMultiplayerGame ? ImGui.GetColorU32(ImGuiCol.TextDisabled) : ImGui.GetColorU32(ImGuiCol.Text));

            ImGui.PopFont();
            if (decreaseTransperency)
            {
                ImGui.PopStyleVar();
                decreaseTransperency = false;
            }
            if (ImGui.IsMouseHoveringRect(ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize()))
                decreaseTransperency = true;
            ImGui.End();

            if (moveStatWindow)
            {
                ImGui.PopStyleColor(2);
            }

            if (Main.GameUpdateCount % 60 == 0)
            {
                PacketsUpLastSecond = PacketsUpLastSecondCounting;
                PacketsDownLastSecond = PacketsDownLastSecondCounting;

                PacketsDownLastSecondCounting = (PacketsUpLastSecondCounting = 0);

                BytesUpLastSecond = BytesUpLastSecondCounting;
                BytesDownLastSecond = BytesDownLastSecondCounting;

                BytesDownLastSecondCounting = (BytesUpLastSecondCounting = 0);
            }
        }
    }
}