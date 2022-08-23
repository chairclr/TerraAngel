using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using TerraAngel.Hooks;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TerraAngel.Cheat;
using TerraAngel.Graphics;
using TerraAngel.WorldEdits;
using TerraAngel;
using System.Reflection;
using Terraria.Localization;
using System.Diagnostics;
using System.IO;
using TerraAngel.Utility;
using System.Runtime.CompilerServices;
using TerraAngel.Client.Config;

namespace TerraAngel.Client.ClientWindows
{
    public class NetMessageWindow : ClientWindow
    {
        public override bool IsToggleable => true;

        public override bool DefaultEnabled => false;

        public override string Title => "Net Debugger";

        public override Keys ToggleKey => ClientConfig.Settings.ToggleNetDebugger;
        public override bool IsPartOfGlobalUI => false;


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
        private int number1 = 0;
        private float number2 = 0;
        private float number3 = 0;
        private float number4 = 0;
        private int number5 = 0;
        private int number6 = 0;
        private int number7 = 0;

        public static bool LoggingMessages = false;
        private string logsFilter = "";
        private string traceFilter = "";
        private bool upMessages = true;
        private bool downMessages = true;

        private List<NetMessageAction> Actions = new List<NetMessageAction>();
        private int selectedAction = 0;
        private string[] actionNames = Util.EnumFancyNames<MessageActions>();

        private static List<NetPacketInfo>[] allPackets = new List<NetPacketInfo>[MessageID.Count];
        private static List<NetPacketInfo>[] sentPackets = new List<NetPacketInfo>[MessageID.Count];
        private static List<NetPacketInfo>[] receivePackets = new List<NetPacketInfo>[MessageID.Count];
        private static bool[] messagesShownInTree = new bool[MessageID.Count];
        public static HashSet<int> MessagesToLogTraces = new HashSet<int>();

        public static void AddPacket(NetPacketInfo packet)
        {
            allPackets[packet.Type].Add(packet);
            if (packet.Sent)
            {
                sentPackets[packet.Type].Add(packet);
            }
            else
            {
                receivePackets[packet.Type].Add(packet);
            }
        }
        static NetMessageWindow()
        {
            for (int i = 0; i < MessageID.Count; i++)
            {
                allPackets[i] = new List<NetPacketInfo>();
                sentPackets[i] = new List<NetPacketInfo>();
                receivePackets[i] = new List<NetPacketInfo>();
            }
        }

        public override void Init()
        {
            maxMessageName = messageIDFields.Max((x) => x.Value.Name.Length);
        }

