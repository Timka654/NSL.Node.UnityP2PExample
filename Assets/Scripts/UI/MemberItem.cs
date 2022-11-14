using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MemberItem : MonoBehaviour
{
    private TMP_Text text;

    private void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    public void Set(NodeLobbyNetwork.RoomJoinMemberMessageInfo data)
    {
        text.text = data.UserId.ToString();
    }
}
