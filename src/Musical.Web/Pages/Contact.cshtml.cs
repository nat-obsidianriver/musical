using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Musical.Web.Pages;

public class ContactModel : PageModel
{
    [BindProperty, Required, MaxLength(100)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [BindProperty, Required, EmailAddress, MaxLength(200)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [BindProperty, Required, MaxLength(2000)]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    public bool Submitted { get; set; }

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        // Placeholder: wire up email sending here in a future update
        Submitted = true;
        return Page();
    }
}
