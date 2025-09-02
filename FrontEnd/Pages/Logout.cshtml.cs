using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http; // Para usar Session

namespace Frontend.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            HttpContext.Session.Clear(); // Limpiar toda la sesi�n
            return RedirectToPage("/Login"); // Redirigir a la p�gina de login
        }
    }
}

