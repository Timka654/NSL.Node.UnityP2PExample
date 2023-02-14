//this only example how devide client/server side logic

#if !SERVER

using NSL.SocketCore.Utils.Buffer;

namespace NSL.Node.RoomServer.Shared.Client.Core
{
    public partial class GameInfo
    {
        private void Initialize()
        {
            // example for client handle
            RoomInfo.RegisterHandle(1, testCommand_handle);
        }

        // example for client handle
        private void testCommand_handle(PlayerInfo player, InputPacketBuffer buffer)
        {
#if UNITY_64
            UnityEngine.Debug.Log($"receive testCommand from {player?.Id.ToString() ?? "Server"}");
#else
            System.Console.WriteLine($"receive testCommand from {player?.Id.ToString() ?? "Server"}");
#endif
        }
    }
}

#endif