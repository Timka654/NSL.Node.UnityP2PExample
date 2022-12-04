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
using UnityEngine;

public class NodeTransportClient
{
    public delegate void OnReceiveSignSessionResultDelegate(bool result, NodeTransportClient instance, Uri from);

    private readonly IEnumerable<Uri> wssUrls;

    private Dictionary<Uri, WSNetworkClient<TransportNetworkClient, WSClientOptions<TransportNetworkClient>>> connections = new Dictionary<Uri, WSNetworkClient<TransportNetworkClient, WSClientOptions<TransportNetworkClient>>>();

    public NodeTransportClient(IEnumerable<string> wssUrls) : this(wssUrls.Select(x => new Uri(x))) { }

    public NodeTransportClient(IEnumerable<Uri> wssUrls) { this.wssUrls = wssUrls; }

    public NodeTransportClient(string wssUrl) : this(Enumerable.Repeat(wssUrl, 1).ToArray()) { }

    public NodeTransportClient(Uri wssUrl) : this(Enumerable.Repeat(wssUrl, 1).ToArray()) { }

    public int Connect(Guid playerIdentity, string sessionIdentity, string endPoint, int connectionTimeout = 2000)
    {
        var serverCount = tryConnect(connectionTimeout);

        if (serverCount > 0)
        {
            if (trySign(playerIdentity, sessionIdentity, endPoint))
                return serverCount;
        }

        return 0;
    }

    private int tryConnect(int connectionTimeout)
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
                .WithClientProcessor<TransportNetworkClient>()
                .WithOptions<WSClientOptions<TransportNetworkClient>>()
                .WithCode(builder =>
                {
                    builder.AddConnectHandle(client => client.Url = uri);
                    builder.AddPacketHandle(NodeTransportPacketEnum.SignSessionResult, OnSignSessionReceive);
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
        }

        return count;
    }

    private bool trySign(Guid playerIdentity, string sessionIdentity, string endPoint)
    {
        var packet = OutputPacketBuffer.Create(NodeTransportPacketEnum.SignSession);

        packet.WriteString16(sessionIdentity);
        packet.WriteGuid(playerIdentity);
        packet.WriteString16(endPoint);

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

    private void OnSignSessionReceive(TransportNetworkClient client, InputPacketBuffer data)
    {
        var result = data.ReadBool();

        if(result)
        {

        }

        OnSignOnServerResult(result, this, client.Url);
    }

    public OnReceiveSignSessionResultDelegate OnSignOnServerResult = (result, instance, from) => { };

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

    private class TransportNetworkClient : BaseSocketNetworkClient
    {
        public Uri Url { get; set; }
    }
}
