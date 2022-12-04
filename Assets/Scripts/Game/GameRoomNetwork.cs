using NSL.BuilderExtensions.UDPClient;
using NSL.BuilderExtensions.UDPServer;
using NSL.SocketServer.Utils;
using NSL.UDP.Client;
using NSL.UDP.Client.Info;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using static NodeBridgeClient;
using static NodeLobbyNetwork;
using static UnityEditor.Progress;

public class GameRoomNetwork : MonoBehaviour
{
    public NodeBridgeClient bridgeClient;

    public NodeTransportClient transportClient;

    private NodeLobbyNetwork.RoomStartInfo roomStartInfo;

    private UDPServer<UDPNodeServerNetworkClient> endPoint;

    private List<StunServerInfo> STUNServers = new List<StunServerInfo>()
    {
        new StunServerInfo("stun.l.google.com:19302"),
        new StunServerInfo("stun1.l.google.com:19302"),
        new StunServerInfo("stun2.l.google.com:19302"),
        new StunServerInfo("stun3.l.google.com:19302"),
        new StunServerInfo("stun4.l.google.com:19302")
    };

    internal void Initialize(NodeLobbyNetwork.RoomStartInfo startupInfo)
    {
        roomStartInfo = startupInfo;

        bridgeClient = new NodeBridgeClient(startupInfo.ConnectionEndPoints);

        TryConnect();
    }

    private void TryConnect()
    {
        List<TransportSessionInfo> connectionPoints = new List<TransportSessionInfo>();

        bridgeClient.OnAvailableBridgeServersResult = (result, instance, from, servers) =>
        {
            Debug.Log($"Result {result} from {from}");

            if (result)
                connectionPoints.AddRange(servers);
        };

        int serverCount = bridgeClient.Connect(roomStartInfo.ServerIdentity, roomStartInfo.RoomId, roomStartInfo.Token);

        if (serverCount == default)
            throw new Exception($"Can't find working servers");

        WaitAndRun(connectionPoints);
    }

    private async void WaitAndRun(List<TransportSessionInfo> connectionPoints)
    {
#if DEBUG
        await System.Threading.Tasks.Task.Delay(2000);
#endif

        await System.Threading.Tasks.Task.Delay(2000);

        if (!connectionPoints.Any())
            throw new Exception($"WaitAndRun : Can't find working servers");


        endPoint = UDPServerEndPointBuilder
            .Create()
            .WithClientProcessor<UDPNodeServerNetworkClient>()
            .WithOptions<UDPServerOptions<UDPNodeServerNetworkClient>>()
            .WithBindingPoint(new IPEndPoint(IPAddress.Any, 0))
            .WithCode(builder => {
                var options = (UDPServerOptions<UDPNodeServerNetworkClient>)builder.GetOptions();

                options.StunServers.AddRange(STUNServers);
            })
            .Build();

        endPoint.Start();

        string publicIPEndPoint = $"{endPoint.StunInformation.PublicEndPoint.Address}:{endPoint.StunInformation.PublicEndPoint.Port}";

        var point = connectionPoints.First();

        transportClient = new NodeTransportClient(point.ConnectionUrl);

        transportClient.OnSignOnServerResult = (result, instance, url) =>
        {

        };

        if (transportClient.Connect(point.Id, roomStartInfo.Token, publicIPEndPoint) == default)
            throw new Exception($"WaitAndRun : Can't find working transport servers");

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


public class UDPNodeServerNetworkClient : IServerNetworkClient
{

}