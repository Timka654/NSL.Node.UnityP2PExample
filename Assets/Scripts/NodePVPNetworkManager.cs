using NSL.Node.LobbyServerExample.Shared.Enums;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NodePVPNetworkManager : MonoBehaviour
{
    [SerializeField] private string lobbyUrl = "wss://localhost:50981/lobby_ws";

    [SerializeField] private string gameSceneName;
    [SerializeField] private GameObject welcomeScreen;
    [SerializeField] private GameObject searchScreen;

    private NodePVPLobbyClient lobby;

    protected virtual void Awake()
    {
        lobby = new NodePVPLobbyClient(lobbyUrl);
        lobby.OnRoomStartedMessage += Network_OnRoomStartedMessage;
        lobby.OnStateChange += Lobby_OnStateChange;
    }

    private void ChangeSearchProcess(bool state)
    {
        welcomeScreen.SetActive(!state);
        searchScreen.SetActive(state);
    }

    private void Lobby_OnStateChange(bool state)
    {
        if (!state)
            ChangeSearchProcess(false);
    }

    public async Task<bool> ConnectToLobby()
    {
        bool result = lobby.State;
        if (!result)
        {
            lobby.Disconnect(); // for death connection of ping alive
            result = await lobby.Connect();
        }

        return result;
    }

    public async void StartSearch()
    {
        if (!await ConnectToLobby())
            return;

        lobby.Send(OutputPacketBuffer.Create(LobbyPacketEnum.FindOpponent));
        ChangeSearchProcess(true);
    }

    public void CancelSearch()
    {
        lobby.Send(OutputPacketBuffer.Create(LobbyPacketEnum.CancelSearch));
        ChangeSearchProcess(false);
    }

    private void Network_OnRoomStartedMessage(RoomStartInfo startupInfo)
    {
        SceneManager.LoadSceneAsync(gameSceneName).completed += _ =>
        {
            try
            {
                var obj = FindObjectsOfType<UnityNodeRoom>().SingleOrDefault();

                obj.Initialize(startupInfo);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            // todo : game loading
        };
    }

}
