using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEditor.Progress;

public class ListRoomScreen : MonoBehaviour
{
    [SerializeField] private NodeRoomNetworkManager roomNetworkManager;

    [SerializeField] private GameObject ItemPrefab;
    [SerializeField] private GameObject StackPanel;

    private List<ListRoomInfoModel> rooms;

    private NodeLobbyNetwork network => roomNetworkManager.GetNetwork();

    private void Start()
    {
        network.OnLobbyNewRoomMessage -= RoomNetworkManager_OnLobbyNewRoomMessage;
        network.OnLobbyNewRoomMessage += RoomNetworkManager_OnLobbyNewRoomMessage;

        network.OnLobbyRemoveRoomMessage -= RoomNetworkManager_OnLobbyRemoveRoomMessage;
        network.OnLobbyRemoveRoomMessage += RoomNetworkManager_OnLobbyRemoveRoomMessage;

        network.OnLobbyChangeTitleRoomInfoMessage -= RoomNetworkManager_OnLobbyChangeTitleRoomInfoMessage;
        network.OnLobbyChangeTitleRoomInfoMessage += RoomNetworkManager_OnLobbyChangeTitleRoomInfoMessage;

        network.OnLobbyChangeTitleRoomInfoMessage -= RoomNetworkManager_OnLobbyChangeTitleRoomInfoMessage;
        network.OnLobbyChangeTitleRoomInfoMessage += RoomNetworkManager_OnLobbyChangeTitleRoomInfoMessage;
    }

    private void RoomNetworkManager_OnLobbyChangeTitleRoomInfoMessage(NodeLobbyNetwork.ChangeTitleRoomInfo obj)
    {
        var room = rooms.Find(x => x.Room.Id.Equals(obj));

        if (room == null)
            return;

        room.RoomComponent.ChangeData(obj);
    }

    private void RoomNetworkManager_OnLobbyRemoveRoomMessage(System.Guid obj)
    {
        var room = rooms.Find(x => x.Room.Id.Equals(obj));

        if (room == null)
            return;

        rooms.Remove(room);

        Destroy(room.Element);
    }

    private void RoomNetworkManager_OnLobbyNewRoomMessage(LobbyRoomModel obj)
    {
        rooms.Add(CreateItem(obj));
    }

    private ListRoomInfoModel CreateItem(LobbyRoomModel room)
    {
        var result = new ListRoomInfoModel()
        {
            Room = room,
            Element = GameObject.Instantiate(ItemPrefab, StackPanel.transform)
        };

        result.RoomComponent = result.Element.GetComponent<LobbyListItem>();

        result.RoomComponent.SetData(room, roomNetworkManager);

        return result;
    }

    public async void Show()
    {
        gameObject.SetActive(true);

        if (await roomNetworkManager.ConnectToLobby())
            rooms = (await roomNetworkManager.GetRoomList()).Select(item => CreateItem(item)).ToList();
    }

    public void Hide()
        => gameObject.SetActive(false);
}

class ListRoomInfoModel
{
    public LobbyRoomModel Room { get; set; }

    public GameObject Element { get; set; }

    public LobbyListItem RoomComponent { get; set; }
}
