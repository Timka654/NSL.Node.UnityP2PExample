using NSL.SocketCore.Utils.Buffer;
using System;

public class RoomJoinMemberMessageInfo
{
    public Guid UserId { get; set; }

    internal static RoomJoinMemberMessageInfo Read(InputPacketBuffer data)
    {
        return new RoomJoinMemberMessageInfo()
        {
            UserId = data.ReadGuid()
        };
    }
}