
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillSync.Models;
using SkillSync.DTOs;

namespace SkillSync.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
        
    {
        _userManager = userManager;
        _signInManager = signInManager;
        
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return Unauthorized("Invalid email or password.");

        var result = await _signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!result.Succeeded)
            return Unauthorized("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            Message = "Login successful",
            UserId = user.Id,
            Email = user.Email,
            Roles = roles
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return Ok(new
        {
            Message = "Logout successful"
        });
    }
}
