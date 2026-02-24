namespace Musical.Api.Dtos;

public record RegisterRequest(string DisplayName, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    string UserId,
    string DisplayName,
    string Email,
    string Role,
    DateTime Expiry);

public record UserProfileDto(
    string UserId,
    string DisplayName,
    string Email,
    string? Bio,
    string? HeadshotFileName);

public class UpdateProfileForm
{
    public string? Bio { get; set; }
    public IFormFile? Headshot { get; set; }
}
