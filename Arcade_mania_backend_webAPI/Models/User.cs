using System;
using System.Collections.Generic;

namespace Arcade_mania_backend_webAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Password { get; set; }

    public int FighterHighScore { get; set; }

    public int MemoryHighScore { get; set; }

    public int SnakeHighScore { get; set; }
}
