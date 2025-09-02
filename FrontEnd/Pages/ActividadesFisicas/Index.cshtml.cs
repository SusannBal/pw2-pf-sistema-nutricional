using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frontend.Pages.ActividadesFisicas
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<ActividadFisica> ActividadesFisicas { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/ActividadFisica");

                if (response.IsSuccessStatusCode)
                {
                    ActividadesFisicas = await response.Content.ReadFromJsonAsync<List<ActividadFisica>>() ?? new List<ActividadFisica>();
                    return Page();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ModelState.AddModelError(string.Empty, "No se encontraron actividades f�sicas.");
                    return Page();
                }

                ModelState.AddModelError(string.Empty, $"Error al cargar las actividades f�sicas: {response.StatusCode}");
                return Page();
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no est� disponible. Por favor, verifica que el backend est� en ejecuci�n.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurri� un error inesperado al cargar las actividades f�sicas: {ex.Message}");
                return Page();
            }
        }
    }
}