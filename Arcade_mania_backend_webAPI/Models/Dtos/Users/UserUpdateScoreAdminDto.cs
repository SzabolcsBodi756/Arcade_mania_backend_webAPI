namespace Arcade_mania_backend_webAPI.Models.Dtos.Users
{
    public class UserUpdateScoreAdminDto
    {

        public Guid GameId { get; set; }
        public int HighScore { get; set; }

    }
}
