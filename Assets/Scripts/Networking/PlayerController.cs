using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using GameSavvy.Byterizer;
using LLNet;

public class PlayerController : MonoBehaviour
{
    [SerializeField] 
    private LLClient _Client;
        
    [SerializeField] 
    private Camera _Camera;

    private void Update() 
    {
        if(_Client.isActiveAndEnabled == false) return;
        MoveToClickLocation();
    }

    private void MoveToClickLocation()
    {
        if(Input.GetMouseButtonDown(1))
        {
            Ray castPoint = _Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(castPoint, out var hit, Mathf.Infinity))
            {
                Debug.Log($"Moving to => {hit.point}");
                GameObject player = _Client.NetUsers[_Client.MyConnectionId].Player;
                if(player != null)
                    _Client.NetUsers[_Client.MyConnectionId].Player.GetComponent<NavMeshAgent>()?.SetDestination(hit.point);
                var msg = new ByteStream();
                msg.Encode((byte)NetMessageType.MOVEMENT, hit.point);
                _Client.SendNetMessage(_Client.ReliableChannel, msg.ToArray());
            }

        }

    }
}
