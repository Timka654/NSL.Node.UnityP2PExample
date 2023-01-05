using NSL.Node.LobbyServerExample.Shared.Enums;
using NSL.Node.LobbyServerExample.Shared.Models;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static NodeLobbyNetwork;

public class NodeRoomNetworkManager : MonoBehaviour
{
    [SerializeField] private string lobbyUrl;
    [SerializeField] private RoomInfoScreen RoomInfoScreen;
    [SerializeField] private CreateRoomScreen CreateRoomScreen;
    [SerializeField] private ListRoomScreen ListRoomScreen;

    private NodeLobbyNetwork lobby;

    public NodeLobbyNetwork GetNetwork() => lobby;

    protected virtual void Awake()
    {
        lobby = new NodeLobbyNetwork(lobbyUrl);
    }

    public async Task<bool> ConnectToLobby()
    {
        if (!lobby.State)
        {
            lobby.Disconnect(); // for death connection of ping alive
            await lobby.Connect();
        }
        return true;
    }

    public async Task<List<LobbyRoomModel>> GetRoomList()
    {
        var packet = WaitablePacketBuffer.Create(ServerReceivePacketEnum.GetRoomList);

        List<LobbyRoomModel> result = default;

        await lobby.Send(packet, input =>
        {
            result = input.ReadCollection(_ => new LobbyRoomModel()
            {
                Id = input.ReadGuid(),
                Name = input.ReadString16()
            }).ToList();

            return Task.CompletedTask;
        });

        return result;
    }

    public async Task<Guid> CreateRoom(string name, string password, int maxMembers)
    {
        var packet = WaitablePacketBuffer.Create(ServerReceivePacketEnum.CreateRoom);

        packet.WriteJson16(new { name, password, maxMembers });

        Guid rid = default;

        await lobby.Send(packet, input =>
        {
            var result = input.ReadBool();

            if (result)
                rid = input.ReadGuid();

            return Task.CompletedTask;
        });

        return rid;
    }

    public async Task<JoinResultEnum> ConnectToRoom(Guid id, string password)
    {
        var packet = WaitablePacketBuffer.Create(ServerReceivePacketEnum.JoinRoom);

        packet.WriteGuid(id);

        packet.WriteString16(password);


        JoinResultEnum result = JoinResultEnum.NotFound;

        LobbyRoomModel roomInfo = default;


        await lobby.Send(packet, input =>
        {
            result = (JoinResultEnum)input.ReadByte();

            if (result == JoinResultEnum.Ok)
            {
                CurrentRoom = new LobbyRoomModel()
                {
                    Id = input.ReadGuid(),
                    Name = input.ReadString16(),
                    OwnerId = input.ReadGuid(),
                    Members = input.ReadCollection(() => input.ReadGuid()).ToList()
                };
            }
            else
            {
                Debug.LogError($"Cannot connect to room, status -> {result}");
            }

            return Task.CompletedTask;
        });

        if (result == JoinResultEnum.Ok)
        {
            OpenRoomInfoScreen();
        }

        return result;
    }

    public bool SendChatMessage(string text)
    {
        var packet = OutputPacketBuffer.Create(ServerReceivePacketEnum.SendChatMessage);

        packet.WriteString16(text);

        return lobby.Send(packet);
    }

    public bool StartRoom()
    {
        var packet = OutputPacketBuffer.Create(ServerReceivePacketEnum.StartRoom);

        return lobby.Send(packet);
    }

    public async Task LeaveRoom()
    {
        if (CurrentRoom == default)
            return;

        var packet = WaitablePacketBuffer.Create(ServerReceivePacketEnum.LeaveRoom);

        bool result = default;

        await lobby.Send(packet, data =>
        {
            result = data.ReadBool();

            return Task.CompletedTask;
        });

        if (result)
        {
            CurrentRoom = default;

            OpenListRoomScreen();
        }
    }

    public LobbyRoomModel CurrentRoom { get; private set; }

    public void OpenListRoomScreen()
    {
        CreateRoomScreen.Hide();
        RoomInfoScreen.Hide();

        ListRoomScreen.Show();
    }

    public void OpenCreateRoomScreen()
    {
        ListRoomScreen.Hide();
        RoomInfoScreen.Hide();

        CreateRoomScreen.Show();
    }

    public void OpenRoomInfoScreen()
    {
        ListRoomScreen.Hide();
        CreateRoomScreen.Hide();

        RoomInfoScreen.Show();
        RoomInfoScreen.InitRoom();

    }
}
