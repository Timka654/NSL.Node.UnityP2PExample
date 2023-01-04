using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeSpawner : MonoBehaviour
{
    [SerializeField] private List<SpawnObject> registredObjects;

    private Dictionary<string, GameObject> registredObjectMap;

    private Dictionary<string, GameObject> sessionObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        registredObjectMap = registredObjects.ToDictionary(x => x.Id, x => x.Prefab);
    }

    public void MessageHandle(NodeClient client, InputPacketBuffer buffer)
    {
        var command = buffer.ReadByte();

        switch (command)
        {
            case 0:
                CreateObject(
                    client,
                    buffer.ReadString16(),
                    buffer.ReadString16(),
                    new Vector3(
                        buffer.ReadFloat(),
                        buffer.ReadFloat(),
                        buffer.ReadFloat()),
                    new Quaternion(
                        buffer.ReadFloat(),
                        buffer.ReadFloat(),
                        buffer.ReadFloat(),
                        buffer.ReadFloat())
                    );
                break;
            case 1:
                DestroyObject(client, buffer.ReadString16());
                break;
            default:
                break;
        }
    }

    void CreateObject(string objectId, string identity, Vector3 pos, Quaternion rotation)
    { 
    
    }

    void CreateObject(NodeClient client, string objectId, string identity, Vector3 pos, Quaternion rotation)
    {
        if (!registredObjectMap.TryGetValue(objectId, out GameObject obj))
            throw new KeyNotFoundException(objectId);

        var gobj = Instantiate(obj, pos, rotation);

        var noid = gobj.GetComponent<NodeObjectIdentity>();

        noid.Initialize(client, identity);
    }

    void DestroyObject(NodeClient client, string objectId)
    {
        if (sessionObjects.Remove(objectId, out var gobj))
        {
            Destroy(gobj);
        }

    }
}

public class NodeObjectIdentity : MonoBehaviour
{
    public NodeClient NodeClient { get; set; }

    public string Identity { get; set; }

    public void Initialize(NodeClient nodeClient, string identity)
    {
        this.NodeClient = nodeClient;
        this.Identity = identity;
    }
}

[Serializable]
public class SpawnObject
{
    public string Id;

    public GameObject Prefab;
}
