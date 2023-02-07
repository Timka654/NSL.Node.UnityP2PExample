#if !SERVER

using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NSL.Node.BridgeTransportClient.Shared
{
    public partial class GameInfo
    {
        private void code11_handle(PlayerInfo player, InputPacketBuffer buffer)
        {

            Debug.Log($"receive {buffer.ReadFloat()} from {player.Id}");
        }
    }
}

#endif