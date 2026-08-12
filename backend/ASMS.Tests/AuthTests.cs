using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ASMS.API.Controllers;
using ASMS.API.DTOs;
using ASMS.API.Services;

namespace ASMS.Tests;

public class AuthTests
{
    private static JwtService CreateJwtService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-jwt-key-change-in-production-32chars!",
                ["Jwt:Issuer"] = "ASMS",
                ["Jwt:Audience"] = "ASMS",
                ["Jwt:ExpiryHours"] = "24"
            }).Build();
        return new JwtService(config);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Login_ValidCredentials_ReturnsToken));
        var controller = new AuthController(db, CreateJwtService());

        var result = await controller.Login(new LoginRequest("admin@test.com", "Admin@123"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.NotEmpty(response.Token);
        Assert.Equal("Admin", response.Role);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Login_InvalidPassword_ReturnsUnauthorized));
        var controller = new AuthController(db, CreateJwtService());

        var result = await controller.Login(new LoginRequest("admin@test.com", "WrongPassword"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_NonExistentEmail_ReturnsUnauthorized()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Login_NonExistentEmail_ReturnsUnauthorized));
        var controller = new AuthController(db, CreateJwtService());

        var result = await controller.Login(new LoginRequest("nobody@test.com", "Admin@123"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_InactiveUser_ReturnsUnauthorized()
    {
        var db = TestHelper.CreateDbWithSeed(nameof(Login_InactiveUser_ReturnsUnauthorized));
        var user = await db.Users.FindAsync(1);
        user!.IsActive = false;
        await db.SaveChangesAsync();

        var controller = new AuthController(db, CreateJwtService());
        var result = await controller.Login(new LoginRequest("admin@test.com", "Admin@123"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
