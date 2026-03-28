using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Musical.Web.Pages.GettingStarted;

public class IndexModel(IHttpClientFactory httpClientFactory, IConfiguration config) : PageModel
{
    public string? VideoEmbedUrl { get; set; }
    public string? VideoFileUrl { get; set; }

    public async Task OnGetAsync()
    {
        var client = httpClientFactory.CreateClient("MusicalApi");

        var source = await GetSetting(client, "tutorial-video-source");

        if (source == "youtube")
        {
            var url = await GetSetting(client, "tutorial-video-url");
            if (!string.IsNullOrEmpty(url))
                VideoEmbedUrl = ConvertToEmbedUrl(url);
        }
        else if (source == "file")
        {
            var apiBase = config["ApiBaseUrl"]?.TrimEnd('/') ?? "https://localhost:7136";
            VideoFileUrl = $"{apiBase}/api/site-settings/tutorial-video/file";
        }
    }

    private static async Task<string?> GetSetting(HttpClient client, string key)
    {
        var response = await client.GetAsync($"api/site-settings/{key}");
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("value").GetString();
    }

    private static string? ConvertToEmbedUrl(string url)
    {
        // Handle youtube.com/watch?v=ID
        if (url.Contains("youtube.com/watch"))
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var videoId = query["v"];
            if (!string.IsNullOrEmpty(videoId))
                return $"https://www.youtube.com/embed/{videoId}";
        }

        // Handle youtu.be/ID
        if (url.Contains("youtu.be/"))
        {
            var videoId = url.Split("youtu.be/")[1].Split('?')[0];
            if (!string.IsNullOrEmpty(videoId))
                return $"https://www.youtube.com/embed/{videoId}";
        }

        // Handle already-embed URLs
        if (url.Contains("youtube.com/embed/"))
            return url;

        return null;
    }
}
