
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSL.SocketCore.Utils.Buffer;
#if UNITY_64
using UnityEngine;
#else
using System.Diagnostics;
#endif

namespace NSL.Node.BridgeTransportClient.Shared
{
    public partial class GameInfo
    {
        public IRoomInfo RoomInfo { get; }

        public GameInfo(IRoomInfo roomInfo)
        {
            RoomInfo = roomInfo;

            roomInfo.RegisterHandle(11, code11_handle);
        }
    }
}
