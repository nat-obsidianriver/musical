using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Musical.Web.Models;

namespace Musical.Web.Pages.Folders;

[Authorize]
public class IndexModel(IHttpClientFactory httpClientFactory) : PageModel
{
    public List<FolderSummary> Folders { get; private set; } = [];
    public string? ErrorMessage { get; private set; }
    public string? StatusMessage { get; private set; }

    // Create/Edit bound properties
    [BindProperty] public string FolderName { get; set; } = string.Empty;
    [BindProperty] public string? FolderDescription { get; set; }
    [BindProperty] public string FolderColor { get; set; } = "#87CEEB";
    [BindProperty] public bool FolderIsMasked { get; set; }

    public async Task OnGetAsync()
    {
        await LoadFoldersAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(FolderName))
        {
            ErrorMessage = "Folder name is required.";
            await LoadFoldersAsync();
            return Page();
        }

        var client = httpClientFactory.CreateClient("MusicalApi");
        var payload = new { name = FolderName.Trim(), description = FolderDescription?.Trim(), color = FolderColor };
        var response = await client.PostAsJsonAsync("api/folders", payload);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return RedirectToPage("/Auth/Login");

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "Failed to create folder.";
            await LoadFoldersAsync();
            return Page();
        }

        StatusMessage = $"Folder \"{FolderName.Trim()}\" created.";
        await LoadFoldersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id)
    {
        if (string.IsNullOrWhiteSpace(FolderName))
        {
            ErrorMessage = "Folder name is required.";
            await LoadFoldersAsync();
            return Page();
        }

        var client = httpClientFactory.CreateClient("MusicalApi");
        var payload = new { name = FolderName.Trim(), description = FolderDescription?.Trim(), color = FolderColor, isMasked = FolderIsMasked };
        var response = await client.PutAsJsonAsync($"api/folders/{id}", payload);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return RedirectToPage("/Auth/Login");

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "Failed to update folder.";
            await LoadFoldersAsync();
            return Page();
        }

        StatusMessage = "Folder updated.";
        await LoadFoldersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var client = httpClientFactory.CreateClient("MusicalApi");
        var response = await client.DeleteAsync($"api/folders/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return RedirectToPage("/Auth/Login");

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "Failed to delete folder.";
            await LoadFoldersAsync();
            return Page();
        }

        StatusMessage = "Folder deleted.";
        await LoadFoldersAsync();
        return Page();
    }

    private async Task LoadFoldersAsync()
    {
        var client = httpClientFactory.CreateClient("MusicalApi");
        var response = await client.GetAsync("api/folders");
        if (response.IsSuccessStatusCode)
            Folders = await response.Content.ReadFromJsonAsync<List<FolderSummary>>() ?? [];
    }
}
