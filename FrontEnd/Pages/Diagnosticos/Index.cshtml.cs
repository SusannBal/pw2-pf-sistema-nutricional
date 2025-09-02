using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frontend.Pages.Diagnosticos
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Diagnostico> Diagnosticos { get; set; } = new();
        public List<Consulta> Consultas { get; set; } = new();
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
                // Cargar diagnósticos
                var response = await _httpClient.GetAsync("api/Diagnosticos");
                if (response.IsSuccessStatusCode)
                {
                    Diagnosticos = await response.Content.ReadFromJsonAsync<List<Diagnostico>>() ?? new List<Diagnostico>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar diagnósticos: {response.StatusCode}");
                }

                // Cargar datos relacionados para mostrar información de consultas y pacientes
                await LoadRelatedData();

                // Relacionar manualmente los diagnósticos con consultas, pacientes y personas
                foreach (var diagnostico in Diagnosticos)
                {
                    diagnostico.Consulta = Consultas.FirstOrDefault(c => c.IdConsulta == diagnostico.IdConsulta);
                    if (diagnostico.Consulta != null)
                    {
                        diagnostico.Consulta.Paciente = Pacientes.FirstOrDefault(p => p.IdPaciente == diagnostico.Consulta.IdPaciente);
                        if (diagnostico.Consulta.Paciente != null)
                        {
                            diagnostico.Consulta.Paciente.Persona = Personas.FirstOrDefault(p => p.IdPersona == diagnostico.Consulta.Paciente.IdPersona);
                        }
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
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar los diagnósticos: {ex.Message}");
                return Page();
            }
        }

        private async Task LoadRelatedData()
        {
            try
            {
                // Cargar consultas
                var consultasResponse = await _httpClient.GetAsync("api/Consultas");
                if (consultasResponse.IsSuccessStatusCode)
                {
                    Consultas = await consultasResponse.Content.ReadFromJsonAsync<List<Consulta>>() ?? new List<Consulta>();
                }

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