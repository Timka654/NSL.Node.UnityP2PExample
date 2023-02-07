using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Node.BridgeTransportClient.Shared
{
    public delegate void ReciveHandleDelegate(PlayerInfo nodePlayer, InputPacketBuffer buffer);

    public class PlayerInfo
    {
        public IPlayerNetwork Network { get; set; }

        public Guid Id { get; set; }
    }
}
