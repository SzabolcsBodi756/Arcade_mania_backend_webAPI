namespace Arcade_mania_backend_webAPI.Models.Dtos.Scores
{
    public class UpdateScoreDto
    {
        public Guid GameId { get; set; }
        public int Score { get; set; }
    }
}
