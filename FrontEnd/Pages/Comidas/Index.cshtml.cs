using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frontend.Pages.Comidas
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Comida> Comidas { get; set; } = new();
        public List<PlanNutricional> PlanesNutricionales { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Cargar comidas
                var response = await _httpClient.GetAsync("api/Comidas");
                if (response.IsSuccessStatusCode)
                {
                    Comidas = await response.Content.ReadFromJsonAsync<List<Comida>>() ?? new List<Comida>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar comidas: {response.StatusCode}");
                }

                // Cargar planes nutricionales para relacionar
                await LoadPlanesNutricionales();

                // Relacionar manualmente las comidas con planes nutricionales
                foreach (var comida in Comidas)
                {
                    comida.PlanNutricional = PlanesNutricionales.FirstOrDefault(p => p.IdPlan == comida.IdPlan);
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
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar las comidas: {ex.Message}");
                return Page();
            }
        }

        private async Task LoadPlanesNutricionales()
        {
            try
            {
                // Cargar planes nutricionales
                var planesResponse = await _httpClient.GetAsync("api/PlanNutricional");
                if (planesResponse.IsSuccessStatusCode)
                {
                    PlanesNutricionales = await planesResponse.Content.ReadFromJsonAsync<List<PlanNutricional>>() ?? new List<PlanNutricional>();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar planes nutricionales: {ex.Message}");
            }
        }
    }
}