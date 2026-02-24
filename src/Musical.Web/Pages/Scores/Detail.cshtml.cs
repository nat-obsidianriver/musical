using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Musical.Web.Models;

namespace Musical.Web.Pages.Scores;

public class DetailModel(IHttpClientFactory httpClientFactory, IConfiguration config) : PageModel
{
    public ScoreSummary? Score { get; private set; }
    public List<AnnotationViewModel> Annotations { get; private set; } = [];
    public string ApiBase { get; private set; } = string.Empty;
    public string? JwtToken => HttpContext.Session.GetString("jwt");
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ApiBase = config["ApiBaseUrl"] ?? "https://localhost:7200";
        var client = httpClientFactory.CreateClient("MusicalApi");

        try
        {
            Score = await client.GetFromJsonAsync<ScoreSummary>($"api/scores/{id}");
            if (Score is null)
                return Page();

            Annotations = await client.GetFromJsonAsync<List<AnnotationViewModel>>(
                $"api/scores/{id}/annotations") ?? [];
        }
        catch
        {
            ErrorMessage = "Could not load score data. Is the API running?";
        }

        return Page();
    }
}
