using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Frontend.Pages.HistorialesMedicos
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public HistorialMedico HistorialMedico { get; set; } = new();

        public List<Paciente> Pacientes { get; set; } = new();
        public List<Persona> Personas { get; set; } = new();

        [TempData]
        public string Mensaje { get; set; }

        public async Task OnGetAsync()
        {
            await LoadPacientes();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadPacientes();
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/HistorialMedico", HistorialMedico);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ResponseCreate>();
                    Mensaje = result?.mensaje ?? "Historial médico creado exitosamente";
                    return RedirectToPage("Index");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al crear el historial médico");
                    await LoadPacientes();
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API.");
                await LoadPacientes();
                return Page();
            }
        }

        private async Task LoadPacientes()
        {
            try
            {
                // Cargar pacientes
                var pacientesResponse = await _httpClient.GetAsync("api/Pacientes");
                if (pacientesResponse.IsSuccessStatusCode)
                {
                    Pacientes = await pacientesResponse.Content.ReadFromJsonAsync<List<Paciente>>() ?? new List<Paciente>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar pacientes: {pacientesResponse.StatusCode}");
                    Pacientes = new List<Paciente>();
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
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar personas: {personasResponse.StatusCode}");
                    Personas = new List<Persona>();
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible para cargar pacientes. Por favor, verifica que el backend esté en ejecución.");
                Pacientes = new List<Paciente>();
                Personas = new List<Persona>();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar pacientes: {ex.Message}");
                Pacientes = new List<Paciente>();
                Personas = new List<Persona>();
            }
        }

        private class ResponseCreate
        {
            public string mensaje { get; set; }
            public HistorialMedico historialMedico { get; set; }
        }

        private class ResponseError
        {
            public string mensaje { get; set; }
        }
    }
}