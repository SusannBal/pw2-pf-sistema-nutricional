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

namespace Frontend.Pages.PlanesNutricionales // Namespace corregido
{
    public class PacientePlanesModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<PlanNutricional> PlanesNutricionales { get; set; } = new();
        public List<Consulta> Consultas { get; set; } = new();

        public PacientePlanesModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("UserRole") != "Paciente")
            {
                return RedirectToPage("/Login");
            }

            var pacienteId = HttpContext.Session.GetInt32("UserId");
            if (pacienteId == null)
            {
                ModelState.AddModelError(string.Empty, "ID de paciente no encontrado en la sesión.");
                return Page();
            }

            try
            {
                // Obtener las consultas del paciente
                var consultasResponse = await _httpClient.GetAsync($"api/Consultas/byPaciente/{pacienteId}");
                if (consultasResponse.IsSuccessStatusCode)
                {
                    Consultas = await consultasResponse.Content.ReadFromJsonAsync<List<Consulta>>() ?? new List<Consulta>();
                }
                else if (consultasResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar las consultas del paciente: {consultasResponse.StatusCode}");
                }

                // Obtener los planes nutricionales asociados a esas consultas
                var allPlanes = new List<PlanNutricional>();
                foreach (var consulta in Consultas)
                {
                    var planesResponse = await _httpClient.GetAsync($"api/PlanNutricional?idConsulta={consulta.IdConsulta}");
                    if (planesResponse.IsSuccessStatusCode)
                    {
                        var planesForConsulta = await planesResponse.Content.ReadFromJsonAsync<List<PlanNutricional>>() ?? new List<PlanNutricional>();
                        allPlanes.AddRange(planesForConsulta);
                    }
                    else if (planesResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        ModelState.AddModelError(string.Empty, $"Error al cargar planes para la consulta {consulta.IdConsulta}: {planesResponse.StatusCode}");
                    }
                }
                PlanesNutricionales = allPlanes.DistinctBy(p => p.IdPlan).ToList();

                // Relacionar manualmente los planes con consultas
                foreach (var plan in PlanesNutricionales)
                {
                    plan.Consulta = Consultas.FirstOrDefault(c => c.IdConsulta == plan.IdConsulta);
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar los planes nutricionales: {ex.Message}");
            }

            return Page();
        }
    }
}