using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Musical.Web.Models;

namespace Musical.Web.Pages.Scores;

public class DetailModel(IHttpClientFactory httpClientFactory, IConfiguration config) : PageModel
{
    public ScoreSummary? Score { get; private set; }
    public List<AnnotationViewModel> Annotations { get; private set; } = [];
    public List<AnnotationUserGroup> AnnotationGroups { get; private set; } = [];
    public List<FolderSummary> UserFolders { get; private set; } = [];
    public string ApiBase { get; private set; } = string.Empty;
    public string? JwtToken => HttpContext.Session.GetString("jwt");
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ApiBase = config["ApiBaseUrl"] ?? "https://localhost:7200";
        var client = httpClientFactory.CreateClient("MusicalApi");

        try
        {
            Score = await client.GetFromJsonAsync<ScoreSummary>($"api/scores/{id}");
            if (Score is null) return Page();

            Annotations = await client.GetFromJsonAsync<List<AnnotationViewModel>>(
                $"api/scores/{id}/annotations") ?? [];

            // Fetch the authenticated user's folders for the annotation form
            if (User.Identity?.IsAuthenticated == true)
            {
                var foldersResp = await client.GetAsync("api/folders");
                if (foldersResp.IsSuccessStatusCode)
                    UserFolders = await foldersResp.Content.ReadFromJsonAsync<List<FolderSummary>>() ?? [];
            }
        }
        catch
        {
            ErrorMessage = "Could not load score data. Is the API running?";
        }

        // Build user → folder → annotation hierarchy
        AnnotationGroups = Annotations
            .GroupBy(a => new { UserKey = a.UserId ?? $"anon:{a.AuthorName}", a.AuthorName })
            .Select(g => new AnnotationUserGroup(
                g.Key.AuthorName,
                g.First().UserId,
                g.GroupBy(a => new { a.FolderId, a.FolderName, a.FolderColor })
                 .Select(fg => new AnnotationFolderGroup(
                     fg.Key.FolderId,
                     fg.Key.FolderName ?? "Uncategorized",
                     fg.Key.FolderColor ?? "#aaaaaa",
                     fg.OrderBy(a => a.CreatedAt).ToList()))
                 .ToList()))
            .OrderBy(g => g.AuthorName)
            .ToList();

        return Page();
    }
}
