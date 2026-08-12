using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASMS.API.Data;
using ASMS.API.DTOs;
using ASMS.API.Models;

namespace ASMS.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Users.Select(u => new UserResponse(u.Id, u.Name, u.Email, u.Role, u.IsActive, u.CreatedAt)).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var u = await db.Users.FindAsync(id);
        return u == null ? NotFound() : Ok(new UserResponse(u.Id, u.Name, u.Email, u.Role, u.IsActive, u.CreatedAt));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest(new { message = "Email already exists" });

        var user = new User
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = req.Role
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserResponse(user.Id, user.Name, user.Email, user.Role, user.IsActive, user.CreatedAt));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest req)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Name = req.Name;
        user.Email = req.Email;
        user.IsActive = req.IsActive;
        await db.SaveChangesAsync();
        return Ok(new UserResponse(user.Id, user.Name, user.Email, user.Role, user.IsActive, user.CreatedAt));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
