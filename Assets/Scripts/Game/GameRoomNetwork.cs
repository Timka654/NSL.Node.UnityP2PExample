using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class GameRoomNetwork : MonoBehaviour
{
    public NodeBridgeClient bridgeClient;

    internal void Initialize(NodeLobbyNetwork.RoomStartInfo startupInfo)
    {
        bridgeClient = new NodeBridgeClient(startupInfo.ConnectionEndPoints);

        bridgeClient.OnAvailableBridgeServersResult = (result, from, servers) =>
        {
            Debug.Log($"Result {result} from {from}");
        };

        int serverCount = bridgeClient.Connect(startupInfo.ServerIdentity, startupInfo.RoomId, startupInfo.Token);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
