using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Musical.Web.Models;

namespace Musical.Web.Pages.Auth;

public class LoginModel(IHttpClientFactory httpClientFactory) : PageModel
{
    [BindProperty] public string Email    { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? ReturnUrl    { get; set; }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email and password are required.";
            return Page();
        }

        var client = httpClientFactory.CreateClient("MusicalApi");
        var res = await client.PostAsJsonAsync("api/auth/login",
            new { email = Email, password = Password });

        if (!res.IsSuccessStatusCode)
        {
            ErrorMessage = "Invalid email or password.";
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

        return LocalRedirect(returnUrl ?? "/");
    }
}
