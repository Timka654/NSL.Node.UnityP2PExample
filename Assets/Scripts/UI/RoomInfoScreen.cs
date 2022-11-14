using NSL.Node.LobbyServerExample.Shared.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.ObjectChangeEventStream;
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

    public LobbyRoomModel CurrentRoom => roomNetworkManager.CurrentRoom;

    private NodeLobbyNetwork network => roomNetworkManager.GetNetwork();

    private void Start()
    {
        network.OnRoomStartedMessage -= Network_OnRoomStartedMessage;
        network.OnRoomStartedMessage += Network_OnRoomStartedMessage;
        network.OnRoomChatMessage -= Network_OnRoomChatMessage;
        network.OnRoomChatMessage += Network_OnRoomChatMessage;
        network.OnRoomJoinMemberMessage -= Network_OnRoomJoinMemberMessage;
        network.OnRoomJoinMemberMessage += Network_OnRoomJoinMemberMessage;
        network.OnRoomLeaveMemberMessage -= Network_OnRoomLeaveMemberMessage;
        network.OnRoomLeaveMemberMessage += Network_OnRoomLeaveMemberMessage;
        network.OnLobbyRemoveRoomMessage -= Network_OnLobbyRemoveRoomMessage;
        network.OnLobbyRemoveRoomMessage += Network_OnLobbyRemoveRoomMessage;

        startBtn.onClick.RemoveAllListeners();
        startBtn.onClick.AddListener(OnStartBtnClick);
        backBtn.onClick.RemoveAllListeners();
        backBtn.onClick.AddListener(OnBackBtnClick);

        chatSendBtn.onClick.RemoveAllListeners();
        chatSendBtn.onClick.AddListener(OnChatSendBtnClick);

        RoomNameText.text = CurrentRoom?.Name ?? "";

        YouUIDText.text = $"You ID is {roomNetworkManager.GetNetwork().GetClientUID()}";
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

    private void Network_OnLobbyRemoveRoomMessage(System.Guid obj)
    {
        if (obj.Equals(CurrentRoom.Id))
            roomNetworkManager.OpenListRoomScreen();
    }

    private Dictionary<Guid, GameObject> members = new Dictionary<Guid, GameObject>();

    private void Network_OnRoomLeaveMemberMessage(Guid obj)
    {
        if (members.Remove(obj, out var go))
            Destroy(go);
    }

    private void Network_OnRoomJoinMemberMessage(NodeLobbyNetwork.RoomJoinMemberMessageInfo obj)
    {
        var member = GameObject.Instantiate(roomMemberPrefab, membersLayout.gameObject.transform);

        member.GetComponent<MemberItem>().Set(obj);

        members.Add(obj.UserId, member);
    }

    private void Network_OnRoomChatMessage(NodeLobbyNetwork.RoomChatMessageInfo obj)
    {
        var member = GameObject.Instantiate(chatMessagePrefab, chatLayout.gameObject.transform);

        member.GetComponent<TMP_Text>().text = $"{obj.From} - {obj.Content}";
    }

    private void Network_OnRoomStartedMessage(NodeLobbyNetwork.RoomStartInfo obj)
    {

    }

    public void Show()
        => gameObject.SetActive(true);

    public void Hide()
        => gameObject.SetActive(false);
}
