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

public partial class NodeRoomLobbyClient
{
    WSNetworkClient<RoomLobbyNetworkClient, WSClientOptions<RoomLobbyNetworkClient>> client;

    RoomLobbyNetworkClient lobbyNetworkClient;

    public Guid GetClientUID() => lobbyNetworkClient?.UID ?? Guid.Empty;

    public NodeRoomLobbyClient(string url) : this(new Uri(url))
    {

    }

    public NodeRoomLobbyClient(Uri url)
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

                builder.AddPacketHandle(ClientReceivePacketEnum.NewUserIdentity, (client, data) => { client.UID = data.ReadGuid(); Debug.Log($"New UID = {client.UID}"); });

                builder.AddReceivePacketHandle(ClientReceivePacketEnum.CreateRoomResult, client => client.waitBuffer);
                builder.AddReceivePacketHandle(ClientReceivePacketEnum.GetRoomListResult, client => client.waitBuffer);
                builder.AddReceivePacketHandle(ClientReceivePacketEnum.JoinRoomResult, client => client.waitBuffer);
                builder.AddReceivePacketHandle(ClientReceivePacketEnum.LeaveRoomResult, client => client.waitBuffer);

                builder.AddPacketHandle(ClientReceivePacketEnum.NewRoomMessage, (client, data) => OnLobbyNewRoomMessage(data.ReadJson16<LobbyRoomModel>()));
                builder.AddPacketHandle(ClientReceivePacketEnum.RoomRemoveMessage, (client, data) => OnLobbyRemoveRoomMessage(data.ReadGuid()));
                builder.AddPacketHandle(ClientReceivePacketEnum.ChangeTitleRoomInfo, (client, data) => OnLobbyChangeTitleRoomInfoMessage(ChangeTitleRoomInfo.Read(data)));

                builder.AddPacketHandle(ClientReceivePacketEnum.RoomStartedMessage, (client, data) => OnRoomStartedMessage(RoomStartInfo.Read(data)));
                builder.AddPacketHandle(ClientReceivePacketEnum.ChatMessage, (client, data) => OnRoomChatMessage(RoomChatMessageInfo.Read(data)));
                builder.AddPacketHandle(ClientReceivePacketEnum.RoomMemberJoinMessage, (client, data) => OnRoomJoinMemberMessage(RoomJoinMemberMessageInfo.Read(data)));
                builder.AddPacketHandle(ClientReceivePacketEnum.RoomMemberLeaveMessage, (client, data) => OnRoomLeaveMemberMessage(data.ReadGuid()));

            })
            .Build();
    }

    public bool State => lobbyNetworkClient?.GetState() == true;

    public Guid ClientUID => lobbyNetworkClient?.UID ?? default;

    public Action<bool> OnStateChange = (state) => { };

    public event Action<LobbyRoomModel> OnLobbyNewRoomMessage = (roomInfo) => { };

    public event Action<ChangeTitleRoomInfo> OnLobbyChangeTitleRoomInfoMessage = (roomInfo) => { };

    public event Action<Guid> OnLobbyRemoveRoomMessage = (roomId) => { };

    public event Action<RoomStartInfo> OnRoomStartedMessage = data => { };

    public event Action<RoomChatMessageInfo> OnRoomChatMessage = data => { };

    public event Action<RoomJoinMemberMessageInfo> OnRoomJoinMemberMessage = data => { };

    public event Action<Guid> OnRoomLeaveMemberMessage = data => { };

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