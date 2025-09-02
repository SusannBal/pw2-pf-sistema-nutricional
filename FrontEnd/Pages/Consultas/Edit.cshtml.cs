using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Linq;
using System.Collections.Generic;

namespace Frontend.Pages.Consultas
{
    public class EditModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public Consulta Consulta { get; set; }

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
                var response = await _httpClient.GetAsync($"api/Consultas/{id}");
                if (response.IsSuccessStatusCode)
                {
                    Consulta = await response.Content.ReadFromJsonAsync<Consulta>();
                    if (Consulta == null)
                    {
                        return NotFound();
                    }
                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar la consulta: {response.StatusCode}");
                    return NotFound();
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API. Por favor, verifica que el backend está en ejecución.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar la consulta: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Recargar datos relacionados si es necesario para la vista
                // (Paciente y Nutricionista ya vienen en el objeto Consulta si se cargó correctamente en OnGet)
                return Page();
            }

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Consultas/{Consulta.IdConsulta}", Consulta);

                if (response.IsSuccessStatusCode)
                {
                    Mensaje = "Consulta actualizada exitosamente";
                    return RedirectToPage("./Index"); // O a NutricionistaConsultas
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al actualizar la consulta");
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
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al actualizar la consulta: {ex.Message}");
                return Page();
            }
        }

        private class ResponseError
        {
            public string mensaje { get; set; }
        }
    }
}