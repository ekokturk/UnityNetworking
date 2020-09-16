using UnityEngine;
using LLNet;
using GameSavvy.Byterizer;

[CreateAssetMenu(menuName = "LLNet/Messages/ConnectionAck")]
public class Message_ConnectionAck : ANetMessage
{

    private void OnEnable()
    {
        MessageType = NetMessageType.CONNECTION_ACK;
    }

    public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
    {
        var conId = msgData.PopInt32();
        var pos = msgData.PopVector3();


        var meUser = new NetUser()
        {
            ConnectionID = conId,
            UserName = client.UserName,
            TeamNumber = client.TeamNumber
        };

        client.NetUsers[conId] = meUser;
        client.MyConnectionId = conId;
        client.NetUsers[conId].Player = Instantiate(client._PlayerPrefab, pos, client._PlayerPrefab.transform.rotation);

        var msg = new ByteStream();
        msg.Encode
        (
            (byte)NetMessageType.USER_INFO,
            meUser.UserName,
            meUser.TeamNumber
        );
        client.SendNetMessage(client.ReliableChannel, msg.ToArray());

        Debug.Log($"@Client -> MyConnectionId [{client.MyConnectionId}]");
    }

    public override void Server_ReceiveMessage(int connectionId, ByteStream data, LLServer server)
    {
        //Never gets called
    }
}
