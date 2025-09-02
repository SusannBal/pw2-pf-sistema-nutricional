using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frontend.Pages.PlanesNutricionales
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<PlanNutricional> PlanesNutricionales { get; set; } = new();
        public List<Consulta> Consultas { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Cargar planes nutricionales
                var response = await _httpClient.GetAsync("api/PlanNutricional");
                if (response.IsSuccessStatusCode)
                {
                    PlanesNutricionales = await response.Content.ReadFromJsonAsync<List<PlanNutricional>>() ?? new List<PlanNutricional>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar planes nutricionales: {response.StatusCode}");
                }

                // Cargar consultas para relacionar
                await LoadConsultas();

                // Relacionar manualmente los planes con consultas
                foreach (var plan in PlanesNutricionales)
                {
                    plan.Consulta = Consultas.FirstOrDefault(c => c.IdConsulta == plan.IdConsulta);
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
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar los planes nutricionales: {ex.Message}");
                return Page();
            }
        }

        private async Task LoadConsultas()
        {
            try
            {
                // Cargar consultas
                var consultasResponse = await _httpClient.GetAsync("api/Consultas");
                if (consultasResponse.IsSuccessStatusCode)
                {
                    Consultas = await consultasResponse.Content.ReadFromJsonAsync<List<Consulta>>() ?? new List<Consulta>();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar consultas: {ex.Message}");
            }
        }
    }
}