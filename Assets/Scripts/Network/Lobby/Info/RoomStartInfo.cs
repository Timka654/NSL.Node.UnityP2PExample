using NSL.SocketCore.Utils.Buffer;
using System.Linq;

public class RoomStartInfo : NodeSessionStartupModel
{
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