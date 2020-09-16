using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLNet;
using GameSavvy.Byterizer;

[CreateAssetMenu(menuName = "LLNet/Messages/UserInfo")]
public class Message_UserInfo : ANetMessage
{
    private void OnEnable()
    {
        MessageType = NetMessageType.USER_INFO;
    }

    public override void Client_ReceiveMessage(ByteStream msgData, LLClient client)
    {
            var conId = msgData.PopInt32();
            var userName = msgData.PopString();
            var teamNum = msgData.PopInt32();

            var newUser = new NetUser()
            {
                ConnectionID = conId,
                UserName = userName,
                TeamNumber = teamNum
            };

            client.NetUsers[conId] = newUser;

            var posPlayer = msgData.PopVector3();
            client.NetUsers[conId].Player = Instantiate(client._PlayerPrefab, posPlayer, client._PlayerPrefab.transform.rotation);

    }

    public override void Server_ReceiveMessage(int connectionId, ByteStream msgData, LLServer server)
    {
            server.NetUsers[connectionId].UserName = msgData.PopString();
            server.NetUsers[connectionId].TeamNumber = msgData.PopInt32();

            // BC this user's info to other users
            var msg = new ByteStream();
            msg.Encode
            (
                (byte)NetMessageType.USER_INFO,
                connectionId,
                server.NetUsers[connectionId].UserName,
                server.NetUsers[connectionId].TeamNumber,
                server.NetUsers[connectionId].Player.transform.position
            );
            server.BroadcastNetMessage(server.ReliableChannel, msg.ToArray(), connectionId);

            // Send other user's info to this user
            foreach (var user in server.NetUsers)
            {
                if (user.Key == connectionId) continue;
                msg = new ByteStream();
                msg.Encode
                (
                    (byte)NetMessageType.USER_INFO,
                    user.Value.ConnectionID,
                    user.Value.UserName,
                    user.Value.TeamNumber,
                    server.NetUsers[user.Value.ConnectionID].Player.transform.position
                );
                server.SendNetMessage(connectionId, server.ReliableChannel, msg.ToArray());
            }

            Debug.Log($"@Server -> User[{connectionId}, {server.NetUsers[connectionId].UserName}, {server.NetUsers[connectionId].TeamNumber}] registered");
    }
}
