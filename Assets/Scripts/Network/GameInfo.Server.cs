//this only example how devide client/server side logic

#if SERVER

using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.Node.RoomServer.Shared.Client.Core
{
    public partial class GameInfo
    {
        private void Initialize()
        {
            // example for server handle
            RoomInfo.RegisterHandle(1, testCommand_handle);
        }

        // example for server handle
        private void testCommand_handle(PlayerInfo player, InputPacketBuffer buffer)
        {
            Console.WriteLine($"receive testCommand from {player?.Id.ToString() ?? "Server"}");
        }
    }
}
#endif