using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLNet;
using GameSavvy.Byterizer;

[CreateAssetMenu(menuName = "LLNet/Messages/ChatBroadcast")]
public class Message_ChatBroadcast : ANetMessage
{
    private void OnEnable()
    {
        MessageType = NetMessageType.CHAT_BROADCAST;
    }

    public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
    {
        var msg = msgData.PopString();
        var sender = msgData.PopInt32();
        client.AddMessageToQueue($"[BC from {client.NetUsers[sender].UserName}] {msg}");

    }

    public override void Server_ReceiveMessage(int connectionId, ByteStream msgData, LLServer server)
    {
        msgData.Append(connectionId);
        server.BroadcastNetMessage(server.ReliableChannel, msgData.ToArray(), connectionId);
    }
}
