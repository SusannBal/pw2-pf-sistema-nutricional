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

namespace Frontend.Pages.Recomendaciones
{
    public class PacienteRecomendacionesModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Recomendacion> Recomendaciones { get; set; } = new();
        public List<Consulta> ConsultasDelPaciente { get; set; } = new(); // To hold consultations for the logged-in patient

        public PacienteRecomendacionesModel(IHttpClientFactory httpClientFactory)
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
                // 1. Get all consultations for the logged-in patient
                var consultasResponse = await _httpClient.GetAsync($"api/Consultas/byPaciente/{pacienteId}");
                if (consultasResponse.IsSuccessStatusCode)
                {
                    ConsultasDelPaciente = await consultasResponse.Content.ReadFromJsonAsync<List<Consulta>>() ?? new List<Consulta>();
                }
                else if (consultasResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar las consultas del paciente: {consultasResponse.StatusCode}");
                }

                // 2. Get recommendations for each of these consultations
                var allRecomendaciones = new List<Recomendacion>();
                foreach (var consulta in ConsultasDelPaciente)
                {
                    // Assuming your API has an endpoint to get recommendations by IdConsulta
                    var recResponse = await _httpClient.GetAsync($"api/Recomendaciones?idConsulta={consulta.IdConsulta}");
                    if (recResponse.IsSuccessStatusCode)
                    {
                        var recsForConsulta = await recResponse.Content.ReadFromJsonAsync<List<Recomendacion>>() ?? new List<Recomendacion>();
                        allRecomendaciones.AddRange(recsForConsulta);
                    }
                    else if (recResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        ModelState.AddModelError(string.Empty, $"Error al cargar recomendaciones para la consulta {consulta.IdConsulta}: {recResponse.StatusCode}");
                    }
                }
                Recomendaciones = allRecomendaciones.DistinctBy(r => r.IdRecomendacion).ToList(); // Remove duplicates

                // 3. Link recommendations back to their consultations for display
                foreach (var recomendacion in Recomendaciones)
                {
                    recomendacion.Consulta = ConsultasDelPaciente.FirstOrDefault(c => c.IdConsulta == recomendacion.IdConsulta);
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar las recomendaciones: {ex.Message}");
            }

            return Page();
        }
    }
}