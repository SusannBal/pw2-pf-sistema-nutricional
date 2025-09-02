using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Frontend.Pages.Personas
{
    public class EditModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public Persona Persona { get; set; }

        [TempData]
        public string Mensaje { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/Personas/{id}");
                if (response.IsSuccessStatusCode)
                {
                    Persona = await response.Content.ReadFromJsonAsync<Persona>();
                    if (Persona == null)
                    {
                        return NotFound();
                    }
                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar la persona: {response.StatusCode}");
                    return NotFound();
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API. Por favor, verifica que el backend esté en ejecución.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar la persona: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Personas/{Persona.IdPersona}", Persona);

                if (response.IsSuccessStatusCode)
                {
                    Mensaje = "Persona actualizada exitosamente";
                    return RedirectToPage("./Index");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al actualizar la persona");
                    return Page();
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al actualizar la persona: {ex.Message}");
                return Page();
            }
        }

        private class ResponseError
        {
            public string mensaje { get; set; }
        }
    }
}