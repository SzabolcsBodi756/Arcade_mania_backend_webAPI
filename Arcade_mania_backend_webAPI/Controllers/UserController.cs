using Arcade_mania_backend_webAPI.Models;
using Arcade_mania_backend_webAPI.Models.Dtos.Auth;
using Arcade_mania_backend_webAPI.Models.Dtos.Users;
using Arcade_mania_backend_webAPI.Models.Dtos.Scores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arcade_mania_backend_webAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ArcadeManiaDatasContext _context;

        public UsersController(ArcadeManiaDatasContext context)
        {
            _context = context;
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRegisterDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name) ||
                    string.IsNullOrWhiteSpace(dto.Password))
                {
                    return StatusCode(400, new { message = "Név és jelszó megadása kötelező.", result = "" });
                }

                bool exists = await _context.Users.AnyAsync(u => u.UserName == dto.Name);
                if (exists)
                {
                    return StatusCode(409, new { message = "Ez a név már foglalt.", result = "" });
                }

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = dto.Name,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // opcionális: 0-s score minden létező játékra
                var games = await _context.Games.ToListAsync();
                foreach (var game in games)
                {
                    _context.UserHighScores.Add(new UserHighScore
                    {
                        UserId = user.Id,
                        GameId = game.Id,
                        HighScore = 0u // uint
                    });
                }

                if (games.Count > 0)
                    await _context.SaveChangesAsync();

                var result = new UserDto
                {
                    Id = user.Id,
                    Name = user.UserName
                };

                return StatusCode(201, new { message = "Sikeres regisztráció", result });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDto dto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == dto.Name);

                if (user == null ||
                    !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                {
                    return StatusCode(401, new { message = "Hibás felhasználónév vagy jelszó.", result = "" });
                }

                var scores = await _context.UserHighScores
                    .Where(s => s.UserId == user.Id)
                    .Join(_context.Games,
                          s => s.GameId,
                          g => g.Id,
                          (s, g) => new GameScoreDto
                          {
                              GameId = g.Id,
                              GameName = g.Name,
                              HighScore = (int)s.HighScore // uint → int
                          })
                    .ToListAsync();

                var result = new UserLoginResultDto
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Scores = scores
                };

                return StatusCode(200, new { message = "Sikeres bejelentkezés", result });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // GET: api/users/{id}/scores
        [HttpGet("{id:guid}/scores")]
        public async Task<ActionResult> GetUserScores(Guid id)
        {
            try
            {
                bool userExists = await _context.Users.AnyAsync(u => u.Id == id);
                if (!userExists)
                {
                    return StatusCode(404, new { message = "Nincs ilyen felhasználó.", result = "" });
                }

                var scores = await _context.UserHighScores
                    .Where(s => s.UserId == id)
                    .Join(_context.Games,
                          s => s.GameId,
                          g => g.Id,
                          (s, g) => new GameScoreDto
                          {
                              GameId = g.Id,
                              GameName = g.Name,
                              HighScore = (int)s.HighScore // uint → int
                          })
                    .ToListAsync();

                return StatusCode(200, new { message = "Sikeres lekérdezés", result = scores });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // POST: api/users/{id}/scores
        [HttpPost("{id:guid}/scores")]
        public async Task<ActionResult> UpdateScore(Guid id, UpdateScoreDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return StatusCode(404, new { message = "Nincs ilyen felhasználó.", result = "" });
                }

                var game = await _context.Games.FindAsync(dto.GameId);
                if (game == null)
                {
                    return StatusCode(404, new { message = "Nincs ilyen játék.", result = "" });
                }

                var entry = await _context.UserHighScores
                    .FirstOrDefaultAsync(s => s.UserId == id && s.GameId == dto.GameId);

                if (entry == null)
                {
                    entry = new UserHighScore
                    {
                        UserId = id,
                        GameId = dto.GameId,
                        HighScore = (uint)dto.Score // int → uint
                    };
                    await _context.UserHighScores.AddAsync(entry);
                }
                else
                {
                    if (dto.Score > entry.HighScore)
                    {
                        entry.HighScore = (uint)dto.Score; // int → uint
                    }
                }

                await _context.SaveChangesAsync();

                return StatusCode(200, new
                {
                    message = "Pontszám sikeresen frissítve",
                    result = new
                    {
                        userId = id,
                        gameId = dto.GameId,
                        highScore = entry.HighScore
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }
    }
}
