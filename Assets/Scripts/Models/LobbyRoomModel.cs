using NSL.Node.LobbyServerExample.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LobbyRoomModel : BaseLobbyRoomModel
{
    public bool PasswordEnabled { get; set; }

    public Guid OwnerId { get; set; }

    public List<Guid> Members { get; set; }
}
