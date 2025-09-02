using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Frontend.Pages.Pacientes
{
    public class RegisterModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public RegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public Persona Persona { get; set; } = new();

        [BindProperty]
        public Paciente Paciente { get; set; } = new();

        [TempData]
        public string Mensaje { get; set; }

        public void OnGet()
        {
            // Inicializar si es necesario
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // 1. Crear la Persona
                var personaResponse = await _httpClient.PostAsJsonAsync("api/Personas", Persona);
                if (!personaResponse.IsSuccessStatusCode)
                {
                    var error = await personaResponse.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al registrar la persona.");
                    return Page();
                }
                var createdPersona = await personaResponse.Content.ReadFromJsonAsync<ResponseCreatePersona>();
                Paciente.IdPersona = createdPersona.persona.IdPersona; // Asignar el ID de la persona creada

                // 2. Crear el Paciente
                var pacienteResponse = await _httpClient.PostAsJsonAsync("api/Pacientes", Paciente);
                if (pacienteResponse.IsSuccessStatusCode)
                {
                    Mensaje = "Paciente registrado exitosamente. Ahora puedes iniciar sesión.";
                    return RedirectToPage("/Login");
                }
                else
                {
                    // Si falla la creación del paciente, intentar eliminar la persona creada para evitar inconsistencias
                    await _httpClient.DeleteAsync($"api/Personas/{Paciente.IdPersona}");
                    var error = await pacienteResponse.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al registrar el paciente.");
                    return Page();
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API. Por favor, verifica que el backend esté en ejecución.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado: {ex.Message}");
                return Page();
            }
        }

        private class ResponseCreatePersona
        {
            public string mensaje { get; set; }
            public Persona persona { get; set; }
        }

        private class ResponseError
        {
            public string mensaje { get; set; }
        }
    }
}