using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;
using System;

public class RoomLobbyNetworkClient : BaseSocketNetworkClient
{
    public PacketWaitBuffer waitBuffer { get; private set; }

    public Guid UID { get; set; }

    public RoomLobbyNetworkClient() : base()
    {
        waitBuffer = new PacketWaitBuffer(this);
    }
}