using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LLNet;
using GameSavvy.Byterizer;

[CreateAssetMenu(menuName = "LLNet/Messages/Movement")]
public class Message_Movement : ANetMessage
{

    private void OnEnable()
    {
        MessageType = NetMessageType.MOVEMENT;
    }

    public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
    {
        var pos = msgData.PopVector3();
        var sender = msgData.PopInt32();

        Debug.Log($"{sender} moved => X: {pos.x} - Y: {pos.y} - Z: {pos.z}");
        GameObject movingPlayer = client.NetUsers[sender].Player; 
        if(movingPlayer != null)
        {
            movingPlayer.GetComponent<NavMeshAgent>().SetDestination(pos);
        }

    }

    public override void Server_ReceiveMessage(int connectionId, ByteStream msgData, LLServer server)
    {
        msgData.Append(connectionId);
        server.BroadcastNetMessage(server.ReliableChannel, msgData.ToArray(), connectionId);
        Vector3 pos = msgData.PopVector3();
        GameObject movingPlayer = server.NetUsers[connectionId].Player; 
        if(movingPlayer != null)
        {
            movingPlayer.GetComponent<NavMeshAgent>().SetDestination(pos);
        }
    }

}
