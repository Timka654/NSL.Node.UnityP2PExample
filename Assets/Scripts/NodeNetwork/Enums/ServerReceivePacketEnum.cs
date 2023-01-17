namespace Assets.Scripts.NodeNetwork.Enums
{
    public enum ServerReceivePacketEnum : ushort
    {
        CreateRoom = 1,
        JoinRoom,
        LeaveRoom,
        SendChatMessage,
        StartRoom,
        RemoveRoom,
        GetRoomList,
        HandshakeResponse
    }
}
