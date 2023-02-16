using TMPro;
using UnityEngine;

public class MemberItem : MonoBehaviour
{
    [SerializeField]private TMP_Text text;

    public void Set(NodeLobbyClient.RoomJoinMemberMessageInfo data)
    {
        Set(data.UserId.ToString());
    }
    public void Set(string name)
    {
        text.text = name;
    }
}
