using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

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

        [TempData]
        public string Mensaje { get; set; }

        public async Task OnGetAsync()
        {
            await LoadConsultas();
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

                // Relacionar consultas con pacientes y personas
                foreach (var consulta in Consultas)
                {
                    consulta.Paciente = Pacientes.FirstOrDefault(p => p.IdPaciente == consulta.IdPaciente);
                    if (consulta.Paciente != null)
                    {
                        consulta.Paciente.Persona = Personas.FirstOrDefault(p => p.IdPersona == consulta.Paciente.IdPersona);
                    }
                }
            }
            catch
            {
                Consultas = new List<Consulta>();
            }
        }
    }
}