using Arcade_mania_backend_webAPI.Models;
using Arcade_mania_backend_webAPI.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arcade_mania_backend_webAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{

    private readonly GameScoresDbContext _context;


    public UsersController(GameScoresDbContext context)
    {
        _context = context;
    }


    // GET: api/Users  -> összes user (jelszó nélkül)
    [HttpGet]
    public async Task<ActionResult<UserGetDto>> GetUsers()
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

        return Ok(users);
    }


    // GET: api/Users/debug  -> összes user, JELSZÓVAL (csak teszt!)
    [HttpGet("debug")]
    public async Task<ActionResult<UserGetDto_allData>> GetUsersWithPassword()
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

        return Ok(users);
    }


    // GET: api/Users/5  -> egy user ID alapján (jelszó nélkül)
    [HttpGet("{id}")]
    public async Task<ActionResult<UserGetByIdDto>> GetUser(uint id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        var dto = new UserGetByIdDto
        {
            Id = user.Id,
            Name = user.Name,
            FighterHighScore = user.FighterHighScore,
            MemoryHighScore = user.MemoryHighScore,
            SnakeHighScore = user.SnakeHighScore
        };

        return Ok(dto);
    }


    [HttpGet("{id}/debug")]
    public async Task<ActionResult<UserGetByIdDto_allData>> GetUserWithPassword(uint id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        var dto = new UserGetByIdDto_allData
        {
            Id = user.Id,
            Name = user.Name,
            Password = user.Password,
            FighterHighScore = user.FighterHighScore,
            MemoryHighScore = user.MemoryHighScore,
            SnakeHighScore = user.SnakeHighScore
        };

        return Ok(dto);
    }


    // POST: api/Users  -> új user felvitele, NÉV EGYEDISÉG ELLENŐRZÉSSEL
    [HttpPost]
    public async Task<ActionResult<UserGetByIdDto>> CreateUser(UserCreateDto dto)
    {
        // Név egyediség ellenőrzése
        var nameExists = await _context.Users
            .AnyAsync(u => u.Name == dto.Name);

        if (nameExists)
        {
            return Conflict(new
            {
                message = "Már létezik ilyen nevű felhasználó."
            });
        }

        var user = new User
        {
            Name = dto.Name,
            Password = dto.Password, // később hash
            FighterHighScore = dto.FighterHighScore,
            MemoryHighScore = dto.MemoryHighScore,
            SnakeHighScore = dto.SnakeHighScore
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var resultDto = new UserGetByIdDto
        {
            Id = user.Id,
            Name = user.Name,
            FighterHighScore = user.FighterHighScore,
            MemoryHighScore = user.MemoryHighScore,
            SnakeHighScore = user.SnakeHighScore
        };

        // 201 Created + Location header: api/Users/{id}
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, resultDto);
    }


    // POST: api/Users/{id}/fighter-score
    [HttpPost("{id}/fighter-score")]
    public async Task<IActionResult> UpdateFighterScore(int id, FighterScoreUpdateDto dto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.FighterHighScore = dto.FighterHighScore;

        await _context.SaveChangesAsync();
        return NoContent();
    }


    // POST: api/Users/{id}/memory-score
    [HttpPost("{id}/memory-score")]
    public async Task<IActionResult> UpdateMemoryScore(int id, MemoryScoreUpdateDto dto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.MemoryHighScore = dto.MemoryHighScore;

        await _context.SaveChangesAsync();
        return NoContent();
    }



    // POST: api/Users/{id}/snake-score
    [HttpPost("{id}/snake-score")]
    public async Task<IActionResult> UpdateSnakeScore(int id, SnakeScoreUpdateDto dto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.SnakeHighScore = dto.SnakeHighScore;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
