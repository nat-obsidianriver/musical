using Microsoft.AspNetCore.Identity;

namespace Musical.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? HeadshotFileName { get; set; }
}
