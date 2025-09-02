using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;

namespace Frontend.Pages.Pacientes
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Paciente> Pacientes { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Pacientes");

                if (response.IsSuccessStatusCode)
                {
                    Pacientes = await response.Content.ReadFromJsonAsync<List<Paciente>>();
                    return Page();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ModelState.AddModelError(string.Empty, "No se encontraron pacientes.");
                    return Page();
                }

                ModelState.AddModelError(string.Empty, $"Error al cargar los pacientes: {response.StatusCode}");
                return Page();
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible. Por favor, verifica que el backend esté en ejecución.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar los pacientes: {ex.Message}");
                return Page();
            }
        }
    }
}