        public override void Draw(ImGuiIOPtr io)
        {
            ImGui.PushFont(ClientAssets.GetMonospaceFont(16f));
            bool open = IsEnabled;
            ImGui.Begin("Net Debugger", ref open, ImGuiWindowFlags.MenuBar);
            IsEnabled = open;

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Message Logs"))
                {
                    if (ImGui.MenuItem("Clear"))
                    {
                        for (int i = 0; i < MessageID.Count; i++)
                        {
                            allPackets[i].Clear();
                            sentPackets[i].Clear();
                            receivePackets[i].Clear();
                        }
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (ImGui.BeginTabBar("Net Debugger Tab Bar"))
            {
                if (ImGui.BeginTabItem("Net Message Logs"))
                {
                    ImGui.Text("Search:"); ImGui.SameLine();
                    ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 2.8f); ImGui.InputText("##MessageFilter", ref logsFilter, 512); ImGui.PopItemWidth(); ImGui.SameLine();
                    ImGui.Checkbox("Log Messages", ref LoggingMessages); ImGui.SameLine();
                    ImGui.Checkbox($"{Icon.ArrowUp}", ref upMessages); ImGui.SameLine();
                    ImGui.Checkbox($"{Icon.ArrowDown}", ref downMessages);

                    ImGui.Text("Packets with traces:"); ImGui.SameLine();
                    unsafe
                    {
                        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 2.8f); ImGui.InputText("##TraceFilter", ref traceFilter, 512, ImGuiInputTextFlags.CallbackCharFilter, (x) =>
                        {
                            switch (x->EventFlag)
                            {
                                case ImGuiInputTextFlags.CallbackCharFilter:
                                    if (char.IsNumber((char)x->EventChar) || x->EventChar == ',' || x->EventChar == ' ') return 0;
                                    return 1;
                                    break;
                            }
                            return 0;
                        }); ImGui.PopItemWidth();
                    }
                    MessagesToLogTraces = new HashSet<int>(traceFilter.Split(',').Select(x => { if (int.TryParse(x.Trim(), out int a)) { return a; } return -1; }).Where(x => x != -1));


                    if (ImGui.BeginChild("##MessageLogScrolling"))
                    {
                        bool CheckFilter(int i, string packetName)
                        {
                            if (logsFilter.Length > 0)
                            {
                                if (int.TryParse(logsFilter, out int p))
                                {
                                    if (i != p)
                                    {
                                        return true;
                                    }
                                }
                                else if (!packetName.ToLower().Contains(logsFilter.ToLower()))
                                {
                                    return true;
                                }
                            }
                            return false;
                        }

                        for (int i = 0; i < MessageID.Count; i++)
                        {
                            string packetName = $"{messageIDFields[i].Name}/{i}";

                            if (CheckFilter(i, packetName))
                                continue;

                            List<NetPacketInfo> packetInfo = allPackets[i];
                            if (upMessages && !downMessages)
                            {
                                packetInfo = sentPackets[i];
                            }
                            else if (downMessages && !upMessages)
                            {
                                packetInfo = receivePackets[i];
                            }

                            if (ImGui.Selectable($"{(messagesShownInTree[i] ? Icon.TriangleDown : Icon.TriangleRight)} {packetName,-35}{(packetInfo.Count == 0 ? "" : packetInfo.Count.ToString())}"))
                            {
                                messagesShownInTree[i] = !messagesShownInTree[i];
                            }

                            if (messagesShownInTree[i])
                            {
                                ImGui.Indent(20f);
                                for (int j = 0; j < packetInfo.Count; j++)
                                {
                                    if (packetInfo[j].Sent)
                                    {
                                        ImGui.TextUnformatted(GetSendCallAsString(packetInfo[j].Type, "", packetInfo[j].Number1, packetInfo[j].Number2, packetInfo[j].Number3, packetInfo[j].Number4, packetInfo[j].Number5, packetInfo[j].Number6, packetInfo[j].Number7, true) + (packetInfo[j].StackTrace.Length == 0 ? "" : $"\nStack Trace:\n{packetInfo[j].StackTrace}"));
                                    }
                                    else
                                    {
                                        ImGui.TextUnformatted($"NetMessage.GetData Type: {messageIDFields[packetInfo[j].Type].Name}/{packetInfo[j].Type}");
                                    }
                                }
                                ImGui.Unindent(20f);
                            }
                        }

                        ImGui.EndChild();
                    }
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Net Message Sender"))
                {
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

                        if (ImGui.CollapsingHeader("Raw Values"))
                        {
                            if (ImGui.CollapsingHeader("Network Text"))
                            {
                                ImGui.InputTextMultiline("##NETstr", ref networkText, short.MaxValue - 100, ImGui.GetContentRegionAvail() / new NVector2(2.5f, 3.5f));
                            }
                            ImGui.Text("Number 1"); ImGui.SameLine(); ImGui.InputInt("##NUM1", ref number1);
                            ImGui.Text("Number 2"); ImGui.SameLine(); ImGui.InputFloat("##NUM2", ref number2);
                            ImGui.Text("Number 3"); ImGui.SameLine(); ImGui.InputFloat("##NUM3", ref number3);
                            ImGui.Text("Number 4"); ImGui.SameLine(); ImGui.InputFloat("##NUM4", ref number4);
                            ImGui.Text("Number 5"); ImGui.SameLine(); ImGui.InputInt("##NUM5", ref number5);
                            ImGui.Text("Number 6"); ImGui.SameLine(); ImGui.InputInt("##NUM6", ref number6);
                            ImGui.Text("Number 7"); ImGui.SameLine(); ImGui.InputInt("##NUM7", ref number7);
                        }


                        if (ImGui.Button("Send net message"))
                        {
                            NetMessage.SendData(messageIdToSend, -1, -1, NetworkText.FromLiteral(networkText), number1, number2, number3, number4, number5, number6, number7);
                        }
                        ImGui.SameLine();
                        ImGui.Checkbox("Send every frame", ref sendMessageEveryFrame);

                        string s = GetSendCallAsString(messageIdToSend, networkText, number1, number2, number3, number4, number5, number6, number7);
                        if (ImGui.Selectable(s))
                        {
                            ImGui.SetClipboardText(s);
                        }
                    }
                    else
                    {
                        ImGui.Text("What are you trying to do sending net packets outside of mulitplayer, silly?");
                    }

                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Raw Message Sender"))
                {
                    if (ImGui.Button($"{Icon.Add}"))
                    {
                        Actions.Add(new NetMessageAction((MessageActions)selectedAction));
                    }
                    ImGui.SameLine(); ImGui.Combo("##ItemActionsAdd", ref selectedAction, actionNames, actionNames.Length);
                    if (ImGui.BeginChild("RawMessageScrolling", NVector2.Zero, false, ImGuiWindowFlags.HorizontalScrollbar))
                    {
                        int size = 0;
                        ImGui.NewLine();
                        ImGui.Spacing();
                        for (int i = 0; i < Actions.Count; i++)
                        {
                            if (ImGui.Button($"{Icon.Remove}##{i}"))
                            {
                                Actions.RemoveAt(i);
                                continue;
                            }
                            ImGui.SameLine();
                            ImGui.TextUnformatted($"writer.Write(");
                            ImGui.SameLine();
                            ImGui.PushItemWidth(100f);
                            unsafe
                            {
                                fixed (ulong* v = &Actions[i].Data)
                                {
                                    ImGui.InputScalar($"##ValueLmao{i}", Actions[i].GetActionAsImGuiType(), (IntPtr)v);
                                }
                            }
                            ImGui.PopItemWidth();
                            ImGui.SameLine();
                            ImGui.TextUnformatted(")");
                            ImGui.SameLine();
                            ImGui.PushItemWidth(180f);
                            if (ImGui.Combo($"##ItemActionsChange{i}", ref Actions[i].ActionInt, actionNames, actionNames.Length)) Actions[i].Data = 0;
                            ImGui.PopItemWidth();

                            size += Actions[i].GetActionSize();
                        }


                        ImGui.SetCursorPosY(0f);

                        ImGui.BeginDisabled();
                        ImGui.Button($"{Icon.Remove}##-1");
                        ImGui.SameLine();
                        ImGui.TextUnformatted($"writer.Write(");
                        ImGui.SameLine();
                        ImGui.PushItemWidth(100f);
                        unsafe
                        {
                            ImGui.InputScalar($"##ValueLmao-1", ImGuiDataType.S32, (IntPtr)(&size));
                        }
                        ImGui.PopItemWidth();
                        ImGui.SameLine();
                        ImGui.TextUnformatted(")");
                        ImGui.SameLine();
                        ImGui.PushItemWidth(180f);
                        int mst = (int)MessageActions.WriteUnsignedShort;
                        ImGui.Combo($"##ItemActionsChange-1", ref mst, actionNames, actionNames.Length);
                        ImGui.PopItemWidth();
                        ImGui.EndDisabled();

                        ImGui.EndChild();
                    }


                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
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

        private string GetSendCallAsString(int type, string networkText, int number1, float number2, float number3, float number4, int number5, int number6, int number7, bool showNumbersInId = false)
        {
            string s = $"NetMessage.SendData(MessageID.{messageIDFields[type].Name}";

            if (showNumbersInId)
            {
                s += $"/{type}";
            }

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

    public struct NetPacketInfo
    {
        public int Type;
        public int Number1;
        public float Number2;
        public float Number3;
        public float Number4;
        public int Number5;
        public int Number6;
        public int Number7;
        public bool Sent;
        public string StackTrace;

        public NetPacketInfo(int type, bool sent, int number1, float number2, float number3, float number4, int number5, int number6, int number7, string stackTrace = "")
        {
            Type = type;
            Sent = sent;
            Number1 = number1;
            Number2 = number2;
            Number3 = number3;
            Number4 = number4;
            Number5 = number5;
            Number6 = number6;
            Number7 = number7;
            StackTrace = stackTrace;
        }
    }
    public class NetMessageAction
    {
        public MessageActions Action;
        public ref int ActionInt
        {
            get
            {
                return ref Unsafe.As<MessageActions, int>(ref Action);
            }
        }
        public ulong Data;

        public NetMessageAction(MessageActions action)
        {
            Action = action;
            Data = 0;
        }
        public NetMessageAction(MessageActions action, ValueType data)
        {
            Action = action;
            Data = (ulong)data;
        }

        public void WriteToStream(BinaryWriter writer)
        {
            switch (Action)
            {
                case MessageActions.WriteByte:
                    writer.Write((byte)Data);
                    break;
                case MessageActions.WriteSignedByte:
                    writer.Write((sbyte)Data);
                    break;
                case MessageActions.WriteShort:
                    writer.Write((short)Data);
                    break;
                case MessageActions.WriteUnsignedShort:
                    writer.Write((ushort)Data);
                    break;
                case MessageActions.WriteInt:
                    writer.Write((int)Data);
                    break;
                case MessageActions.WriteUnsignedInt:
                    writer.Write((uint)Data);
                    break;
                case MessageActions.WriteFloat:
                    writer.Write((float)Data);
                    break;
                case MessageActions.WriteDouble:
                    writer.Write((double)Data);
                    break;
                case MessageActions.WriteLong:
                    writer.Write((long)Data);
                    break;
                case MessageActions.WriteUnsignedLong:
                    writer.Write((ulong)Data);
                    break;
            }
        }

        public ImGuiDataType GetActionAsImGuiType()
        {
            switch (Action)
            {
                case MessageActions.WriteByte:
                    return ImGuiDataType.U8;
                case MessageActions.WriteSignedByte:
                    return ImGuiDataType.S8;
                case MessageActions.WriteShort:
                    return ImGuiDataType.S16;
                case MessageActions.WriteUnsignedShort:
                    return ImGuiDataType.U16;
                case MessageActions.WriteInt:
                    return ImGuiDataType.S32;
                case MessageActions.WriteUnsignedInt:
                    return ImGuiDataType.U32;
                case MessageActions.WriteFloat:
                    return ImGuiDataType.Float;
                case MessageActions.WriteDouble:
                    return ImGuiDataType.Double;
                case MessageActions.WriteLong:
                    return ImGuiDataType.S64;
                case MessageActions.WriteUnsignedLong:
                    return ImGuiDataType.U64;
            }

            return ImGuiDataType.U8;
        }

        public int GetActionSize()
        {
            switch (Action)
            {
                case MessageActions.WriteByte:
                case MessageActions.WriteSignedByte:
                    return 1;
                case MessageActions.WriteShort:
                case MessageActions.WriteUnsignedShort:
                    return 2;
                case MessageActions.WriteInt:
                case MessageActions.WriteUnsignedInt:
                case MessageActions.WriteFloat:
                    return 4;
                case MessageActions.WriteDouble:
                case MessageActions.WriteLong:
                case MessageActions.WriteUnsignedLong:
                    return 8;
            }

            return 8;
        }
    }
    public enum MessageActions
    {
        WriteByte,
        WriteSignedByte,
        WriteShort,
        WriteUnsignedShort,
        WriteInt,
        WriteUnsignedInt,
        WriteFloat,
        WriteDouble,
        WriteLong,
        WriteUnsignedLong,

    }
}