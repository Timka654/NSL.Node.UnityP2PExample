using NSL.SocketCore.Utils.Buffer;
using System;

public class RoomChatMessageInfo
{
    public Guid From { get; set; }

    public string Content { get; set; }

    internal static RoomChatMessageInfo Read(InputPacketBuffer data)
    {
        return new RoomChatMessageInfo()
        {
            From = data.ReadGuid(),
            Content = data.ReadString16()
        };
    }
}