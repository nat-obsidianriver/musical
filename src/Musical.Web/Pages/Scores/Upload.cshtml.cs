using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace Musical.Web.Pages.Scores;

public class UploadModel(IHttpClientFactory httpClientFactory) : PageModel
{
    [BindProperty]
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    [MaxLength(200)]
    public string? Composer { get; set; }

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    [Display(Name = "Sheet Music Image")]
    public IFormFile? Image { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || Image is null)
        {
            ErrorMessage = "Please fill in all required fields and select an image.";
            return Page();
        }

        var client = httpClientFactory.CreateClient("MusicalApi");

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(Title), "Title");
        if (!string.IsNullOrWhiteSpace(Composer))
            form.Add(new StringContent(Composer), "Composer");
        if (!string.IsNullOrWhiteSpace(Description))
            form.Add(new StringContent(Description), "Description");

        await using var stream = Image.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(Image.ContentType);
        form.Add(fileContent, "image", Image.FileName);

        var response = await client.PostAsync("api/scores", form);

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Upload failed: {await response.Content.ReadAsStringAsync()}";
            return Page();
        }

        return RedirectToPage("/Index");
    }
}
