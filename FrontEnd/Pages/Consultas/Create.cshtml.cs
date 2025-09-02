using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;

namespace Frontend.Pages.Consultas
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public Consulta Consulta { get; set; } = new();

        public List<Paciente> Pacientes { get; set; } = new();
        public List<Nutricionista> Nutricionistas { get; set; } = new();
        public List<Persona> Personas { get; set; } = new();

        [TempData]
        public string Mensaje { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDependencies();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDependencies();
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Consultas", Consulta);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ResponseCreate>();
                    Mensaje = result?.mensaje ?? "Consulta creada exitosamente";
                    return RedirectToPage("Index");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al crear la consulta");
                    await LoadDependencies();
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API.");
                await LoadDependencies();
                return Page();
            }
        }

        private async Task LoadDependencies()
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
                }

                // Cargar nutricionistas
                var nutricionistasResponse = await _httpClient.GetAsync("api/Nutricionistas");
                if (nutricionistasResponse.IsSuccessStatusCode)
                {
                    Nutricionistas = await nutricionistasResponse.Content.ReadFromJsonAsync<List<Nutricionista>>() ?? new List<Nutricionista>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar nutricionistas: {nutricionistasResponse.StatusCode}");
                }

                // Cargar todas las personas para poder relacionarlas
                var personasResponse = await _httpClient.GetAsync("api/Personas");
                if (personasResponse.IsSuccessStatusCode)
                {
                    Personas = await personasResponse.Content.ReadFromJsonAsync<List<Persona>>() ?? new List<Persona>();

                    // Relacionar personas con pacientes
                    foreach (var paciente in Pacientes)
                    {
                        paciente.Persona = Personas.FirstOrDefault(p => p.IdPersona == paciente.IdPersona);
                    }

                    // Relacionar personas con nutricionistas
                    foreach (var nutricionista in Nutricionistas)
                    {
                        nutricionista.Persona = Personas.FirstOrDefault(p => p.IdPersona == nutricionista.IdPersona);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar personas: {personasResponse.StatusCode}");
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible para cargar dependencias. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar dependencias: {ex.Message}");
            }
        }

        private class ResponseCreate
        {
            public string mensaje { get; set; }
            public Consulta consulta { get; set; }
        }

        private class ResponseError
        {
            public string mensaje { get; set; }
        }
    }
}