using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Musical.Web.Models;

namespace Musical.Web.Pages.Scores;

[Authorize]
public class UploadModel(IHttpClientFactory httpClientFactory) : PageModel
{
    [BindProperty, Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [BindProperty, MaxLength(200)]
    public string? Composer { get; set; }

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public int? FolderId { get; set; }

    [BindProperty, Display(Name = "Sheet Music Image")]
    public IFormFile? Image { get; set; }

    public List<SelectListItem> FolderOptions { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadFoldersAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || Image is null)
        {
            ErrorMessage = "Please fill in all required fields and select an image.";
            await LoadFoldersAsync();
            return Page();
        }

        var client = httpClientFactory.CreateClient("MusicalApi");

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(Title), "Title");
        if (!string.IsNullOrWhiteSpace(Composer))
            form.Add(new StringContent(Composer), "Composer");
        if (!string.IsNullOrWhiteSpace(Description))
            form.Add(new StringContent(Description), "Description");
        if (FolderId.HasValue)
            form.Add(new StringContent(FolderId.Value.ToString()), "FolderId");

        await using var stream = Image.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(Image.ContentType);
        form.Add(fileContent, "image", Image.FileName);

        var response = await client.PostAsync("api/scores", form);

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Upload failed: {await response.Content.ReadAsStringAsync()}";
            await LoadFoldersAsync();
            return Page();
        }

        return RedirectToPage("/Index");
    }

    private async Task LoadFoldersAsync()
    {
        var client = httpClientFactory.CreateClient("MusicalApi");
        var folders = await client.GetFromJsonAsync<List<FolderSummary>>("api/folders") ?? [];
        FolderOptions = folders.Select(f => new SelectListItem(f.Name, f.Id.ToString())).ToList();
    }
}
