using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Musical.Web.Pages.Auth;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        HttpContext.Session.Remove("jwt");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Index");
    }
}
