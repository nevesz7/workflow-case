using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Workflow.Api.DTOs;
using Workflow.Api.Models;

namespace Workflow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {
        using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        
        // 1. Validate User from DB
        var sql = "SELECT * FROM Users WHERE Username = @Username";
        var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { login.Username });

        if (user == null || user.PasswordHash != login.Password)
        {
            return Unauthorized("Invalid username or password.");
        }

        // 2. Generate JWT Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "super_secret_key_for_dev_1234567890");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim("name", user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(4),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // 3. Return Token
        return Ok(new LoginResponse(tokenString, user.Username, user.Role.ToString(), user.Id));
    }
}
