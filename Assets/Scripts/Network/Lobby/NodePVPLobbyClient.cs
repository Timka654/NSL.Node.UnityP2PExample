using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.SocketCore.Unity;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.Node.LobbyServerExample.Shared.Enums;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Client;
using System;
using System.Threading.Tasks;
using UnityEngine;

public partial class NodePVPLobbyClient
{
    WSNetworkClient<RoomLobbyNetworkClient, WSClientOptions<RoomLobbyNetworkClient>> client;

    RoomLobbyNetworkClient lobbyNetworkClient;

    public Guid GetClientUID() => lobbyNetworkClient?.UID ?? Guid.Empty;

    public NodePVPLobbyClient(string url) : this(new Uri(url))
    {

    }

    public NodePVPLobbyClient(Uri url)
    {
        client = WebSocketsClientEndPointBuilder.Create()
            .WithClientProcessor<RoomLobbyNetworkClient>()
            .WithOptions<WSClientOptions<RoomLobbyNetworkClient>>()
            .WithUrl(url)
            .WithCode(builder =>
            {
                builder.AddSendHandleForUnity((client, pid, len, stack) =>
                {
                    Debug.Log($"[Lobby Server] Send {pid}");
                });

                builder.AddReceiveHandleForUnity((client, pid, len) =>
                {
                    Debug.Log($"[Lobby Server] Receive {pid}");
                });

                builder.AddConnectHandle(client =>
                {
                    client.PingPongEnabled = true;
                    lobbyNetworkClient = client;
                    OnStateChange(lobbyNetworkClient?.GetState(true) == true);
                });

                builder.AddDisconnectHandle(client =>
                {
                    OnStateChange(State);
                });

                builder.AddExceptionHandle((ex, client) =>
                {
                    Debug.LogError(ex.ToString());
                });

                builder.AddPacketHandle(LobbyPacketEnum.StartupRoomInfo, (client, data) => OnRoomStartedMessage(RoomStartInfo.Read(data)));

            })
            .Build();
    }

    public bool State => lobbyNetworkClient?.GetState() == true;

    public Guid ClientUID => lobbyNetworkClient?.UID ?? default;

    public event Action<bool> OnStateChange = (state) => { };

    public event Action<RoomStartInfo> OnRoomStartedMessage = data => { };

    #region Connect

    public async void ConnectAsync(int timeout = 3000)
        => await Connect(timeout);

    public async Task<bool> Connect(int timeout = 3000)
        => await client.ConnectAsync(timeout);

    public void Disconnect()
        => client.Disconnect();

    #endregion

    #region Send

    public bool Send(OutputPacketBuffer buffer, bool disposeOnSend = true)
    {
        if (lobbyNetworkClient?.GetState(true) != true)
            return false;

        client.Send(buffer, disposeOnSend);

        return true;
    }

    public async Task<bool> Send(WaitablePacketBuffer buffer, Func<InputPacketBuffer, Task> onResult, bool disposeOnSend = true)
    {
        if (lobbyNetworkClient?.GetState(true) != true)
            return false;

        await lobbyNetworkClient.waitBuffer.SendWaitRequest(buffer, onResult, disposeOnSend);

        return true;
    }

#endregion
}