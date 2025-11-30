namespace Arcade_mania_backend_webAPI.Models.Dtos
{
    public class UserGetByIdDto_allData
    {

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int FighterHighScore { get; set; }
        public int MemoryHighScore { get; set; }
        public int SnakeHighScore { get; set; }

    }
}
