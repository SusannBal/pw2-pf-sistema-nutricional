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
    public class PacienteConsultasModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Consulta> Consultas { get; set; } = new();
        public List<Nutricionista> Nutricionistas { get; set; } = new(); // Added
        public List<Persona> Personas { get; set; } = new(); // Added

        public PacienteConsultasModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar si el usuario está logueado y es paciente
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
                var response = await _httpClient.GetAsync($"api/Consultas/byPaciente/{pacienteId}");
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

                // Load related data (Nutricionistas and Personas)
                await LoadRelatedData();

                // Manually link consultations with their Nutricionistas and Personas
                foreach (var consulta in Consultas)
                {
                    consulta.Nutricionista = Nutricionistas.FirstOrDefault(n => n.IdNutricionista == consulta.IdNutricionista);
                    if (consulta.Nutricionista != null)
                    {
                        consulta.Nutricionista.Persona = Personas.FirstOrDefault(p => p.IdPersona == consulta.Nutricionista.IdPersona);
                    }
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

        private async Task LoadRelatedData()
        {
            try
            {
                // Load Nutricionistas
                var nutricionistasResponse = await _httpClient.GetAsync("api/Nutricionistas");
                if (nutricionistasResponse.IsSuccessStatusCode)
                {
                    Nutricionistas = await nutricionistasResponse.Content.ReadFromJsonAsync<List<Nutricionista>>() ?? new List<Nutricionista>();
                }

                // Load Personas
                var personasResponse = await _httpClient.GetAsync("api/Personas");
                if (personasResponse.IsSuccessStatusCode)
                {
                    Personas = await personasResponse.Content.ReadFromJsonAsync<List<Persona>>() ?? new List<Persona>();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar datos relacionados: {ex.Message}");
            }
        }
    }
}