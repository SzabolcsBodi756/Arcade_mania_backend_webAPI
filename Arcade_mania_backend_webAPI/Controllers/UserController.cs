using Arcade_mania_backend_webAPI.Models;
using Arcade_mania_backend_webAPI.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arcade_mania_backend_webAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly GameScoresDbContext _context;

        public UsersController(GameScoresDbContext context)
        {
            _context = context;
        }

        // GET: api/Users  -> összes user (jelszó nélkül)
        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new UserGetDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        FighterHighScore = u.FighterHighScore,
                        MemoryHighScore = u.MemoryHighScore,
                        SnakeHighScore = u.SnakeHighScore
                    })
                    .ToListAsync();

                return StatusCode(200, new { message = "Sikeres lekérdezés", result = users });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // GET: api/Users/debug  -> összes user, JELSZÓVAL (csak teszt!)
        [HttpGet("debug")]
        public async Task<ActionResult> GetUsersWithPassword()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new UserGetDto_allData
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Password = u.Password,
                        FighterHighScore = u.FighterHighScore,
                        MemoryHighScore = u.MemoryHighScore,
                        SnakeHighScore = u.SnakeHighScore
                    })
                    .ToListAsync();

                return StatusCode(200, new { message = "Sikeres lekérdezés (debug)", result = users });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // GET: api/Users/5  -> egy user ID alapján (jelszó nélkül)
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user != null)
                {
                    var dto = new UserGetByIdDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        FighterHighScore = user.FighterHighScore,
                        MemoryHighScore = user.MemoryHighScore,
                        SnakeHighScore = user.SnakeHighScore
                    };

                    return StatusCode(200, new { message = "Sikeres lekérdezés", result = dto });
                }

                return StatusCode(404, new { message = "Nincs ilyen ID", result = "" });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // GET: api/Users/5/debug  -> egy user ID alapján, JELSZÓVAL (csak teszt!)
        [HttpGet("{id}/debug")]
        public async Task<ActionResult> GetUserWithPassword(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user != null)
                {
                    var dto = new UserGetByIdDto_allData
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Password = user.Password,
                        FighterHighScore = user.FighterHighScore,
                        MemoryHighScore = user.MemoryHighScore,
                        SnakeHighScore = user.SnakeHighScore
                    };

                    return StatusCode(200, new { message = "Sikeres lekérdezés (debug)", result = dto });
                }

                return StatusCode(404, new { message = "Nincs ilyen ID", result = "" });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // POST: api/Users  -> új user felvitele, NÉV EGYEDISÉG ELLENŐRZÉSSEL (TESZT, password visszaadva)
        [HttpPost]
        public async Task<ActionResult> CreateUser(UserCreateDto dto)
        {
            try
            {
                var nameExists = await _context.Users.AnyAsync(u => u.Name == dto.Name);
                if (nameExists)
                {
                    return StatusCode(409, new { message = "Már létezik ilyen nevű felhasználó.", result = "" });
                }

                var user = new User
                {
                    Name = dto.Name,
                    Password = dto.Password, // TESZT: később hash!
                    FighterHighScore = dto.FighterHighScore,
                    MemoryHighScore = dto.MemoryHighScore,
                    SnakeHighScore = dto.SnakeHighScore
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var resultDto = new UserGetByIdDto_allData
                {
                    Id = user.Id,
                    Name = user.Name,
                    Password = user.Password,
                    FighterHighScore = user.FighterHighScore,
                    MemoryHighScore = user.MemoryHighScore,
                    SnakeHighScore = user.SnakeHighScore
                };

                return StatusCode(201, new { message = "Sikeres hozzáadás", result = resultDto });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // POST: api/Users/register  -> csak Name + Password, minden score 0
        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRegisterDto dto)
        {
            try
            {
                var exists = await _context.Users.AnyAsync(u => u.Name == dto.Name);
                if (exists)
                {
                    return StatusCode(409, new { message = "Ez a név már foglalt.", result = "" });
                }

                var user = new User
                {
                    Name = dto.Name,
                    Password = dto.Password, // TESZT: később hash!
                    FighterHighScore = 0,
                    MemoryHighScore = 0,
                    SnakeHighScore = 0
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                return StatusCode(201, new
                {
                    message = "Sikeres regisztráció",
                    result = new { user.Id, user.Name }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // POST: api/Users/login
        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDto dto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Name == dto.Name);

                if (user == null || user.Password != dto.Password)
                {
                    return StatusCode(401, new { message = "Hibás felhasználónév vagy jelszó.", result = "" });
                }

                var result = new UserLoginResultDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    FighterHighScore = user.FighterHighScore,
                    MemoryHighScore = user.MemoryHighScore,
                    SnakeHighScore = user.SnakeHighScore
                };

                return StatusCode(200, new { message = "Sikeres bejelentkezés", result = result });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // POST: api/Users/{id}/fighter-score
        [HttpPost("{id}/fighter-score")]
        public async Task<ActionResult> UpdateFighterScore(int id, FighterScoreUpdateDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    user.FighterHighScore = dto.FighterHighScore;
                    await _context.SaveChangesAsync();

                    return StatusCode(200, new { message = "Fighter pontszám sikeresen frissítve", result = user.FighterHighScore });
                }

                return StatusCode(404, new { message = "Nincs ilyen ID", result = "" });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // POST: api/Users/{id}/memory-score
        [HttpPost("{id}/memory-score")]
        public async Task<ActionResult> UpdateMemoryScore(int id, MemoryScoreUpdateDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    user.MemoryHighScore = dto.MemoryHighScore;
                    await _context.SaveChangesAsync();

                    return StatusCode(200, new { message = "Memory pontszám sikeresen frissítve", result = user.MemoryHighScore });
                }

                return StatusCode(404, new { message = "Nincs ilyen ID", result = "" });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // POST: api/Users/{id}/snake-score
        [HttpPost("{id}/snake-score")]
        public async Task<ActionResult> UpdateSnakeScore(int id, SnakeScoreUpdateDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    user.SnakeHighScore = dto.SnakeHighScore;
                    await _context.SaveChangesAsync();

                    return StatusCode(200, new { message = "Snake pontszám sikeresen frissítve", result = user.SnakeHighScore });
                }

                return StatusCode(404, new { message = "Nincs ilyen ID", result = "" });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }
    }
}
