using Microsoft.AspNetCore.Mvc.RazorPages;
using Musical.Web.Models;

namespace Musical.Web.Pages;

public class IndexModel(IHttpClientFactory httpClientFactory, IConfiguration config) : PageModel
{
    public List<ScoreSummary> Scores { get; private set; } = [];
    public string ApiBase { get; private set; } = string.Empty;

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
    }
}
