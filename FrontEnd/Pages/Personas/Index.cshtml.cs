using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq; // Added for .Any()

namespace Frontend.Pages.Personas
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Persona> Personas { get; set; } = new();
        public List<Paciente> Pacientes { get; set; } = new(); // Added to check roles
        public List<Nutricionista> Nutricionistas { get; set; } = new(); // Added to check roles

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Personas");

                if (response.IsSuccessStatusCode)
                {
                    Personas = await response.Content.ReadFromJsonAsync<List<Persona>>() ?? new List<Persona>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar las personas: {response.StatusCode}");
                }

                // Load Pacientes and Nutricionistas to determine roles
                var pacientesResponse = await _httpClient.GetAsync("api/Pacientes");
                if (pacientesResponse.IsSuccessStatusCode)
                {
                    Pacientes = await pacientesResponse.Content.ReadFromJsonAsync<List<Paciente>>() ?? new List<Paciente>();
                }

                var nutricionistasResponse = await _httpClient.GetAsync("api/Nutricionistas");
                if (nutricionistasResponse.IsSuccessStatusCode)
                {
                    Nutricionistas = await nutricionistasResponse.Content.ReadFromJsonAsync<List<Nutricionista>>() ?? new List<Nutricionista>();
                }

                if (!Personas.Any() && !ModelState.IsValid) // Only show "No data" if no persons and no other errors
                {
                    ModelState.AddModelError(string.Empty, "No se encontraron personas.");
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
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar las personas: {ex.Message}");
                return Page();
            }
        }
    }
}