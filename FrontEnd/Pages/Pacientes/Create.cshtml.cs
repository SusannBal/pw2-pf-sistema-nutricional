using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Frontend.Pages.Pacientes
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public Paciente Paciente { get; set; } = new();

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
                await LoadPersonas(); // Recargar personas si hay errores de validación
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Pacientes", Paciente);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ResponseCreate>();
                    Mensaje = result?.mensaje ?? "Paciente creado exitosamente";
                    return RedirectToPage("Index");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al crear el paciente");
                    await LoadPersonas(); // Recargar personas si hay errores del backend
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API.");
                await LoadPersonas(); // Recargar personas si hay errores de conexión
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
                    Personas = await response.Content.ReadFromJsonAsync<List<Persona>>();

                    if (Personas == null)
                    {
                        Personas = new List<Persona>();
                        ModelState.AddModelError(string.Empty, "No se encontraron personas disponibles.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar las personas: {response.StatusCode}");
                    Personas = new List<Persona>();
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible para cargar personas. Por favor, verifica que el backend esté en ejecución.");
                Personas = new List<Persona>();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar las personas: {ex.Message}");
                Personas = new List<Persona>();
            }
        }

        private class ResponseCreate
        {
            public string mensaje { get; set; }
            public Paciente paciente { get; set; }
        }

        private class ResponseError
        {
            public string mensaje { get; set; }
        }
    }
}