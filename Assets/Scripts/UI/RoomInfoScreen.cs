using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomInfoScreen : MonoBehaviour
{
    [SerializeField] private NodeRoomNetworkManager roomNetworkManager;
    [SerializeField] private Button backBtn;
    [SerializeField] private Button startBtn;
    [SerializeField] private TMP_Text RoomNameText;
    [SerializeField] private TMP_Text YouUIDText;
    [SerializeField] private VerticalLayoutGroup membersLayout;
    [SerializeField] private GameObject roomMemberPrefab;

    [SerializeField] private VerticalLayoutGroup chatLayout;
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private Button chatSendBtn;
    [SerializeField] private GameObject chatMessagePrefab;

    [SerializeField] private string gameSceneName;

    public LobbyRoomModel CurrentRoom => roomNetworkManager.CurrentRoom;

    private NodeRoomLobbyClient network => roomNetworkManager.GetNetwork();

    public void InitRoom()
    {
        network.OnRoomStartedMessage += Network_OnRoomStartedMessage;
        network.OnRoomChatMessage += Network_OnRoomChatMessage;
        network.OnRoomJoinMemberMessage += Network_OnRoomJoinMemberMessage;
        network.OnRoomLeaveMemberMessage += Network_OnRoomLeaveMemberMessage;
        network.OnLobbyRemoveRoomMessage += Network_OnLobbyRemoveRoomMessage;

        startBtn.onClick.AddListener(OnStartBtnClick);
        backBtn.onClick.AddListener(OnBackBtnClick);
        chatSendBtn.onClick.AddListener(OnChatSendBtnClick);

        RoomNameText.text = CurrentRoom?.Name ?? "";
        YouUIDText.text = $"You ID is {roomNetworkManager.GetNetwork().GetClientUID()}";

        foreach (var item in CurrentRoom.Members)
        {
            joinMember(item, item.ToString());
        }
    }

    private void OnChatSendBtnClick()
    {
        var text = chatInput.text;

        if (roomNetworkManager.SendChatMessage(text))
            chatInput.text = string.Empty;
    }

    private void OnStartBtnClick()
    {
        roomNetworkManager.StartRoom();
    }

    private async void OnBackBtnClick()
    {
        await roomNetworkManager.LeaveRoom();
    }

    private void Network_OnLobbyRemoveRoomMessage(Guid obj)
    {
        if (obj.Equals(CurrentRoom?.Id))
            roomNetworkManager.OpenListRoomScreen();
    }

    private Dictionary<Guid, GameObject> members = new Dictionary<Guid, GameObject>();

    private void Network_OnRoomLeaveMemberMessage(Guid obj)
    {
        if (members.Remove(obj, out var go))
            Destroy(go);
    }

    private void Network_OnRoomJoinMemberMessage(RoomJoinMemberMessageInfo obj)
    {
        joinMember(obj.UserId, obj.UserId.ToString());
    }

    private void joinMember(Guid uid, string name)
    {
        var member = GameObject.Instantiate(roomMemberPrefab, membersLayout.gameObject.transform);

        member.GetComponent<MemberItem>().Set(name);
        members.TryAdd(uid, member);
    }

    private void Network_OnRoomChatMessage(RoomChatMessageInfo obj)
    {
        var member = GameObject.Instantiate(chatMessagePrefab, chatLayout.gameObject.transform);

        member.GetComponent<TMP_Text>().text = $"{obj.From} - {obj.Content}";
    }

    private void Network_OnRoomStartedMessage(RoomStartInfo startupInfo)
    {
        SceneManager.LoadSceneAsync(gameSceneName).completed += _ =>
        {
            try
            {

                var obj = FindObjectsOfType<NodeNetwork>().SingleOrDefault();

                obj.Initialize(startupInfo);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            // todo : game loading
        };
    }

    public void Show()
        => gameObject.SetActive(true);

    public void Hide()
    {
        if (destroyed)
            return;

        network.OnRoomStartedMessage -= Network_OnRoomStartedMessage;
        network.OnRoomChatMessage -= Network_OnRoomChatMessage;
        network.OnRoomJoinMemberMessage -= Network_OnRoomJoinMemberMessage;
        network.OnRoomLeaveMemberMessage -= Network_OnRoomLeaveMemberMessage;
        network.OnLobbyRemoveRoomMessage -= Network_OnLobbyRemoveRoomMessage;

        startBtn.onClick.RemoveAllListeners();
        backBtn.onClick.RemoveAllListeners();
        chatSendBtn.onClick.RemoveAllListeners();

        foreach (Transform item in membersLayout.gameObject.transform)
        {
            Destroy(item.gameObject);
        }

        foreach (Transform item in chatLayout.gameObject.transform)
        {
            Destroy(item.gameObject);
        }

        members.Clear();

        gameObject.SetActive(false);
    }

    bool destroyed = false;

    private void OnDestroy()
    {
        destroyed = true;
    }
}
