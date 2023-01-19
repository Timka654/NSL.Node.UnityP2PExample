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

    public event OnChangeRoomStateDelegate OnChangeRoomState = state =>
    {
#if DEBUG
        Debug.Log($"{nameof(NodeNetwork)} change state -> {state}");
#endif
    };
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

        OnChangeRoomState -= OnChangeState;
        OnChangeRoomState += OnChangeState;

        await TryConnectAsync(cancellationToken);
    }

    public bool Ready { get; private set; }

    private void OnChangeState(RoomStateEnum state)
    {
        Ready = state == RoomStateEnum.Ready;
    }

    private async void TryConnect(CancellationToken cancellationToken = default)
        => await TryConnectAsync(cancellationToken);

    private async Task TryConnectAsync(CancellationToken cancellationToken = default)
    {
#if DEBUG
        TransportMode = NodeTransportMode.P2POnly;
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
        WaitBridgeDelayMS = 10_000;
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

                //builder.AddReceivePacketHandle(NodeTransportPacketEnum.Transport,)
            })
            .Build();

        endPoint.Start();

        if (endPoint?.StunInformation != null)
            endPointConnectionUrl = NSLEndPoint.FromIPAddress(
                NSLEndPoint.Type.UDP,
                endPoint.StunInformation.PublicEndPoint.Address,
                endPoint.StunInformation.PublicEndPoint.Port
                ).ToString();
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

        transportClient.OnRoomAllNodesReady += (createTime, srv_offs) =>
        {
#if DEBUG
            Debug.Log($"{nameof(transportClient.OnRoomAllNodesReady)} - {createTime} - {srv_offs}");
#endif
        };


        transportClient.OnChangeNodeList = (data, instance) =>
        {
            foreach (var item in data)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (item.NodeId == LocalNodeId)
                    continue;

                var nodeClient = connectedClients.GetOrAdd(item.NodeId, id => new NodeClient(item, this, instance));

                if (nodeClient.State == NodeClientState.None)
                {
                    nodeClient.RegisterHandle(11, (node, buffer) => { Debug.Log($"receive {buffer.ReadFloat()} from {node.PlayerId}"); });

                    if (!nodeClient.TryConnect(item))
                        throw new Exception($"Cannot connect");
                }
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
        do
        {
            OnChangeRoomState(RoomStateEnum.WaitConnections);

            for (int i = 0; i < MaxNodesWaitCycle && connectedClients.Count < roomStartInfo.TotalPlayerCount - 1; i++)
            {
                await Task.Delay(1000, cancellationToken);

                OnChangeNodesReadyDelay(i + 1, MaxNodesWaitCycle);
            }

        } while (!(await transportClient.SendReady(roomStartInfo.TotalPlayerCount, connectedClients.Select(x => x.Key).Append(LocalNodeId)) || MaxNodesWaitCycle == 0));
    }

    #region Transport

    public bool Broadcast(Action<OutputPacketBuffer> builder, ushort code)
    {
        if (!Ready)
            return false;

        Parallel.ForEach(connectedClients, c => { c.Value.Transport(builder, code); });

        //var packet = new OutputPacketBuffer().WithPid(NodeTransportPacketEnum.Broadcast);

        return true;
    }

    public bool Broadcast(Action<OutputPacketBuffer> builder)
    {
        if (!Ready)
            return false;
        Parallel.ForEach(connectedClients, c => { c.Value.Transport(builder); });

        //var packet = new OutputPacketBuffer().WithPid(NodeTransportPacketEnum.Broadcast);

        return true;
    }

    public bool SendTo(NodeClient node, Action<OutputPacketBuffer> builder, ushort code)
    {
        if (!Ready)
            return false;

        node.Transport(builder, code);

        return true;
    }

    public bool SendTo(NodeClient node, Action<OutputPacketBuffer> builder)
    {
        if (!Ready)
            return false;

        node.Transport(builder);

        return true;
    }

    public bool SendTo(Guid nodeId, Action<OutputPacketBuffer> builder)
    {
        if (connectedClients.TryGetValue(nodeId, out var node))
            SendTo(node, builder);

        return false;
    }
    
    public bool SendTo(Guid nodeId, Action<OutputPacketBuffer> builder, ushort code)
    {
        if (connectedClients.TryGetValue(nodeId, out var node))
            SendTo(node, builder, code);

        return false;
    }

    #endregion

#if DEBUG

    private void Update()
    {
        if (Ready)
        {
            var delta = Time.deltaTime;

            Broadcast(p =>
            {
                p.WriteFloat(delta);
            }, 11);
        }
    }
#endif
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