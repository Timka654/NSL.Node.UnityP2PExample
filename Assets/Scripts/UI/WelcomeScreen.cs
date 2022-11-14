using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WelcomeScreen : MonoBehaviour
{
    [SerializeField] private NodeRoomNetworkManager roomNetworkManager;

    public void MoveToLobby()
    {
        gameObject.SetActive(false);
        roomNetworkManager.OpenListRoomScreen();
    }
}
