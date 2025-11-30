namespace Arcade_mania_backend_webAPI.Models.Dtos
{
    public class UserCreateDto
    {
        public string Name { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int FighterHighScore { get; set; }
        public int MemoryHighScore { get; set; }
        public int SnakeHighScore { get; set; }
    }
}
