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
