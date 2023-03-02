using UnityEngine;

public class RoomsWelcomeScreen : MonoBehaviour
{
    [SerializeField] private NodeRoomNetworkManager roomNetworkManager;

    public void MoveToLobby()
    {
        gameObject.SetActive(false);
        roomNetworkManager.OpenListRoomScreen();
    }
}
