using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASMS.API.Data;
using ASMS.API.DTOs;
using ASMS.API.Services;

namespace ASMS.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, JwtService jwt) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email && u.IsActive);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials" });

        var token = jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Role, user.Name, user.Id));
    }
}
