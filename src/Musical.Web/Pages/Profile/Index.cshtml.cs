using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Musical.Web.Models;

namespace Musical.Web.Pages.Profile;

[Authorize]
public class IndexModel(IHttpClientFactory httpClientFactory, IConfiguration config) : PageModel
{
    public UserProfileViewModel Profile { get; private set; } = null!;
    public string ApiBase { get; private set; } = string.Empty;

    [BindProperty]
    public string? Bio { get; set; }

    [BindProperty]
    public IFormFile? Headshot { get; set; }

    public string? StatusMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        ApiBase = config["ApiBaseUrl"] ?? "https://localhost:7200";
        var client = httpClientFactory.CreateClient("MusicalApi");
        var dto = await client.GetFromJsonAsync<UserProfileViewModel>("api/auth/profile");
        if (dto is null) return RedirectToPage("/Auth/Login");

        Profile = dto;
        Bio = dto.Bio;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ApiBase = config["ApiBaseUrl"] ?? "https://localhost:7200";
        var client = httpClientFactory.CreateClient("MusicalApi");

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(Bio ?? ""), "bio");

        if (Headshot is { Length: > 0 })
        {
            var stream = Headshot.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(Headshot.ContentType ?? "image/jpeg");
            form.Add(fileContent, "headshot", Headshot.FileName);
        }

        var response = await client.PutAsync("api/auth/profile", form);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Failed to update profile.");
            var dto = await client.GetFromJsonAsync<UserProfileViewModel>("api/auth/profile");
            Profile = dto ?? new UserProfileViewModel("", "", "", null, null);
            return Page();
        }

        Profile = (await response.Content.ReadFromJsonAsync<UserProfileViewModel>())!;
        StatusMessage = "Profile updated successfully.";
        Bio = Profile.Bio;
        return Page();
    }
}

public record UserProfileViewModel(
    string UserId,
    string DisplayName,
    string Email,
    string? Bio,
    string? HeadshotFileName);
