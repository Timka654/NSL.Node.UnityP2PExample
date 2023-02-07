using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Node.BridgeTransportClient.Shared
{
    public interface IRoomInfo
    {
        void Broadcast(OutputPacketBuffer packet);

        PlayerInfo GetPlayer(Guid id);

        void SendTo(Guid nodeId, OutputPacketBuffer packet);

        void SendTo(PlayerInfo player, OutputPacketBuffer packet, bool disposeOnSend = true);

        void RegisterHandle(ushort command, ReciveHandleDelegate action);
        void Execute(ushort command, Action<OutputPacketBuffer> build);
        void SendToGameServer(OutputPacketBuffer packet);
    }
}
