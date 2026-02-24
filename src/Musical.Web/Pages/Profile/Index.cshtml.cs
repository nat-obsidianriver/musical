using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        var response = await client.GetAsync("api/auth/profile");
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return RedirectToPage("/Auth/Login");

        response.EnsureSuccessStatusCode();
        Profile = (await response.Content.ReadFromJsonAsync<UserProfileViewModel>())!;
        Bio = Profile.Bio;
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
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return RedirectToPage("/Auth/Login");

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Failed to update profile.");
            // Re-fetch profile for display
            var getResp = await client.GetAsync("api/auth/profile");
            if (getResp.IsSuccessStatusCode)
                Profile = (await getResp.Content.ReadFromJsonAsync<UserProfileViewModel>())!;
            else
                Profile = new UserProfileViewModel("", User.Identity?.Name ?? "", "", null, null);
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
