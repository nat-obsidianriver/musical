using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Musical.Web.Models;

namespace Musical.Web.Pages.Auth;

public class RegisterModel(IHttpClientFactory httpClientFactory) : PageModel
{
    [BindProperty] public string DisplayName     { get; set; } = string.Empty;
    [BindProperty] public string Email           { get; set; } = string.Empty;
    [BindProperty] public string Password        { get; set; } = string.Empty;
    [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return Page();
        }

        var client = httpClientFactory.CreateClient("MusicalApi");
        var res = await client.PostAsJsonAsync("api/auth/register",
            new { displayName = DisplayName, email = Email, password = Password });

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            ErrorMessage = "Registration failed. " + body;
            return Page();
        }

        var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is null) { ErrorMessage = "Unexpected error."; return Page(); }

        HttpContext.Session.SetString("jwt", auth.Token);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, auth.UserId),
            new Claim(ClaimTypes.Name,  auth.DisplayName),
            new Claim(ClaimTypes.Email, auth.Email),
            new Claim(ClaimTypes.Role,  auth.Role)
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToPage("/Index");
    }
}
