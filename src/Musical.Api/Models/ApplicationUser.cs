using Microsoft.AspNetCore.Identity;

namespace Musical.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
