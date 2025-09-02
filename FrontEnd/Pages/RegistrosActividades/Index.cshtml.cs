using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frontend.Pages.RegistrosActividades
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Backend.Models.RegistroActividad> RegistrosActividad { get; set; } = new();
        public List<Paciente> Pacientes { get; set; } = new();
        public List<Persona> Personas { get; set; } = new();
        public List<ActividadFisica> ActividadesFisicas { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Cargar registros de actividad
                var response = await _httpClient.GetAsync("api/RegistroActividad");
                if (response.IsSuccessStatusCode)
                {
                    RegistrosActividad = await response.Content.ReadFromJsonAsync<List<Backend.Models.RegistroActividad>>() ?? new List<Backend.Models.RegistroActividad>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar registros de actividad: {response.StatusCode}");
                }

                // Cargar datos relacionados para mostrar nombres
                await LoadRelatedData();

                // Relacionar manualmente los registros con pacientes, personas y actividades
                foreach (var registro in RegistrosActividad)
                {
                    registro.Paciente = Pacientes.FirstOrDefault(p => p.IdPaciente == registro.IdPaciente);
                    if (registro.Paciente != null)
                    {
                        registro.Paciente.Persona = Personas.FirstOrDefault(p => p.IdPersona == registro.Paciente.IdPersona);
                    }
                    registro.Actividad = ActividadesFisicas.FirstOrDefault(a => a.IdActividad == registro.IdActividad);
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
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar los registros de actividad: {ex.Message}");
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

                // Cargar actividades físicas
                var actividadesResponse = await _httpClient.GetAsync("api/ActividadFisica");
                if (actividadesResponse.IsSuccessStatusCode)
                {
                    ActividadesFisicas = await actividadesResponse.Content.ReadFromJsonAsync<List<ActividadFisica>>() ?? new List<ActividadFisica>();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar datos relacionados: {ex.Message}");
            }
        }
    }
}