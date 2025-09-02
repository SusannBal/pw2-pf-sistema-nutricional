using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Frontend.Pages.Consultas
{
    public class NutricionistaConsultasModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Consulta> Consultas { get; set; } = new();

        public NutricionistaConsultasModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar si el usuario está logueado y es nutricionista
            if (HttpContext.Session.GetString("UserRole") != "Nutricionista")
            {
                return RedirectToPage("/Login");
            }

            var nutricionistaId = HttpContext.Session.GetInt32("UserId");
            if (nutricionistaId == null)
            {
                ModelState.AddModelError(string.Empty, "ID de nutricionista no encontrado en la sesión.");
                return Page();
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/Consultas/byNutricionista/{nutricionistaId}");
                if (response.IsSuccessStatusCode)
                {
                    Consultas = await response.Content.ReadFromJsonAsync<List<Consulta>>() ?? new List<Consulta>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Consultas = new List<Consulta>(); // No hay consultas, no es un error
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar las consultas: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar las consultas: {ex.Message}");
            }

            return Page();
        }
    }
}