using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Frontend.Pages.PlanesNutricionales
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public PlanNutricional PlanNutricional { get; set; }
        public List<Comida> Comidas { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Cargar el plan nutricional
                var planResponse = await _httpClient.GetAsync($"api/PlanNutricional/{id}");
                if (planResponse.IsSuccessStatusCode)
                {
                    PlanNutricional = await planResponse.Content.ReadFromJsonAsync<PlanNutricional>();
                    if (PlanNutricional == null)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar el plan nutricional: {planResponse.StatusCode}");
                    return NotFound();
                }

                // Cargar las comidas asociadas a este plan
                // Assuming your API has an endpoint to get meals by IdPlan
                var comidasResponse = await _httpClient.GetAsync($"api/Comidas?idPlan={id}");
                if (comidasResponse.IsSuccessStatusCode)
                {
                    Comidas = await comidasResponse.Content.ReadFromJsonAsync<List<Comida>>() ?? new List<Comida>();
                }
                else if (comidasResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar las comidas del plan: {comidasResponse.StatusCode}");
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar los detalles del plan: {ex.Message}");
            }

            return Page();
        }
    }
}