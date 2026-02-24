using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Musical.Api.Data;
using Musical.Api.Dtos;
using Musical.Api.Models;
using Musical.Core.Models;

namespace Musical.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    MusicalDbContext db,
    IConfiguration config,
    IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return BadRequest("Display name is required.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName.Trim()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await userManager.AddToRoleAsync(user, "User");

        // Create default folder named after the user
        var folder = new Folder
        {
            Name = user.DisplayName,
            UserId = user.Id,
            UserDisplayName = user.DisplayName,
            Color = "#87CEEB"
        };
        db.Folders.Add(folder);
        await db.SaveChangesAsync();

        return Ok(BuildAuthResponse(user, "User"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Invalid email or password.");

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.Contains("Admin") ? "Admin" : "User";

        return Ok(BuildAuthResponse(user, role));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> Me()
    {
        var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (user is null) return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.Contains("Admin") ? "Admin" : "User";

        return Ok(BuildAuthResponse(user, role));
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (user is null) return Unauthorized();

        return Ok(new UserProfileDto(user.Id, user.DisplayName, user.Email ?? "", user.Bio, user.HeadshotFileName));
    }

    [HttpPut("profile")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromForm] UpdateProfileForm form)
    {
        var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (user is null) return Unauthorized();

        user.Bio = form.Bio?.Trim();

        if (form.Headshot is { Length: > 0 })
        {
            var ext = Path.GetExtension(form.Headshot.FileName).ToLowerInvariant();
            string[] allowed = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
            if (!allowed.Contains(ext))
                return BadRequest("Headshot must be an image (jpg, png, gif, webp).");

            var uploadsDir = Path.Combine(env.ContentRootPath, "uploads");
            Directory.CreateDirectory(uploadsDir);

            // Delete old headshot if present
            if (user.HeadshotFileName is not null)
            {
                var old = Path.Combine(uploadsDir, user.HeadshotFileName);
                if (System.IO.File.Exists(old)) System.IO.File.Delete(old);
            }

            user.HeadshotFileName = $"headshot_{user.Id}{ext}";
            var path = Path.Combine(uploadsDir, user.HeadshotFileName);
            await using var stream = System.IO.File.Create(path);
            await form.Headshot.CopyToAsync(stream);
        }

        await userManager.UpdateAsync(user);
        return Ok(new UserProfileDto(user.Id, user.DisplayName, user.Email ?? "", user.Bio, user.HeadshotFileName));
    }

    [HttpGet("/api/users/{userId}/headshot")]
    public async Task<IActionResult> GetHeadshot(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user?.HeadshotFileName is null) return NotFound();

        var path = Path.Combine(env.ContentRootPath, "uploads", user.HeadshotFileName);
        if (!System.IO.File.Exists(path)) return NotFound();

        var ext = Path.GetExtension(user.HeadshotFileName).TrimStart('.').ToLowerInvariant();
        var contentType = ext switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png"  => "image/png",
            "gif"  => "image/gif",
            "webp" => "image/webp",
            _      => "application/octet-stream"
        };

        return PhysicalFile(path, contentType);
    }

    private AuthResponse BuildAuthResponse(ApplicationUser user, string role)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddHours(double.Parse(config["Jwt:ExpiryHours"] ?? "24"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name,  user.DisplayName),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role,  role)
        };

        var token = new JwtSecurityToken(
            issuer:   config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims:   claims,
            expires:  expiry,
            signingCredentials: creds);

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Id,
            user.DisplayName,
            user.Email ?? "",
            role,
            expiry);
    }
}
