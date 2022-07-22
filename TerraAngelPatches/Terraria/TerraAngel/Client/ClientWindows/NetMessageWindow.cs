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
using TerraAngel.Cheat;
using TerraAngel.Graphics;
using TerraAngel.WorldEdits;
using TerraAngel;
using System.Reflection;
using Terraria.Localization;

namespace TerraAngel.Client.ClientWindows
{
    public class NetMessageWindow : ClientWindow
    {
        public override bool DefaultEnabled => false;
        public override bool IsToggleable => true;
        public override string Title => "Net Message Sender";
        public override Keys ToggleKey => ClientLoader.Config.ToggleNetMessageSender;


        private Dictionary<int, FieldInfo> messageIDFields = typeof(MessageID).GetFields().Where(x => 
        { 
            if (!x.IsStatic) return false; 
            object? ovalue = x.GetValue(null); 
            if (ovalue is null) return false; 
            if (ovalue.GetType() != typeof(byte)) return false;
            byte value = (byte)ovalue;
            if (value < 0 || value > MessageID.Count - 1) return false;
            return true;
        }).ToDictionary(x => (int)((byte)x.GetRawConstantValue()), y => y);
        private Dictionary<string, FieldInfo> messageIDFieldsByName = typeof(MessageID).GetFields().Where(x =>
        {
            if (!x.IsStatic) return false;
            object? ovalue = x.GetValue(null);
            if (ovalue is null) return false;
            if (ovalue.GetType() != typeof(byte)) return false;
            byte value = (byte)ovalue;
            if (value < 0 || value > MessageID.Count - 1) return false;
            return true;
        }).ToDictionary(x => x.Name, y => y);
        private int maxMessageName;


        private bool sendMessageEveryFrame = false;
        private string messageName = nameof(MessageID.NeverCalled);

        private int messageIdToSend = 0;

        private string networkText = "";

        private int   number1 = 0;
        private float number2 = 0;
        private float number3 = 0;
        private float number4 = 0;
        private int   number5 = 0;
        private int   number6 = 0;
        private int   number7 = 0;

        public override void Init()
        {
            maxMessageName = messageIDFields.Max((x) => x.Value.Name.Length);
        }

        public override void Draw(ImGuiIOPtr io)
        {
            ImGui.PushFont(ClientAssets.GetMonospaceFont(16));
            ImGui.Begin("Net Message Sender");

            bool isInMultiplayerGame = Main.netMode == 1 && Netplay.Connection.State != 0;

            if (isInMultiplayerGame)
            {
                ImGui.Text("Message ID: ");
                ImGui.SameLine();
                ImGui.PushItemWidth(200f);
                if (ImGui.InputInt("##IDint", ref messageIdToSend))
                {
                    messageIdToSend = Utils.Clamp(messageIdToSend, 0, MessageID.Count - 1);
                    messageName = messageIDFields[messageIdToSend].Name;
                }
                ImGui.PopItemWidth();
                ImGui.SameLine();
                ImGui.Text("/");
                ImGui.SameLine();
                ImGui.PushItemWidth(200f);
                if (ImGui.InputText("##IDstr", ref messageName, (uint)maxMessageName))
                {
                    if (messageIDFieldsByName.ContainsKey(messageName))
                    {
                        messageIdToSend = (int)((byte)messageIDFieldsByName[messageName].GetRawConstantValue());
                    }
                }
                
                if (ImGui.CollapsingHeader("Network Text"))
                {
                    ImGui.InputTextMultiline("##NETstr", ref networkText, short.MaxValue - 100, ImGui.GetContentRegionAvail() / new NVector2(2.5f, 4f));
                }
                if (ImGui.CollapsingHeader("Raw Values"))
                {
                    ImGui.Text("Number 1"); ImGui.SameLine(); ImGui.InputInt("##NUM1",   ref number1);
                    ImGui.Text("Number 2"); ImGui.SameLine(); ImGui.InputFloat("##NUM2", ref number2);
                    ImGui.Text("Number 3"); ImGui.SameLine(); ImGui.InputFloat("##NUM3", ref number3);
                    ImGui.Text("Number 4"); ImGui.SameLine(); ImGui.InputFloat("##NUM4", ref number4);
                    ImGui.Text("Number 5"); ImGui.SameLine(); ImGui.InputInt("##NUM5",   ref number5);
                    ImGui.Text("Number 6"); ImGui.SameLine(); ImGui.InputInt("##NUM6",   ref number6);
                    ImGui.Text("Number 7"); ImGui.SameLine(); ImGui.InputInt("##NUM7",   ref number7);
                }


                if (ImGui.Button("Send net message"))
                {
                    NetMessage.SendData(messageIdToSend, -1, -1, NetworkText.FromLiteral(networkText), number1, number2, number3, number4, number5, number6, number7);
                }
                ImGui.SameLine();
                ImGui.Checkbox("Send every frame", ref sendMessageEveryFrame);

                if (ImGui.Selectable(GetSendCallAsString()))
                {
                    ImGui.SetClipboardText(GetSendCallAsString());
                }
            }
            else
            {
                ImGui.Text("What are you trying to do sending net packets outside of mulitplayer, silly?");
            }

            ImGui.End();
            ImGui.PopFont();
        }

        public override void Update()
        {
            if (sendMessageEveryFrame)
            {
                NetMessage.SendData(messageIdToSend, -1, -1, NetworkText.FromLiteral(networkText), number1, number2, number3, number4, number5, number6, number7);
            }
        }

        private string GetSendCallAsString()
        {
            string s = $"NetMessage.SendData(MessageID.{messageIDFields[messageIdToSend].Name}";

            if (networkText.Length != 0)
            {
                s += $", text: NetworkText.FromLiteral(\"{networkText}\")";
            }

            if (number1 != 0) s += $", number1: {number1}";
            if (number2 != 0) s += $", number2: {number2}";
            if (number3 != 0) s += $", number3: {number3}";
            if (number4 != 0) s += $", number4: {number4}";
            if (number5 != 0) s += $", number5: {number5}";
            if (number6 != 0) s += $", number6: {number6}";
            if (number7 != 0) s += $", number7: {number7}";

            s += ")";

            return s;
        }
    }
}
