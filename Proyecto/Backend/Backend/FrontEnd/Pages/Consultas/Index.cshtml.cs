using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frontend.Pages.Consultas
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Consulta> Consultas { get; set; } = new();
        public List<Paciente> Pacientes { get; set; } = new();
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
                // Cargar consultas
                var consultasResponse = await _httpClient.GetAsync("api/Consultas");
                if (consultasResponse.IsSuccessStatusCode)
                {
                    Consultas = await consultasResponse.Content.ReadFromJsonAsync<List<Consulta>>() ?? new List<Consulta>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar consultas: {consultasResponse.StatusCode}");
                }

                // Cargar pacientes y nutricionistas para relacionar
                await LoadRelatedData();

                // Relacionar manualmente las consultas con pacientes y nutricionistas
                foreach (var consulta in Consultas)
                {
                    consulta.Paciente = Pacientes.FirstOrDefault(p => p.IdPaciente == consulta.IdPaciente);
                    consulta.Nutricionista = Nutricionistas.FirstOrDefault(n => n.IdNutricionista == consulta.IdNutricionista);
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
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar las consultas: {ex.Message}");
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

                // Cargar nutricionistas
                var nutricionistasResponse = await _httpClient.GetAsync("api/Nutricionistas");
                if (nutricionistasResponse.IsSuccessStatusCode)
                {
                    Nutricionistas = await nutricionistasResponse.Content.ReadFromJsonAsync<List<Nutricionista>>() ?? new List<Nutricionista>();
                }

                // Cargar personas para relacionar
                var personasResponse = await _httpClient.GetAsync("api/Personas");
                if (personasResponse.IsSuccessStatusCode)
                {
                    Personas = await personasResponse.Content.ReadFromJsonAsync<List<Persona>>() ?? new List<Persona>();

                    // Relacionar pacientes con personas
                    foreach (var paciente in Pacientes)
                    {
                        paciente.Persona = Personas.FirstOrDefault(p => p.IdPersona == paciente.IdPersona);
                    }

                    // Relacionar nutricionistas con personas
                    foreach (var nutricionista in Nutricionistas)
                    {
                        nutricionista.Persona = Personas.FirstOrDefault(p => p.IdPersona == nutricionista.IdPersona);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar datos relacionados: {ex.Message}");
            }
        }
    }
}