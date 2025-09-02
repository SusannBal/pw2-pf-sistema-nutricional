using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Frontend.Pages.Recomendaciones
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public Recomendacion Recomendacion { get; set; } = new();

        public List<Consulta> Consultas { get; set; } = new();

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
                var response = await _httpClient.PostAsJsonAsync("api/Recomendaciones", Recomendacion);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ResponseCreate>();
                    Mensaje = result?.mensaje ?? "Recomendación creada exitosamente";
                    return RedirectToPage("Index");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al crear la recomendación");
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
                var response = await _httpClient.GetAsync("api/Consultas");
                if (response.IsSuccessStatusCode)
                {
                    Consultas = await response.Content.ReadFromJsonAsync<List<Consulta>>() ?? new List<Consulta>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar consultas: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible para cargar consultas. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar consultas: {ex.Message}");
            }
        }

        private class ResponseCreate
        {
            public string mensaje { get; set; }
            public Recomendacion recomendacion { get; set; }
        }

        private class ResponseError
        {
            public string mensaje { get; set; }
        }
    }
}