using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;

namespace Frontend.Pages.Personas
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Persona> Personas { get; set; } = new();

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
                    // Ahora la respuesta es una lista directa, sin $values
                    Personas = await response.Content.ReadFromJsonAsync<List<Persona>>();
                    return Page();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ModelState.AddModelError(string.Empty, "No se encontraron personas.");
                    return Page();
                }

                ModelState.AddModelError(string.Empty, $"Error al cargar las personas: {response.StatusCode}");
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
