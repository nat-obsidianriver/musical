using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Musical.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class TutorialVideoModel(IHttpClientFactory httpClientFactory) : PageModel
{
    public string? CurrentSource { get; set; }
    public string? CurrentYouTubeUrl { get; set; }
    public string? CurrentFileName { get; set; }
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadCurrentSettings();
    }

    public async Task<IActionResult> OnPostYouTubeUrlAsync(string youtubeUrl)
    {
        var client = httpClientFactory.CreateClient("MusicalApi");

        await PutSetting(client, "tutorial-video-url", youtubeUrl);
        await PutSetting(client, "tutorial-video-source", "youtube");

        StatusMessage = "YouTube URL saved successfully.";
        await LoadCurrentSettings();
        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync(IFormFile videoFile)
    {
        if (videoFile is null || videoFile.Length == 0)
        {
            StatusMessage = "Please select a video file.";
            await LoadCurrentSettings();
            return Page();
        }

        var client = httpClientFactory.CreateClient("MusicalApi");

        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(videoFile.OpenReadStream());
        content.Add(streamContent, "file", videoFile.FileName);

        var response = await client.PostAsync("api/site-settings/tutorial-video/upload", content);

        StatusMessage = response.IsSuccessStatusCode
            ? "Video uploaded successfully."
            : "Failed to upload video. Please try again.";

        await LoadCurrentSettings();
        return Page();
    }

    private async Task LoadCurrentSettings()
    {
        var client = httpClientFactory.CreateClient("MusicalApi");
        CurrentSource = await GetSetting(client, "tutorial-video-source");
        CurrentYouTubeUrl = await GetSetting(client, "tutorial-video-url");
        CurrentFileName = await GetSetting(client, "tutorial-video-file");
    }

    private static async Task<string?> GetSetting(HttpClient client, string key)
    {
        var response = await client.GetAsync($"api/site-settings/{key}");
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("value").GetString();
    }

    private static async Task PutSetting(HttpClient client, string key, string value)
    {
        var json = JsonSerializer.Serialize(new { value });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await client.PutAsync($"api/site-settings/{key}", content);
    }
}
