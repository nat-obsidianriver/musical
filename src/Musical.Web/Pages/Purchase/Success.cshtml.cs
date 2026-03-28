using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Musical.Web.Pages.Purchase;

public class SuccessModel(IHttpClientFactory httpClientFactory) : PageModel
{
    private record VerifyResponse(bool Paid, string Email, string SessionId);

    public bool Verified { get; set; }
    public string? Email { get; set; }
    public string? SessionId { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        SessionId = Request.Query["session_id"].ToString();
        if (string.IsNullOrEmpty(SessionId))
        {
            ErrorMessage = "No payment session found.";
            return Page();
        }

        var client = httpClientFactory.CreateClient("MusicalApi");
        try
        {
            var response = await client.GetAsync("api/payment/verify-session/" + SessionId);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<VerifyResponse>();
                Verified = result?.Paid ?? false;
                Email = result?.Email;
            }
            else
            {
                ErrorMessage = "Payment verification failed. " + await response.Content.ReadAsStringAsync();
            }
        }
        catch
        {
            ErrorMessage = "Unable to verify payment.";
        }

        if (Verified)
        {
            return RedirectToPage("/Auth/Register", new { session_id = SessionId, email = Email });
        }

        return Page();
    }
}
