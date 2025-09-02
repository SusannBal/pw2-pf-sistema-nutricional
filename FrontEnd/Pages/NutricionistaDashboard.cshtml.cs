using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http; // Para usar Session

namespace Frontend.Pages
{
    public class NutricionistaDashboardModel : PageModel
    {
        public string UserName { get; set; }
        public int UserId { get; set; }

        public IActionResult OnGet()
        {
            // Verificar si el usuario está logueado y es nutricionista
            if (HttpContext.Session.GetString("UserRole") != "Nutricionista")
            {
                return RedirectToPage("/Login"); // Redirigir si no es nutricionista
            }

            UserName = HttpContext.Session.GetString("UserName");
            UserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            return Page();
        }
    }
}