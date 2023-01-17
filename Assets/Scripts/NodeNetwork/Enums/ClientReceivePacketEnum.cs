namespace Assets.Scripts.NodeNetwork.Enums
{
    public enum ClientReceivePacketEnum : ushort
    {
        CreateRoomResult = 1,
        NewRoomMessage,
        ChangeTitleRoomInfo,
        ChangeRoomInfo,
        RoomMemberJoinMessage,
        RoomMemberLeaveMessage,
        JoinRoomResult,
        LeaveRoomResult,
        ChatMessage,
        NewUserIdentity,
        RoomStartedMessage,
        RoomRemoveMessage,
        GetRoomListResult,
        ErrorHandShake
    }
}
