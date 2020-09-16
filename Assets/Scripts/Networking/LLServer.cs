using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GameSavvy.Byterizer;

namespace LLNet
{
    public class LLServer : MonoBehaviour
    {
        [SerializeField]
        private int _ServerPort = 27000;

        [SerializeField]
        private int _BufferSize = 1024;

        [SerializeField]
        private byte _ThreadPoolSize = 3;

        [SerializeField]
        private NetMessageContainer _NetMessages;

        public GameObject _PlayerPrefab;

        public Dictionary<int, NetUser> NetUsers { get; private set; }
        public byte ReliableChannel { get; private set; }
        public byte UnreliableChannel { get; private set; }

        private int _SocketId = 0;

        private void Start()
        {
            StartServer();
        }

        private void StartServer()
        {
            NetUsers = new Dictionary<int, NetUser>();
            GlobalConfig globalConfig = new GlobalConfig()
            {
                ThreadPoolSize = _ThreadPoolSize
            };

            NetworkTransport.Init(globalConfig);

            ConnectionConfig connectionConfig = new ConnectionConfig()
            {
                SendDelay = 0,
                MinUpdateTimeout = 1
            };
            ReliableChannel = connectionConfig.AddChannel(QosType.Reliable);
            UnreliableChannel = connectionConfig.AddChannel(QosType.Unreliable);

            HostTopology hostTopology = new HostTopology(connectionConfig, 16);
            _SocketId = NetworkTransport.AddHost(hostTopology, _ServerPort);

            StartCoroutine(Receiver());

            Debug.Log($"@StartServer -> {_SocketId}");
        }

        private IEnumerator Receiver()
        {
            int recSocketId, recConnectionId, recChannelId, recDataSize;
            byte error;
            byte[] recBuffer = new byte[_BufferSize];

            while (true)
            {
                var netEventType = NetworkTransport.Receive
                (
                    out recSocketId,
                    out recConnectionId,
                    out recChannelId,
                    recBuffer,
                    _BufferSize,
                    out recDataSize,
                    out error
                );

                switch (netEventType)
                {
                    case NetworkEventType.Nothing:
                        {
                            yield return null;
                            break;
                        }

                    case NetworkEventType.DataEvent:
                        {
                            OnDataEvent(recConnectionId, recChannelId, recBuffer, recDataSize);
                            break;
                        }

                    case NetworkEventType.ConnectEvent:
                        {
                            OnConnectedEvent(recConnectionId);
                            break;
                        }

                    case NetworkEventType.DisconnectEvent:
                        {
                            OnDisconnectedEvent(recConnectionId);
                            break;
                        }

                    default:
                        {
                            Debug.Log($"@Receiver -> Unrecognized Net Message Type [{netEventType.ToString()}]");
                            break;
                        }
                }
            }
        }

        private void OnConnectedEvent(int connectionId)
        {
            float posX = Random.Range(-30f,30f);
            float posZ = Random.Range(-30f,30f);
            Vector3 spawnPosition = new Vector3(posX, _PlayerPrefab.transform.position.y, posZ);

            if (NetUsers.ContainsKey(connectionId))
            {
                Debug.Log($"@Receiver.Connect -> userId = [{connectionId}] Re-Conneted");
            }
            else
            {
                var newUser = new NetUser() { ConnectionID = connectionId };
                NetUsers[connectionId] = newUser;
                NetUsers[connectionId].Player = Instantiate(_PlayerPrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"@Receiver.Connect -> userId = [{connectionId}]");
            }

            var byteStream = new ByteStream();
            byteStream.Append((byte)NetMessageType.CONNECTION_ACK);
            byteStream.Append(connectionId);
            byteStream.Append(spawnPosition);
            SendNetMessage(connectionId, ReliableChannel, byteStream.ToArray());
        }

// ---------------------------------------------------
        // Send disconnected message to all users
        private void OnDisconnectedEvent(int recConnectionId)
        {
            GameObject disconnectPlayer = NetUsers[recConnectionId].Player;
            if(disconnectPlayer != null)
            {
                Destroy(disconnectPlayer);
            }
            NetUsers.Remove(recConnectionId);                                           // Remove disconnected user from server list
            var msg = new ByteStream();                                             
            msg.Encode((byte)NetMessageType.DISCONNECTION_ACK, recConnectionId);        // Create a new disconnection message with user id
            foreach(var user in NetUsers)                                               // Send to all clents
            {
                SendNetMessage(user.Key, ReliableChannel, msg.ToArray());
            }
            Debug.Log($"@Receiver.Disconnect -> userId = [{recConnectionId}]");
        }
// ---------------------------------------------------

        private void OnDataEvent(int connectionId, int recChannelId, byte[] data, int dataSize)
        {
            var msgData = new ByteStream(data, dataSize);
            NetMessageType msgType = (NetMessageType)msgData.PopByte();
            _NetMessages.NetMessagesMap[msgType].Server_ReceiveMessage(connectionId, msgData, this);
        }

        public void SendNetMessage(int targetId, byte channelId, byte[] data)
        {
            NetworkTransport.Send
            (
                _SocketId,
                targetId,
                channelId,
                data,
                data.Length,
                out var error
            );

            if (error != 0)
            {
                Debug.LogError($"@Server -> Error: [{error}] : Could Not Send Message To Client [{targetId}]");
            }
        }

        public void BroadcastNetMessage(byte channelId, byte[] data, int excludeId = -1)
        {
            foreach (var user in NetUsers)
            {
                if (user.Key == excludeId) continue;

                NetworkTransport.Send
                (
                    _SocketId,
                    user.Key,
                    channelId,
                    data,
                    data.Length,
                    out var error
                );

                if (error != 0)
                {
                    Debug.LogError($"@Server -> Error: [{error}] : Could Not Send Message To Client [{user.Key}]");
                }
            }
        }

//----------------------------
        // A message which is sent to multiple clients
        public void MulticastNetMessage(int[] targets, byte channelId, byte[] data)
        {
            // Send message to every target
            for(int i = 0; i< targets.Length ; i++)
            {
                NetworkTransport.Send
                (
                    _SocketId,
                    targets[i],
                    channelId,
                    data,
                    data.Length,
                    out var error
                );
                
                if (error != 0)
                {
                    Debug.LogError($"@Server -> Error: [{error}] : Could Not Send Multicast Message To Client [{targets[i]}]");
                }
            }
        }
//----------------------------





        private void OnGUI()
        {
            GUILayout.Space(32);
            GUILayout.Label("Users Connected");
            GUILayout.Space(32);
            foreach (var user in NetUsers)
            {
                if (GUILayout.Button($"{user.Key} - {user.Value.UserName}"))
                {
                    //Kick the player
                    NetworkTransport.Disconnect(_SocketId, user.Key, out var err);
                }
            }
        }

    }//Class

}// Namescape