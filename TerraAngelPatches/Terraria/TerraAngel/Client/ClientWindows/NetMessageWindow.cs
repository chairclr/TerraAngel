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

namespace TerraAngel.Client.ClientWindows
{
    public class NetMessageWindow : ClientWindow
    {
        public override bool DefaultEnabled => false;
        public override bool IsToggleable => true;
        public override string Title => "Net Message Sender";
        public override Keys ToggleKey => ClientLoader.Config.ToggleNetMessageSender;

        private int messageIdToSend = 0;
        private string messageName = nameof(MessageID.NeverCalled);


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
                ImGui.SameLine();
            }
            else
            {
                ImGui.Text("What are you trying to do sending net packets outside of mulitplayer, silly?");
            }

            ImGui.End();
            ImGui.PopFont();
        }
    }
}
