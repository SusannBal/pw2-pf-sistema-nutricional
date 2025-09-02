using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Frontend.Pages.Recordatorios
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public Recordatorio Recordatorio { get; set; } = new();

        public List<Persona> Personas { get; set; } = new();

        [TempData]
        public string Mensaje { get; set; }

        public async Task OnGetAsync()
        {
            await LoadPersonas();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadPersonas();
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Recordatorios", Recordatorio);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ResponseCreate>();
                    Mensaje = result?.mensaje ?? "Recordatorio creado exitosamente";
                    return RedirectToPage("Index");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al crear el recordatorio");
                    await LoadPersonas();
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API.");
                await LoadPersonas();
                return Page();
            }
        }

        private async Task LoadPersonas()
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
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible para cargar personas. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar las personas: {ex.Message}");
            }
        }

        private class ResponseCreate
        {
            public string mensaje { get; set; }
            public Recordatorio recordatorio { get; set; }
        }

        private class ResponseError
        {
            public string mensaje { get; set; }
        }
    }
}