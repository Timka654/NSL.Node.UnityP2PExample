using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeClient
{
    public string Identity { get; }

    public Guid PlayerId { get; }

    public NodeTransportClient Proxy { get; }

    public string EndPoint { get; }

    public NodeClient(string identity, Guid playerId, NodeTransportClient proxy, string endPoint)
    {
        Identity = identity;
        PlayerId = playerId;
        Proxy = proxy;
        EndPoint = endPoint;
    }

    public bool TryConnect()
    {
        if (string.IsNullOrWhiteSpace(EndPoint))
            return true;




        return false;
    }
}
