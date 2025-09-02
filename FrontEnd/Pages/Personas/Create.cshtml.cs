using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Frontend.Pages.Personas
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public Persona Persona { get; set; } = new();

        [TempData]
        public string Mensaje { get; set; }

        public void OnGet()
        {
            // No hace falta cargar nada para crear
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Personas", Persona);

                if (response.IsSuccessStatusCode)
                {
                    // Leer mensaje de éxito si quieres
                    var result = await response.Content.ReadFromJsonAsync<ResponseCreate>();

                    Mensaje = result?.mensaje ?? "Persona creada exitosamente";

                    return RedirectToPage("Index");
                }
                else
                {
                    // Leer mensaje de error del backend
                    var error = await response.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al crear la persona");
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API.");
                return Page();
            }
        }

        // Clases para leer respuesta JSON del backend
        private class ResponseCreate
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
