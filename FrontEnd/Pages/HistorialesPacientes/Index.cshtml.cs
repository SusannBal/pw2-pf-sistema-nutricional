using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frontend.Pages.HistorialesPacientes
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<HistorialPaciente> HistorialesPacientes { get; set; } = new();
        public List<Paciente> Pacientes { get; set; } = new();
        public List<Persona> Personas { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Cargar historiales de pacientes
                var response = await _httpClient.GetAsync("api/HistorialPaciente");
                if (response.IsSuccessStatusCode)
                {
                    HistorialesPacientes = await response.Content.ReadFromJsonAsync<List<HistorialPaciente>>() ?? new List<HistorialPaciente>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar historiales de pacientes: {response.StatusCode}");
                }

                // Cargar datos relacionados para mostrar nombres
                await LoadRelatedData();

                // Relacionar manualmente los historiales con pacientes y personas
                foreach (var historial in HistorialesPacientes)
                {
                    historial.Paciente = Pacientes.FirstOrDefault(p => p.IdPaciente == historial.IdPaciente);
                    if (historial.Paciente != null)
                    {
                        historial.Paciente.Persona = Personas.FirstOrDefault(p => p.IdPersona == historial.Paciente.IdPersona);
                    }
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
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar los historiales de pacientes: {ex.Message}");
                return Page();
            }
        }

        private async Task LoadRelatedData()
        {
            try
            {
                // Cargar pacientes
                var pacientesResponse = await _httpClient.GetAsync("api/Pacientes");
                if (pacientesResponse.IsSuccessStatusCode)
                {
                    Pacientes = await pacientesResponse.Content.ReadFromJsonAsync<List<Paciente>>() ?? new List<Paciente>();
                }

                // Cargar personas
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