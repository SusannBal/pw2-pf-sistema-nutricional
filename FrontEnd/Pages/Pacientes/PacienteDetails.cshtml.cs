using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Frontend.Pages.Pacientes
{
    public class PacienteDetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public PacienteDetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public Paciente Paciente { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/Pacientes/{id}");
                if (response.IsSuccessStatusCode)
                {
                    Paciente = await response.Content.ReadFromJsonAsync<Paciente>();
                    if (Paciente == null)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar el paciente: {response.StatusCode}");
                    return NotFound();
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no est� disponible. Por favor, verifica que el backend est� en ejecuci�n.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurri� un error inesperado al cargar el paciente: {ex.Message}");
            }

            return Page();
        }
    }
}