using Arcade_mania_backend_webAPI.Models;
using Arcade_mania_backend_webAPI.Models.Dtos.Auth;
using Arcade_mania_backend_webAPI.Models.Dtos.Users;
using Arcade_mania_backend_webAPI.Models.Dtos.Scores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NETCore.Encrypt;

namespace Arcade_mania_backend_webAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ArcadeManiaDatasContext _context;
        private readonly string _passwordKey;   // ⬅️ kulcs config-ból

        public UsersController(ArcadeManiaDatasContext context, IConfiguration config)
        {
            _context = context;
            _passwordKey = config["Crypto:PasswordKey"]
                ?? throw new InvalidOperationException("Crypto:PasswordKey is not configured.");
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
                    // jelszó titkosítása kétirányú AES-sel, kulcs configból
                    PasswordHash = EncryptProvider.AESEncrypt(dto.Password, _passwordKey)
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

                if (user == null)
                {
                    return StatusCode(401, new { message = "Hibás felhasználónév vagy jelszó.", result = "" });
                }

                // jelszó visszafejtése az adatbázisból
                string storedPlainPassword;
                try
                {
                    storedPlainPassword = EncryptProvider.AESDecrypt(user.PasswordHash, _passwordKey);
                }
                catch
                {
                    return StatusCode(401, new { message = "Hibás felhasználónév vagy jelszó.", result = "" });
                }

                if (storedPlainPassword != dto.Password)
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



        // --------------------------
        //  ÚJ VÉGPONTOK – USER / ADMIN
        // --------------------------

        // GET: api/users/public  -> összes user (NINCS ID, NINCS jelszó)
        [HttpGet("public")]
        public async Task<ActionResult> GetAllUsersPublic()
        {
            try
            {
                var users = await _context.Users.ToListAsync();

                var allScores = await _context.UserHighScores
                    .Join(_context.Games,
                          s => s.GameId,
                          g => g.Id,
                          (s, g) => new
                          {
                              s.UserId,
                              GameId = g.Id,
                              GameName = g.Name,
                              HighScore = (int)s.HighScore
                          })
                    .ToListAsync();

                var result = users.Select(u => new UserDataPublicDto
                {
                    Name = u.UserName,
                    Scores = allScores
                        .Where(x => x.UserId == u.Id)
                        .Select(x => new GameScoreDto
                        {
                            GameId = x.GameId,
                            GameName = x.GameName,
                            HighScore = x.HighScore
                        })
                        .ToList()
                }).ToList();

                return StatusCode(200, new { message = "Sikeres lekérdezés (public)", result });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        [HttpGet("public/{id:guid}")] 
        public async Task<ActionResult> GetUserPublicById(Guid id) 
        { 
            try 
            { 
                var user = await _context.Users.FindAsync(id); 

                if (user == null) 
                { 
                    return StatusCode(404, new { message = "Nincs ilyen felhasználó.", result = "" }); } 
                
                    var scores = await _context.UserHighScores
                        .Where(s => s.UserId == id)
                        .Join(_context.Games, 
                            s => s.GameId, 
                            g => g.Id, 
                            (s, g) => new GameScoreDto 
                            { 
                                GameId = g.Id, 
                                GameName = g.Name, 
                                HighScore = (int)s.HighScore })
                        .ToListAsync();
                
                    var result = new UserDataPublicDto 
                        {   Name = user.UserName, 
                            Scores = scores 
                        }; 
                
                    return StatusCode(200, new { message = "Sikeres lekérdezés (public, ID alapján)", result }); 
            } 
            catch (Exception ex) 
            { 

                return StatusCode(400, new { message = ex.Message, result = "" }); } 

            }

        // GET: api/users/admin  -> összes user (ID + PLAIN jelszó + score-ok)
        [HttpGet("admin")]
        public async Task<ActionResult> GetAllUsersAdmin()
        {
            try
            {
                var users = await _context.Users.ToListAsync();

                var allScores = await _context.UserHighScores
                    .Join(_context.Games,
                          s => s.GameId,
                          g => g.Id,
                          (s, g) => new
                          {
                              s.UserId,
                              GameId = g.Id,
                              GameName = g.Name,
                              HighScore = (int)s.HighScore
                          })
                    .ToListAsync();

                var result = users.Select(u => new UserDataAdminDto
                {
                    Id = u.Id,
                    Name = u.UserName,
                    Password = EncryptProvider.AESDecrypt(u.PasswordHash, _passwordKey),
                    Scores = allScores
                        .Where(x => x.UserId == u.Id)
                        .Select(x => new GameScoreDto
                        {
                            GameId = x.GameId,
                            GameName = x.GameName,
                            HighScore = x.HighScore
                        })
                        .ToList()
                }).ToList();

                return StatusCode(200, new { message = "Sikeres lekérdezés (admin)", result });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // GET: api/users/admin/{id}  -> egy user (ID + PLAIN jelszó + score-ok)
        [HttpGet("admin/{id:guid}")]
        public async Task<ActionResult> GetUserAdminById(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
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
                              HighScore = (int)s.HighScore
                          })
                    .ToListAsync();

                var result = new UserDataAdminDto
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Password = EncryptProvider.AESDecrypt(user.PasswordHash, _passwordKey),
                    Scores = scores
                };

                return StatusCode(200, new { message = "Sikeres lekérdezés (admin, ID alapján)", result });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // POST: api/users/admin  -> új user felvitele (ADMIN), minden score = 0
        [HttpPost("admin")]
        public async Task<ActionResult> CreateUserAdmin(UserCreateAdminDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name) ||
                    string.IsNullOrWhiteSpace(dto.Password))
                {
                    return StatusCode(400, new { message = "Név és jelszó megadása kötelező.", result = "" });
                }

                var cleanName = dto.Name.Trim();

                // név egyediség ellenőrzés
                bool exists = await _context.Users
                    .AnyAsync(u => u.UserName == cleanName);

                if (exists)
                {
                    return StatusCode(409, new { message = "Ez a név már foglalt.", result = "" });
                }

                // új user létrehozása, jelszó AES titkosítással
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = cleanName,
                    PasswordHash = EncryptProvider.AESEncrypt(dto.Password.Trim(), _passwordKey)
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // minden létező játékhoz 0-s score
                var games = await _context.Games.ToListAsync();

                foreach (var game in games)
                {
                    _context.UserHighScores.Add(new UserHighScore
                    {
                        UserId = user.Id,
                        GameId = game.Id,
                        HighScore = 0u
                    });
                }

                if (games.Count > 0)
                {
                    await _context.SaveChangesAsync();
                }

                // visszaadjuk admin szempontból az új user adatait
                var result = new UserDataAdminDto
                {
                    Id = user.Id,
                    Name = user.UserName,
                    // adminnak olvasható jelszó (amit most vittél fel)
                    Password = dto.Password.Trim(),
                    Scores = games.Select(g => new GameScoreDto
                    {
                        GameId = g.Id,
                        GameName = g.Name,
                        HighScore = 0
                    }).ToList()
                };

                return StatusCode(201, new { message = "Új felhasználó sikeresen létrehozva (admin).", result });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }

        // PUT: api/users/admin/{id}  -> név/jelszó + score-ok módosítása (ADMIN)
        [HttpPut("admin/{id:guid}")]
        public async Task<ActionResult> UpdateUserAdmin(Guid id, UserUpdateAdminDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return StatusCode(404, new { message = "Nincs ilyen felhasználó.", result = "" });
                }

                // Név módosítás
                if (!string.IsNullOrWhiteSpace(dto.Name))
                {
                    var newName = dto.Name.Trim();

                    if (newName != user.UserName)
                    {
                        bool nameExists = await _context.Users
                            .AnyAsync(u => u.UserName == newName && u.Id != id);

                        if (nameExists)
                        {
                            return StatusCode(409, new { message = "Ez a felhasználónév már foglalt.", result = "" });
                        }

                        user.UserName = newName;
                    }
                }

                // Jelszó módosítás (encrypt)
                if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    var newPassword = dto.Password.Trim();
                    user.PasswordHash = EncryptProvider.AESEncrypt(newPassword, _passwordKey);
                }

                // SCORE-OK MÓDOSÍTÁSA / TÖRLÉSE – EGY PUT-BAN TÖBBET IS
                if (dto.Scores != null)
                {
                    // meglévő score-ok adott userhez
                    var existingScores = await _context.UserHighScores
                        .Where(s => s.UserId == id)
                        .ToListAsync();

                    var incomingGameIds = dto.Scores
                        .Select(s => s.GameId)
                        .ToHashSet();

                    // 1) törlendők: ami a DB-ben van, de NINCS a bejövő listában
                    var toDelete = existingScores
                        .Where(es => !incomingGameIds.Contains(es.GameId))
                        .ToList();

                    if (toDelete.Count > 0)
                    {
                        _context.UserHighScores.RemoveRange(toDelete);
                    }

                    // 2) upsert: ami jön a listában → vagy létrehoz, vagy frissít
                    foreach (var scoreDto in dto.Scores)
                    {
                        var existing = existingScores
                            .FirstOrDefault(es => es.GameId == scoreDto.GameId);

                        if (existing == null)
                        {
                            var newEntry = new UserHighScore
                            {
                                UserId = id,
                                GameId = scoreDto.GameId,
                                HighScore = (uint)scoreDto.HighScore
                            };
                            await _context.UserHighScores.AddAsync(newEntry);
                        }
                        else
                        {
                            existing.HighScore = (uint)scoreDto.HighScore;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return StatusCode(200, new { message = "Felhasználó és pontszámok sikeresen módosítva (admin)", result = "" });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }



        // DELETE: api/users/admin/{id}  -> user + score-ok törlése (ADMIN)
        [HttpDelete("admin/{id:guid}")]
        public async Task<ActionResult> DeleteUserAdmin(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return StatusCode(404, new { message = "Nincs ilyen felhasználó.", result = "" });
                }

                var scores = await _context.UserHighScores
                    .Where(s => s.UserId == id)
                    .ToListAsync();

                if (scores.Count > 0)
                {
                    _context.UserHighScores.RemoveRange(scores);
                }

                _context.Users.Remove(user);

                await _context.SaveChangesAsync();

                return StatusCode(200, new { message = "Felhasználó és pontszámok sikeresen törölve (admin)", result = "" });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message, result = "" });
            }
        }
    }
}
