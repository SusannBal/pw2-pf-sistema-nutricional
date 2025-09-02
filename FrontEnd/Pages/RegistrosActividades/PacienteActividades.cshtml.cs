using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Frontend.Pages.RegistrosActividades
{
    public class PacienteActividadesModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Backend.Models.RegistroActividad> RegistrosActividad { get; set; } = new();
        public List<ActividadFisica> ActividadesFisicas { get; set; } = new();

        public PacienteActividadesModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("UserRole") != "Paciente")
            {
                return RedirectToPage("/Login");
            }

            var pacienteId = HttpContext.Session.GetInt32("UserId");
            if (pacienteId == null)
            {
                ModelState.AddModelError(string.Empty, "ID de paciente no encontrado en la sesión.");
                return Page();
            }

            try
            {
                // Cargar registros de actividad del paciente
                // Assuming your API has an endpoint to get activity records by IdPaciente
                var response = await _httpClient.GetAsync($"api/RegistroActividad/byPaciente/{pacienteId}");
                if (response.IsSuccessStatusCode)
                {
                    RegistrosActividad = await response.Content.ReadFromJsonAsync<List<Backend.Models.RegistroActividad>>() ?? new List<Backend.Models.RegistroActividad>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    RegistrosActividad = new List<Backend.Models.RegistroActividad>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar los registros de actividad: {response.StatusCode}");
                }

                // Cargar actividades físicas para relacionar
                var actividadesResponse = await _httpClient.GetAsync("api/ActividadFisica");
                if (actividadesResponse.IsSuccessStatusCode)
                {
                    ActividadesFisicas = await actividadesResponse.Content.ReadFromJsonAsync<List<ActividadFisica>>() ?? new List<ActividadFisica>();
                }

                // Relacionar manualmente los registros con actividades
                foreach (var registro in RegistrosActividad)
                {
                    registro.Actividad = ActividadesFisicas.FirstOrDefault(a => a.IdActividad == registro.IdActividad);
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar los registros de actividad: {ex.Message}");
            }

            return Page();
        }
    }
}