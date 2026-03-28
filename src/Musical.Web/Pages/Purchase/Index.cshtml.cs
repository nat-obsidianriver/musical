using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Musical.Web.Pages.Purchase;

public class IndexModel(IHttpClientFactory httpClientFactory) : PageModel
{
    private record CheckoutResponse(string SessionId, string Url);

    public bool IsAuthenticated { get; set; }

    public void OnGet()
    {
        IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = httpClientFactory.CreateClient("MusicalApi");
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var response = await client.PostAsJsonAsync("api/payment/create-checkout-session", new
        {
            successUrl = baseUrl + "/Purchase/Success",
            cancelUrl = baseUrl + "/Purchase/Index"
        });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", "Unable to start checkout. " + error);
            return Page();
        }

        var result = await response.Content.ReadFromJsonAsync<CheckoutResponse>();
        return Redirect(result!.Url);
    }
}
