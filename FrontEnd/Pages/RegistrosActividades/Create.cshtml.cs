using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Frontend.Pages.RegistrosActividades
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public Backend.Models.RegistroActividad RegistroActividad { get; set; } = new();

        public List<Paciente> Pacientes { get; set; } = new();
        public List<Persona> Personas { get; set; } = new();
        public List<ActividadFisica> ActividadesFisicas { get; set; } = new();

        [TempData]
        public string Mensaje { get; set; }

        public async Task OnGetAsync()
        {
            await LoadRelatedData();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRelatedData();
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/RegistroActividad", RegistroActividad);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ResponseCreate>();
                    Mensaje = result?.mensaje ?? "Registro de actividad creado exitosamente";
                    return RedirectToPage("Index");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al crear el registro de actividad");
                    await LoadRelatedData();
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API.");
                await LoadRelatedData();
                return Page();
            }
        }

        private async Task LoadRelatedData()
        {
            try
            {
                // Cargar pacientes
                var pacientesResponse = await _httpClient.GetAsync("api/Pacientes");
                if (pacientesResponse.IsSuccessStatusCode)
                {
                    Pacientes = await pacientesResponse.Content.ReadFromJsonAsync<List<Paciente>>() ?? new List<Paciente>();
                }

                // Cargar personas para relacionar
                var personasResponse = await _httpClient.GetAsync("api/Personas");
                if (personasResponse.IsSuccessStatusCode)
                {
                    Personas = await personasResponse.Content.ReadFromJsonAsync<List<Persona>>() ?? new List<Persona>();

                    // Relacionar pacientes con personas
                    foreach (var paciente in Pacientes)
                    {
                        paciente.Persona = Personas.FirstOrDefault(p => p.IdPersona == paciente.IdPersona);
                    }
                }

                // Cargar actividades físicas
                var actividadesResponse = await _httpClient.GetAsync("api/ActividadFisica");
                if (actividadesResponse.IsSuccessStatusCode)
                {
                    ActividadesFisicas = await actividadesResponse.Content.ReadFromJsonAsync<List<ActividadFisica>>() ?? new List<ActividadFisica>();
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible para cargar datos relacionados. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar datos relacionados: {ex.Message}");
            }
        }

        private class ResponseCreate
        {
            public string mensaje { get; set; }
            public Backend.Models.RegistroActividad registroActividad { get; set; }
        }

        private class ResponseError
        {
            public string mensaje { get; set; }
        }
    }
}