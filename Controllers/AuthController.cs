using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillSync.Models;
using SkillSync.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SkillSync.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return Unauthorized("Invalid email or password.");

        var passwordValid = await _userManager.CheckPasswordAsync(
            user,
            request.Password);

        if (!passwordValid)
            return Unauthorized("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("SkillSyncSuperSecretKey123456789"));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return Ok(new
        {
            Message = "Login successful",
            UserId = user.Id,
            Email = user.Email,
            Roles = roles,
            Token = tokenString
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new
        {
            Message = "Logout successful"
        });
    }
}
