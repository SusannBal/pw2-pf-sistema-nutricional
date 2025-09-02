using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Frontend.Pages.Nutricionistas
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
        public Nutricionista Nutricionista { get; set; } = new();

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
                Nutricionista.IdPersona = createdPersona.persona.IdPersona; // Asignar el ID de la persona creada

                // 2. Crear el Nutricionista
                var nutricionistaResponse = await _httpClient.PostAsJsonAsync("api/Nutricionistas", Nutricionista);
                if (nutricionistaResponse.IsSuccessStatusCode)
                {
                    Mensaje = "Nutricionista registrado exitosamente. Ahora puedes iniciar sesión.";
                    return RedirectToPage("/Login");
                }
                else
                {
                    // Si falla la creación del nutricionista, intentar eliminar la persona creada para evitar inconsistencias
                    await _httpClient.DeleteAsync($"api/Personas/{Nutricionista.IdPersona}");
                    var error = await nutricionistaResponse.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al registrar el nutricionista.");
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