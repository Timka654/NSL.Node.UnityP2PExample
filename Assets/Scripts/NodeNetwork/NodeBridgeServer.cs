using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.WebSocketsClient;
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

public class NodeBridgeServer
{
    public delegate void OnReceiveSignSessionResultDelegate(bool result, Uri wssUrl, IEnumerable<IPEndPoint> servers);

    private readonly IEnumerable<Uri> wssUrls;

    private const ushort SignSessionPID = 1;
    private const ushort SignSessionResultPID = SignSessionPID;

    private Dictionary<Uri, WSNetworkClient<BridgeNetworkClient, WSClientOptions<BridgeNetworkClient>>> connections;

    public NodeBridgeServer(IEnumerable<string> wssUrls) : this(wssUrls.Select(x => new Uri(x))) { }

    public NodeBridgeServer(IEnumerable<Uri> wssUrls) { this.wssUrls = wssUrls; }

    public NodeBridgeServer(string wssUrl) : this(Enumerable.Repeat(wssUrl, 1).ToArray()) { }

    public NodeBridgeServer(Uri wssUrl) : this(Enumerable.Repeat(wssUrl, 1).ToArray()) { }

    public int Connect(string serverIdentity, string sessionIdentity, int maxCount = 1, int connectionTimeout = 2000)
    {
        var serverCount = tryConnect(maxCount, connectionTimeout);

        if (serverCount > 0)
            trySign(serverIdentity, sessionIdentity);

        return serverCount;
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
                    //builder.AddPacketHandle(SignSessionResultPID, OnSignSessionReceive);
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

    private bool trySign(string serverIdentity, string sessionIdentity)
    {
        OutputPacketBuffer packet = new OutputPacketBuffer();

        packet.PacketId = SignSessionPID;

        packet.WriteString16(serverIdentity);
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
            OnAvailableBridgeServersResult(data.ReadBool(), new Uri(data.ReadString16()), data.ReadCollection(() => new IPEndPoint(IPAddress.Parse(data.ReadString16()), data.ReadInt32())));
    }

    public OnReceiveSignSessionResultDelegate OnAvailableBridgeServersResult = (result, wssUrl, servers) => { };

    private class BridgeNetworkClient : BaseSocketNetworkClient
    {

    }
}
