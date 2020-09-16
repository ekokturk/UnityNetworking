using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLNet;
using GameSavvy.Byterizer;

[CreateAssetMenu(menuName = "LLNet/Messages/ChatTeam")]
public class Message_ChatTeam : ANetMessage
{
    private void OnEnable()
    {
        MessageType = NetMessageType.CHAT_TEAM_MESSAGE;
    }

    public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
    {
        var msg = msgData.PopString();
        var sender = msgData.PopInt32();
        client.AddMessageToQueue($"[Team Message from {client.NetUsers[sender].UserName}] {msg}");    // Show message

    }

    public override void Server_ReceiveMessage(int connectionId, ByteStream msgData, LLServer server)
    {
        List<int> targetIdList = new List<int>();
        var targetTeam = msgData.PopInt32();                                // Get team number from bytestream
        var msg = msgData.PopString();                                      // Get message from bytestream
        foreach(var user in server.NetUsers)                                // Check for all users with the same team
        {
            if(user.Value.ConnectionID == connectionId) continue;           // Skip player that sent the message
            if(user.Value.TeamNumber != targetTeam) continue;               // Skip other team
            targetIdList.Add(user.Value.ConnectionID);                      // Contain their ids
        }
        int[] targetIdArray = targetIdList.ToArray();                       // Convert list to array

        // Create a new message stream for team message
        var msgNew = new ByteStream();
        msgNew.Encode
        (
            (byte)NetMessageType.CHAT_TEAM_MESSAGE,
            msg,
            connectionId
        );
        // Send team message to the members of the same team with multicast
        server.MulticastNetMessage(targetIdArray, server.ReliableChannel, msgNew.ToArray());        
    }
}
