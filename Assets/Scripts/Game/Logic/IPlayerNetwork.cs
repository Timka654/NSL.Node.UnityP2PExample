using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.BridgeTransportClient.Shared
{
    public interface IPlayerNetwork
    {
        void Transport(Action<OutputPacketBuffer> build, ushort code);

        void Transport(Action<OutputPacketBuffer> build);

        void Send(OutputPacketBuffer packet, bool disposeOnSend = true);
    }
}
