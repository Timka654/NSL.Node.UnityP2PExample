using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NodeLobbyClient;

public class LobbyListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private Button connectBtn;

    NodeRoomNetworkManager manager;

    LobbyRoomModel room;

    private void Start()
    {
        connectBtn.onClick.RemoveAllListeners();
        connectBtn.onClick.AddListener(async () =>
        {
            await manager.ConnectToRoom(room.Id, string.Empty);
        });
    }

    public void SetData(LobbyRoomModel room, NodeRoomNetworkManager manager)
    {
        this.manager = manager;
        this.room = room;

        title.text = room.Name;
    }

    public void ChangeData(ChangeTitleRoomInfo data)
    { 

    }
}
