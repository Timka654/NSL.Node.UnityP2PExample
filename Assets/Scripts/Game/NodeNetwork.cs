using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.SocketCore.Unity;
using NSL.BuilderExtensions.UDPClient;
using NSL.BuilderExtensions.UDPServer;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer.Utils;
using NSL.UDP.Client;
using NSL.UDP.Client.Info;
using NSL.UDP.Client.Interface;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static NodeBridgeClient;
using static NodeLobbyNetwork;

public class NodeNetwork : MonoBehaviour
{
    public NodeBridgeClient bridgeClient;

    public NodeTransportClient transportClient;

    private RoomStartInfo roomStartInfo;

    private UDPServer<UDPNodeServerNetworkClient> endPoint;

    private string endPointConnectionUrl;

    private ConcurrentDictionary<Guid, NodeClient> connectedClients = new ConcurrentDictionary<Guid, NodeClient>();

    private List<StunServerInfo> STUNServers = new List<StunServerInfo>()
    {
        new StunServerInfo("stun.l.google.com:19302"),
        new StunServerInfo("stun1.l.google.com:19302"),
        new StunServerInfo("stun2.l.google.com:19302"),
        new StunServerInfo("stun3.l.google.com:19302"),
        new StunServerInfo("stun4.l.google.com:19302")
    };

    /// <summary>
    /// Can set how transport all data - P2P, Proxy, All
    /// default: All
    /// </summary>
    public NodeTransportMode TransportMode { get; set; } = NodeTransportMode.All;

    /// <summary>
    /// 1 unit = 1 second
    /// for no wait connections set this value to default = 0
    /// </summary>
    public int MaxNodesWaitCycle = 10;

    /// <summary>
    /// Receive transport servers from bridge server delay before continue
    /// </summary>
    public int WaitBridgeDelayMS = 3000;

    public event OnChangeRoomStateDelegate OnChangeRoomState = state => { };
    public event OnChangeNodesReadyDelegate OnChangeNodesReady = (current, total) => { };
    public event OnChangeNodesReadyDelayDelegate OnChangeNodesReadyDelay = (current, total) => { };

    /// <summary>
    /// Id for local enemy
    /// </summary>
    public Guid LocalNodeId { get; private set; } = Guid.Empty;

    internal async void Initialize(RoomStartInfo startupInfo, CancellationToken cancellationToken = default)
        => await InitializeAsync(startupInfo, cancellationToken);

    internal async Task InitializeAsync(RoomStartInfo startupInfo, CancellationToken cancellationToken = default)
    {
        roomStartInfo = startupInfo;

        await TryConnectAsync(cancellationToken);
    }

    private async void TryConnect(CancellationToken cancellationToken = default)
        => await TryConnectAsync(cancellationToken);

    private async Task TryConnectAsync(CancellationToken cancellationToken = default)
    {
#if DEBUG
        TransportMode = NodeTransportMode.ProxyOnly;
#endif

        try
        {
            OnChangeRoomState(RoomStateEnum.ConnectionBridge);

            bridgeClient = new NodeBridgeClient(roomStartInfo.ConnectionEndPoints);

            List<TransportSessionInfo> connectionPoints = new List<TransportSessionInfo>();

            bridgeClient.OnAvailableBridgeServersResult = (result, instance, from, servers) =>
            {
#if DEBUG
                Debug.Log($"Result {result} from {from}");
#endif

                if (result)
                    connectionPoints.AddRange(servers);
            };

            int serverCount = bridgeClient.Connect(roomStartInfo.ServerIdentity, roomStartInfo.RoomId, roomStartInfo.Token);

            if (serverCount == default)
                throw new Exception($"Can't find working servers");

            await WaitBridgeAsync(connectionPoints, cancellationToken);

            if (TransportMode.HasFlag(NodeTransportMode.P2POnly))
                CreateUdpEndPoint();

            InitializeTransportClients(connectionPoints, cancellationToken);

            await WaitNodeConnection(cancellationToken);

            OnChangeRoomState(RoomStateEnum.Ready);

        }
        catch (TaskCanceledException)
        {
            // todo: dispose all
            throw;
        }
    }

