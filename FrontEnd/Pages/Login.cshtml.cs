using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Para usar Session

namespace Frontend.Pages
{
    public class LoginModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public string CI { get; set; }

        public void OnGet()
        {
            // Limpiar sesión al cargar la página de login
            HttpContext.Session.Clear();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Intentar loguear como Nutricionista
                var nutricionistaResponse = await _httpClient.GetAsync($"api/Nutricionistas/byci/{CI}");
                if (nutricionistaResponse.IsSuccessStatusCode)
                {
                    var nutricionista = await nutricionistaResponse.Content.ReadFromJsonAsync<Nutricionista>();
                    HttpContext.Session.SetString("UserRole", "Nutricionista");
                    HttpContext.Session.SetInt32("UserId", nutricionista.IdNutricionista);
                    HttpContext.Session.SetString("UserName", $"{nutricionista.Persona?.Nombre} {nutricionista.Persona?.ApellidoPaterno}");
                    return RedirectToPage("/NutricionistaDashboard");
                }

                // Intentar loguear como Paciente
                var pacienteResponse = await _httpClient.GetAsync($"api/Pacientes/byci/{CI}");
                if (pacienteResponse.IsSuccessStatusCode)
                {
                    var paciente = await pacienteResponse.Content.ReadFromJsonAsync<Paciente>();
                    HttpContext.Session.SetString("UserRole", "Paciente");
                    HttpContext.Session.SetInt32("UserId", paciente.IdPaciente);
                    HttpContext.Session.SetString("UserName", $"{paciente.Persona?.Nombre} {paciente.Persona?.ApellidoPaterno}");
                    return RedirectToPage("/PacienteDashboard");
                }

                ModelState.AddModelError(string.Empty, "CI no encontrado o credenciales inválidas.");
                return Page();
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API. Por favor, verifica que el backend esté en ejecución.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado: {ex.Message}");
                return Page();
            }
        }
    }
}