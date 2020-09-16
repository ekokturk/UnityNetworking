using UnityEngine;
using System.Collections.Generic;
using System;

namespace LLNet
{
    [CreateAssetMenu(menuName = "LLNet/NetMessageContainer")]
    public class NetMessageContainer : ScriptableObject
    {

        [SerializeField]
        private ANetMessage[] _NetMessages;

        public Dictionary<NetMessageType, ANetMessage> NetMessagesMap { get; private set; }

        private void OnEnable()
        {
            MapMessages();
        }

        public void MapMessages()
        {
            NetMessagesMap = new Dictionary<NetMessageType, ANetMessage>(_NetMessages.Length);

            foreach (var item in _NetMessages)
            {
                if( item == null || NetMessagesMap.ContainsKey(item.MessageType))
                {
                    Debug.LogWarning($"Cannot Add Message [{item}]");
                }
                else
                {
                    NetMessagesMap[item.MessageType] = item;
                }
            }
            Debug.Log($"Mapping Done -> Added [{NetMessagesMap.Count}] messages!");
        }
    }
}
