using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frontend.Pages.Nutricionistas
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Nutricionista> Nutricionistas { get; set; } = new();
        public List<Persona> Personas { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Cargar nutricionistas
                var response = await _httpClient.GetAsync("api/Nutricionistas");
                if (response.IsSuccessStatusCode)
                {
                    Nutricionistas = await response.Content.ReadFromJsonAsync<List<Nutricionista>>() ?? new List<Nutricionista>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar nutricionistas: {response.StatusCode}");
                }

                // Cargar personas para relacionar
                await LoadPersonas();

                // Relacionar manualmente los nutricionistas con sus personas
                foreach (var nutricionista in Nutricionistas)
                {
                    nutricionista.Persona = Personas.FirstOrDefault(p => p.IdPersona == nutricionista.IdPersona);
                }

                return Page();
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible. Por favor, verifica que el backend esté en ejecución.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar los nutricionistas: {ex.Message}");
                return Page();
            }
        }

        private async Task LoadPersonas()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Personas");
                if (response.IsSuccessStatusCode)
                {
                    Personas = await response.Content.ReadFromJsonAsync<List<Persona>>() ?? new List<Persona>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar personas: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar personas: {ex.Message}");
            }
        }
    }
}