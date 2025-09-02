using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Frontend.Pages.ActividadesFisicas
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public ActividadFisica ActividadFisica { get; set; } = new();

        [TempData]
        public string Mensaje { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ActividadFisica", ActividadFisica);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ResponseCreate>();
                    Mensaje = result?.mensaje ?? "Actividad física creada exitosamente";
                    return RedirectToPage("Index");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al crear la actividad física");
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API.");
                return Page();
            }
        }

        private class ResponseCreate
        {
            public string mensaje { get; set; }
            public ActividadFisica actividadFisica { get; set; }
        }

        private class ResponseError
        {
            public string mensaje { get; set; }
        }
    }
}