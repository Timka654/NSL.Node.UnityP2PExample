using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.Node.BridgeServer.Shared.Enums;
using NSL.SocketClient;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class NodeBridgeClient
{
    public delegate void OnReceiveSignSessionResultDelegate(bool result, Uri from, IEnumerable<TransportSessionInfo> servers);

    private readonly IEnumerable<Uri> wssUrls;

    private Dictionary<Uri, WSNetworkClient<BridgeNetworkClient, WSClientOptions<BridgeNetworkClient>>> connections = new Dictionary<Uri, WSNetworkClient<BridgeNetworkClient, WSClientOptions<BridgeNetworkClient>>>();

    public NodeBridgeClient(IEnumerable<string> wssUrls) : this(wssUrls.Select(x => new Uri(x))) { }

    public NodeBridgeClient(IEnumerable<Uri> wssUrls) { this.wssUrls = wssUrls; }

    public NodeBridgeClient(string wssUrl) : this(Enumerable.Repeat(wssUrl, 1).ToArray()) { }

    public NodeBridgeClient(Uri wssUrl) : this(Enumerable.Repeat(wssUrl, 1).ToArray()) { }

    public int Connect(string serverIdentity, Guid roomId, string sessionIdentity, int maxCount = 1, int connectionTimeout = 2000)
    {
        var serverCount = tryConnect(maxCount, connectionTimeout);

        if (serverCount > 0)
        {
            if (trySign(serverIdentity, roomId, sessionIdentity))
                return serverCount;
        }

        return 0;
    }

    private int tryConnect(int maxCount, int connectionTimeout)
    {
        foreach (var item in connections)
        {
            if (item.Value.GetState())
                item.Value.Disconnect();
        }

        connections.Clear();

        var bridgeServers = wssUrls.ToDictionary(
            uri => uri,
            uri => WebSocketsClientEndPointBuilder.Create()
                .WithClientProcessor<BridgeNetworkClient>()
                .WithOptions<WSClientOptions<BridgeNetworkClient>>()
                .WithCode(builder =>
                {
                    builder.AddConnectHandle(client => client.Url = uri);
                    builder.AddPacketHandle(NodeBridgeClientPacketEnum.SignSessionResultPID, OnSignSessionReceive);
                })
                .WithUrl(uri)
                .Build());

        var count = 0;

        foreach (var item in bridgeServers)
        {
            if (!item.Value.Connect(connectionTimeout))
                continue;

            count++;

            connections.Add(item.Key, item.Value);

            if (count == maxCount)
                break;
        }

        return count;
    }

    private bool trySign(string serverIdentity, Guid roomId, string sessionIdentity)
    {
        var packet = OutputPacketBuffer.Create(NodeBridgeClientPacketEnum.SignSessionPID);

        packet.WriteString16(serverIdentity);
        packet.WriteGuid(roomId);
        packet.WriteString16(sessionIdentity);

        bool any = false;

        foreach (var item in connections)
        {
            if (!item.Value.GetState())
                continue;

            item.Value.Send(packet, false);

            any = true;
        }

        packet.Dispose();

        return any;
    }

    private void OnSignSessionReceive(BridgeNetworkClient client, InputPacketBuffer data)
    {
        if (OnAvailableBridgeServersResult != null)
        {
            var result = data.ReadBool();

            var sessions = result ? data.ReadCollection(() => new TransportSessionInfo(data.ReadString16(), data.ReadGuid())) : null;

            OnAvailableBridgeServersResult(result, client.Url, sessions);
        }
    }

    public OnReceiveSignSessionResultDelegate OnAvailableBridgeServersResult = (result, from, servers) => { };

    public class TransportSessionInfo
    {
        public TransportSessionInfo(string connectionUrl, Guid id)
        {
            ConnectionUrl = connectionUrl;
            Id = id;
        }

        public string ConnectionUrl { get; }

        public Guid Id { get; }
    }

    private class BridgeNetworkClient : BaseSocketNetworkClient
    {
        public Uri Url { get; set; }
    }
}
