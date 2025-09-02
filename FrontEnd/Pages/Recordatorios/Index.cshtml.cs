using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frontend.Pages.Recordatorios
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Recordatorio> Recordatorios { get; set; } = new();
        public List<Persona> Personas { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Cargar recordatorios
                var response = await _httpClient.GetAsync("api/Recordatorios");
                if (response.IsSuccessStatusCode)
                {
                    Recordatorios = await response.Content.ReadFromJsonAsync<List<Recordatorio>>() ?? new List<Recordatorio>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar recordatorios: {response.StatusCode}");
                }

                // Cargar personas para relacionar
                await LoadPersonas();

                // Relacionar manualmente los recordatorios con personas
                foreach (var recordatorio in Recordatorios)
                {
                    recordatorio.Persona = Personas.FirstOrDefault(p => p.IdPersona == recordatorio.IdPersona);
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
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar los recordatorios: {ex.Message}");
                return Page();
            }
        }

        private async Task LoadPersonas()
        {
            try
            {
                // Cargar personas
                var personasResponse = await _httpClient.GetAsync("api/Personas");
                if (personasResponse.IsSuccessStatusCode)
                {
                    Personas = await personasResponse.Content.ReadFromJsonAsync<List<Persona>>() ?? new List<Persona>();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar personas: {ex.Message}");
            }
        }
    }
}