using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLNet;
using GameSavvy.Byterizer;

[CreateAssetMenu(menuName = "LLNet/Messages/ChatWhisper")]
public class Message_ChatWhisper : ANetMessage
{
    private void OnEnable()
    {
        MessageType = NetMessageType.CHAT_WHISPER;
    }

    public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
    {

        msgData.PopInt32(); // Popping target User
        var msg = msgData.PopString();
        var sender = msgData.PopInt32();

        client.AddMessageToQueue($"[Whisper from {client.NetUsers[sender].UserName}] {msg}");

    }

    public override void Server_ReceiveMessage(int connectionId, ByteStream msgData, LLServer server)
    {
        var targetId = msgData.PopInt32();
        msgData.Append(connectionId);

        server.SendNetMessage(targetId, server.ReliableChannel, msgData.ToArray());
    }
}