    private async Task WaitBridgeAsync(List<TransportSessionInfo> connectionPoints, CancellationToken cancellationToken = default)
    {
        OnChangeRoomState(RoomStateEnum.WaitTransportServerList);

#if DEBUG
        await Task.Delay(2000, cancellationToken);
#endif

        await Task.Delay(WaitBridgeDelayMS, cancellationToken);

        if (!connectionPoints.Any())
            throw new Exception($"WaitAndRun : Can't find any working servers");
    }

    private void CreateUdpEndPoint()
    {
        endPoint = UDPServerEndPointBuilder
            .Create()
            .WithClientProcessor<UDPNodeServerNetworkClient>()
            .WithOptions<UDPServerOptions<UDPNodeServerNetworkClient>>()
            .WithBindingPoint(new IPEndPoint(IPAddress.Any, 0))
            .WithCode(builder =>
            {
                var options = builder.GetOptions() as ISTUNOptions;

                options.StunServers.AddRange(STUNServers);

                builder.AddExceptionHandleForUnity((ex, c) =>
                {
                    Debug.LogError(ex.ToString());
                });
            })
            .Build();

        endPoint.Start();

        if (endPoint?.StunInformation != null)
            endPointConnectionUrl = $"udp://{endPoint.StunInformation.PublicEndPoint.Address}:{endPoint.StunInformation.PublicEndPoint.Port}";
        else
            endPointConnectionUrl = default;
    }

    private void InitializeTransportClients(List<TransportSessionInfo> connectionPoints, CancellationToken cancellationToken = default)
    {
        OnChangeRoomState(RoomStateEnum.ConnectionTransportServers);

        var point = connectionPoints.First();

        transportClient = new NodeTransportClient(point.ConnectionUrl);

        transportClient.OnSignOnServerResult = (result, instance, url) =>
        {
            if (result)
                return;

            Debug.LogError($"Cannot sign on {nameof(NodeTransportClient)}");
        };

        transportClient.OnChangeNodeList = (data, instance) =>
        {
            foreach (var item in data)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var nodeClient = connectedClients.GetOrAdd(item.NodeId, id => new NodeClient(item, this, instance));

                if (!nodeClient.TryConnect(item) && nodeClient.State == NodeClientState.None)
                    throw new Exception($"Cannot connect");
            }

            OnChangeNodesReady(data.Count(), roomStartInfo.TotalPlayerCount);
        };

        if (cancellationToken.IsCancellationRequested)
            throw new TaskCanceledException();

        if (transportClient.Connect(LocalNodeId = point.Id, roomStartInfo.Token, endPointConnectionUrl) == default)
            throw new Exception($"WaitAndRun : Can't find working transport servers");
    }

    private async Task WaitNodeConnection(CancellationToken cancellationToken = default)
    {
        OnChangeRoomState(RoomStateEnum.WaitConnections);

        for (int i = 1; i < MaxNodesWaitCycle + 1 && connectedClients.Count < roomStartInfo.TotalPlayerCount - 1; i++)
        {
            await Task.Delay(1000, cancellationToken);

            OnChangeNodesReadyDelay(i, MaxNodesWaitCycle);
        }

        await Task.Delay(500, cancellationToken);

        await transportClient.SendReady(connectedClients.Select(x => x.Key), roomStartInfo.TotalPlayerCount);
    }

    public void Broadcast(Action<OutputPacketBuffer> builder)
    {
        Parallel.ForEach(connectedClients, c => { c.Value.Transport(builder); });

        //var packet = new OutputPacketBuffer().WithPid(NodeTransportPacketEnum.Broadcast);
    }
}


public class UDPNodeServerNetworkClient : IServerNetworkClient
{

}

public delegate void OnChangeRoomStateDelegate(RoomStateEnum state);
public delegate void OnChangeNodesReadyDelegate(int current, int total);
public delegate void OnChangeNodesReadyDelayDelegate(int current, int total);

public enum RoomStateEnum
{
    ConnectionBridge,
    WaitTransportServerList,
    ConnectionTransportServers,
    WaitConnections,
    Ready
}

[Flags]
public enum NodeTransportMode
{
    P2POnly = 1,
    ProxyOnly = 2,
    All = P2POnly | ProxyOnly
}