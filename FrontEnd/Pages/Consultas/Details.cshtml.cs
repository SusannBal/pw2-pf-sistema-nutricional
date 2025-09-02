using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Frontend.Pages.Consultas
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public Consulta Consulta { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/Consultas/{id}");
                if (response.IsSuccessStatusCode)
                {
                    Consulta = await response.Content.ReadFromJsonAsync<Consulta>();
                    if (Consulta == null)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar la consulta: {response.StatusCode}");
                    return NotFound();
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar la consulta: {ex.Message}");
            }

            return Page();
        }
    }
}