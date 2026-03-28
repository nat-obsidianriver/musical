using Microsoft.AspNetCore.Mvc.RazorPages;
using Musical.Web.Models;

namespace Musical.Web.Pages;

public class IndexModel(IHttpClientFactory httpClientFactory, IConfiguration config) : PageModel
{
    public List<ScoreSummary> Scores { get; private set; } = [];
    public string ApiBase { get; private set; } = string.Empty;
    public Dictionary<string, SiteContentDto> SiteContent { get; private set; } = new();
    public bool IsAdmin => User.IsInRole("Admin");
    public string? JwtToken => HttpContext.Session.GetString("jwt");

    public async Task OnGetAsync()
    {
        ApiBase = config["ApiBaseUrl"] ?? "https://localhost:7200";
        var client = httpClientFactory.CreateClient("MusicalApi");
        try
        {
            Scores = await client.GetFromJsonAsync<List<ScoreSummary>>("api/scores") ?? [];
        }
        catch
        {
            // API unavailable — show empty state
            Scores = [];
        }

        try
        {
            var contentItems = await client.GetFromJsonAsync<List<SiteContentDto>>("api/site-content") ?? [];
            foreach (var item in contentItems)
            {
                SiteContent[item.Slug] = item;
            }
        }
        catch
        {
            // Site content unavailable — use defaults
        }
    }
}
