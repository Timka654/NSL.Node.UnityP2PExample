using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;

public class RoomStartInfo
{
    public string Token { get; set; }

    public Guid RoomId { get; set; }

    public string ServerIdentity { get; set; }

    public List<string> ConnectionEndPoints { get; set; }

    public int TotalPlayerCount { get; set; }

    internal static RoomStartInfo Read(InputPacketBuffer data)
    {
        return new RoomStartInfo()
        {
            RoomId = data.ReadGuid(),
            Token = data.ReadString16(),
            ServerIdentity = data.ReadString16(),
            ConnectionEndPoints = data.ReadCollection(() => data.ReadString16()).ToList(),
            TotalPlayerCount = data.ReadInt32()
        };
    }
}