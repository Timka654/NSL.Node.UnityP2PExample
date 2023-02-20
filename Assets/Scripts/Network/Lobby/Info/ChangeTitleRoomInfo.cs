using NSL.SocketCore.Utils.Buffer;
using System;

public class ChangeTitleRoomInfo
{
    public Guid RoomId { get; set; }

    public int MaxMemberCount { get; set; }

    public int MemberCount { get; set; }

    internal static ChangeTitleRoomInfo Read(InputPacketBuffer data)
    {
        return new ChangeTitleRoomInfo()
        {
            RoomId = data.ReadGuid(),
            MaxMemberCount = data.ReadInt32(),
            MemberCount = data.ReadInt32()
        };
    }
}