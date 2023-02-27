using UnityEngine;

public class GameRoomPlayer : MonoBehaviour
{
    public UnityNodeRoom NodeNetworkRoom;

    public NodeClient NodeNetworkPlayer;

    [SerializeField] private float speedDelta = 5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var elapsed = Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            MoveCommand(KeyCode.UpArrow, elapsed);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            MoveCommand(KeyCode.DownArrow, elapsed);
        if (Input.GetKeyDown(KeyCode.RightArrow))
            MoveCommand(KeyCode.RightArrow, elapsed);
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            MoveCommand(KeyCode.LeftArrow, elapsed);
    }

    private void MoveCommand(KeyCode code, float elapsed)
    {
        if (NodeNetworkPlayer.IsLocalNode) // is localPlayer
            NodeNetworkRoom.NodeNetwork.Broadcast(p =>
            {
                p.WriteInt16((short)code);
                p.WriteFloat(elapsed);
            });

        if (code == KeyCode.UpArrow)
            transform.position += new Vector3(0, 1) * elapsed * speedDelta;
        else if (code == KeyCode.DownArrow)
            transform.position -= new Vector3(0, 1) * elapsed * speedDelta;
        else if (code == KeyCode.RightArrow)
            transform.position += new Vector3(0, 1) * elapsed * speedDelta;
        else if (code == KeyCode.LeftArrow)
            transform.position -= new Vector3(0, 1) * elapsed * speedDelta;
    }
}
