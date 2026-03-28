using Microsoft.AspNetCore.Mvc.RazorPages;
using Musical.Web.Models;

namespace Musical.Web.Pages;

public class AboutModel(IHttpClientFactory httpClientFactory, IConfiguration config) : PageModel
{
    public Dictionary<string, SiteContentDto> SiteContent { get; private set; } = new();
    public bool IsAdmin => User.IsInRole("Admin");
    public string? JwtToken => HttpContext.Session.GetString("jwt");
    public string ApiBase => config["ApiBaseUrl"] ?? "http://localhost:5241";

    public async Task OnGetAsync()
    {
        var client = httpClientFactory.CreateClient("MusicalApi");
        try
        {
            var items = await client.GetFromJsonAsync<List<SiteContentDto>>("api/site-content");
            SiteContent = (items ?? []).ToDictionary(c => c.Slug);
        }
        catch
        {
            SiteContent = new();
        }
    }
}
