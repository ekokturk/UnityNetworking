using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLNet;
using GameSavvy.Byterizer;

[CreateAssetMenu(menuName = "LLNet/Messages/DisconnectAck")]
public class Message_DisconnectAck : ANetMessage
{
    private void OnEnable()
    {
        MessageType = NetMessageType.DISCONNECTION_ACK;
    }
    
    public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
    {
        var conId = msgData.PopInt32();                                                     // Get id of the disconnected user
        Debug.Log($"@Client.Disconnect -> userId = [{conId}]");                             // Disconnected log
        client.AddMessageToQueue($"{client.NetUsers[conId].UserName} disconnected");        // Show disconnect message in the chat
        GameObject disconnectPlayer = client.NetUsers[conId].Player;
        if(disconnectPlayer != null)
        {
            Destroy(disconnectPlayer);
        }
        client.NetUsers.Remove(conId);                                                      // Remove player from the list
    }

    public override void Server_ReceiveMessage(int connectionId, ByteStream msgData, LLServer server)
    {
        // Not required
    }
}
