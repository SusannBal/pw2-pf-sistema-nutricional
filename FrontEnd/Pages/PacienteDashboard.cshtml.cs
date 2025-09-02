using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http; // Para usar Session
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Frontend.Pages
{
    public class PacienteDashboardModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public PacienteDashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public string UserName { get; set; }
        public int UserId { get; set; }
        public int PersonaId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar si el usuario está logueado y es paciente
            if (HttpContext.Session.GetString("UserRole") != "Paciente")
            {
                return RedirectToPage("/Login"); // Redirigir si no es paciente
            }

            UserName = HttpContext.Session.GetString("UserName");
            UserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Obtener el IdPersona del paciente para el enlace de edición de datos personales
            try
            {
                var response = await _httpClient.GetAsync($"api/Pacientes/{UserId}");
                if (response.IsSuccessStatusCode)
                {
                    var paciente = await response.Content.ReadFromJsonAsync<Paciente>();
                    PersonaId = paciente.IdPersona;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No se pudo cargar la información del paciente.");
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API para cargar la información del paciente.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar la información del paciente: {ex.Message}");
            }

            return Page();
        }
    }
}