using System;
using System.Collections.Generic;

namespace Arcade_mania_backend_webAPI.Models;

public partial class UserHighScore
{
    public Guid UserId { get; set; }

    public Guid GameId { get; set; }

    public uint HighScore { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
