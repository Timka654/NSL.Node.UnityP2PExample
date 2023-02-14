using NSL.Node.LobbyServerExample.Shared.Models;
using System;
using System.Collections.Generic;

public class LobbyRoomModel : BaseLobbyRoomModel
{
    public bool PasswordEnabled { get; set; }

    public Guid OwnerId { get; set; }

    public List<Guid> Members { get; set; }
}
