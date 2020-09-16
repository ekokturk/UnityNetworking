using System;
using System.Collections;
using System.Collections.Generic;
using GameSavvy.Byterizer;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

namespace LLNet
{
    public class LLClient : MonoBehaviour
    {
        [SerializeField]
        private string _ServerAddress = "127.0.0.1";

        [SerializeField]
        private int _ServerPort = 27000;

        [SerializeField]
        private int _BufferSize = 1024;

        [SerializeField]
        private byte _ThreadPoolSize = 3;

        [SerializeField]
        private string _UserName;
        public string UserName { get => _UserName; }

        [SerializeField]
        private int _TeamNumber;
        public int TeamNumber { get => _TeamNumber; }

        public GameObject _PlayerPrefab;

        public int MyConnectionId;

        [SerializeField]
        private NetMessageContainer _NetMessages;

        public Dictionary<int, NetUser> NetUsers { get; private set; }
        public byte ReliableChannel { get; private set; }
        public byte UnreliableChannel { get; private set; }

        private int _SocketId = 0;
        private int _ServerConnectionId = 0;


        private void Start()
        {
            ConnectToServer();
        }

        private void ConnectToServer()
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
            _SocketId = NetworkTransport.AddHost(hostTopology, 0);

            byte error;
            _ServerConnectionId = NetworkTransport.Connect(_SocketId, _ServerAddress, _ServerPort, 0, out error);

            if (error == 0)
            {
                StartCoroutine(Receiver());
                Debug.Log($"@ConnectToServer -> {_SocketId} - {_ServerConnectionId}");
            }
            else
            {
                Debug.LogError($"@ConnectToServer -> {error}");
            }

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
                            OnDataEvent(recChannelId, recBuffer, recDataSize);
                            break;
                        }

                    case NetworkEventType.ConnectEvent:
                        {
                            Debug.Log($"@Receiver.Connect -> Socket = [{recSocketId}], userId = [{recConnectionId}]");
                            break;
                        }

                    case NetworkEventType.DisconnectEvent:
                        {
                            Debug.Log($"@Receiver.Disconnect -> Socket = [{recSocketId}], userId = [{recConnectionId}]");
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

        private void OnDataEvent(int channelId, byte[] data, int dataSize)
        {
            var msgData = new ByteStream(data, dataSize);
            NetMessageType msgType = (NetMessageType)msgData.PopByte();
            _NetMessages.NetMessagesMap[msgType].Client_ReceiveMessage(msgData, this);
        }

        // _TODO: Refactor the rest of the code, (Client and server)

        public void SendNetMessage(byte channelId, byte[] data)
        {
            NetworkTransport.Send
            (
                _SocketId,
                _ServerConnectionId,
                channelId,
                data,
                data.Length,
                out var error
            );

            if (error != 0)
            {
                Debug.LogError($"@Client -> Error: [{error}] : Could Not Send Message To Server");
            }
        }

        private Queue<string> _ChatMessages = new Queue<string>();
        public void AddMessageToQueue(string msg)
        {
            _ChatMessages.Enqueue(msg);
            if (_ChatMessages.Count >= 20)
            {
                _ChatMessages.Dequeue();
            }
        }

        private string _MsgToSend;
        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(32);
                    _MsgToSend = GUILayout.TextField(_MsgToSend);
                    GUILayout.Space(32);
                    if (GUILayout.Button("BROADCAST"))
                    {
                        var msg = new ByteStream();
                        msg.Encode((byte)NetMessageType.CHAT_BROADCAST, _MsgToSend);
                        SendNetMessage(ReliableChannel, msg.ToArray());
                        AddMessageToQueue($"BC > {_MsgToSend}");
                        _MsgToSend = "";
                    }
                    GUILayout.Space(32);
// ----------------------------------------------------------------
                    if (GUILayout.Button("TEAM"))                           // Team chat button
                    {
                        var msg = new ByteStream();
                        msg.Encode((byte)NetMessageType.CHAT_TEAM_MESSAGE, _TeamNumber, _MsgToSend);
                        SendNetMessage(ReliableChannel, msg.ToArray());     // Send team message to the server
                        AddMessageToQueue($"TEAM > {_MsgToSend}");          // Add your message to your queue
                        _MsgToSend = "";                                    // Clear input field
                    }
// ----------------------------------------------------------------
                    GUILayout.Space(32);
                    GUILayout.Label("Users Connected");
                    GUILayout.Space(32);
                    foreach (var user in NetUsers)
                    {
                        if (GUILayout.Button($"{user.Key} - {user.Value.UserName}"))
                        {
                            var msg = new ByteStream();
                            msg.Encode
                            (
                                (byte)NetMessageType.CHAT_WHISPER,
                                user.Value.ConnectionID,
                                _MsgToSend
                            );
                            SendNetMessage(ReliableChannel, msg.ToArray());
                            AddMessageToQueue($">>>> {_MsgToSend}");
                            _MsgToSend = "";
                        }
                    }
                }
                GUILayout.EndVertical();
                GUILayout.Space(40); // Horizontal space
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("Chat Messages");
                    GUILayout.Space(32);
                    foreach (var msg in _ChatMessages)
                    {
                        GUILayout.Label($"- {msg}");
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

    }//Class

}//NameSpace