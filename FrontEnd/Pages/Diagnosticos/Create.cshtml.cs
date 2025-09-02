using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

// ... (existing usings)
using Microsoft.AspNetCore.Mvc.Rendering; // Add this for SelectList

namespace Frontend.Pages.Diagnosticos
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public Diagnostico Diagnostico { get; set; } = new();

        public List<Consulta> Consultas { get; set; } = new();
        public List<Paciente> Pacientes { get; set; } = new();
        public List<Persona> Personas { get; set; } = new();

        public SelectList ConsultaOptions { get; set; } // Added for dropdown

        [TempData]
        public string Mensaje { get; set; }

        public async Task OnGetAsync(int? idConsulta, int? idPaciente) // Added idConsulta and idPaciente
        {
            await LoadConsultas();

            if (idConsulta.HasValue)
            {
                Diagnostico.IdConsulta = idConsulta.Value;
            }
            else if (idPaciente.HasValue)
            {
                // Try to find a recent consultation for this patient
                var recentConsulta = Consultas
                    .Where(c => c.IdPaciente == idPaciente.Value)
                    .OrderByDescending(c => c.Fecha)
                    .FirstOrDefault();
                if (recentConsulta != null)
                {
                    Diagnostico.IdConsulta = recentConsulta.IdConsulta;
                }
            }

            ConsultaOptions = new SelectList(Consultas, "IdConsulta", "DisplayInfo", Diagnostico.IdConsulta);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadConsultas();
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Diagnosticos", Diagnostico);
                if (response.IsSuccessStatusCode)
                {
                    Mensaje = "Diagnóstico creado exitosamente";
                    return RedirectToPage("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al crear el diagnóstico");
                    await LoadConsultas();
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API.");
                await LoadConsultas();
                return Page();
            }
        }

        private async Task LoadConsultas()
        {
            try
            {
                // Cargar consultas
                var response = await _httpClient.GetAsync("api/Consultas");
                if (response.IsSuccessStatusCode)
                {
                    Consultas = await response.Content.ReadFromJsonAsync<List<Consulta>>() ?? new List<Consulta>();
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

                // Relacionar consultas con pacientes y personas y crear DisplayInfo
                foreach (var consulta in Consultas)
                {
                    consulta.Paciente = Pacientes.FirstOrDefault(p => p.IdPaciente == consulta.IdPaciente);
                    if (consulta.Paciente != null)
                    {
                        consulta.Paciente.Persona = Personas.FirstOrDefault(p => p.IdPersona == consulta.Paciente.IdPersona);
                        consulta.DisplayInfo = $"Consulta ID: {consulta.IdConsulta} - Paciente: {consulta.Paciente.Persona?.Nombre} {consulta.Paciente.Persona?.ApellidoPaterno} - Motivo: {consulta.Motivo}";
                    }
                    else
                    {
                        consulta.DisplayInfo = $"Consulta ID: {consulta.IdConsulta} - Paciente ID: {consulta.IdPaciente} - Motivo: {consulta.Motivo}";
                    }
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible para cargar consultas. Por favor, verifica que el backend esté en ejecución.");
                Consultas = new List<Consulta>();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar consultas: {ex.Message}");
                Consultas = new List<Consulta>();
            }
        }
    }
}
