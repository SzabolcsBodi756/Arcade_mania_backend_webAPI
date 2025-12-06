using System;
using System.Collections.Generic;

namespace Arcade_mania_backend_webAPI.Models;

public partial class Game
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<UserHighScore> UserHighScores { get; set; } = new List<UserHighScore>();
}
