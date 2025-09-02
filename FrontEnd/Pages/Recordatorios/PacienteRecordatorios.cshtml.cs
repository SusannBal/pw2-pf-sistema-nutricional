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

namespace Frontend.Pages.Recordatorios
{
    public class PacienteRecordatoriosModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<Recordatorio> Recordatorios { get; set; } = new();
        public int PersonaId { get; set; } // Para obtener los recordatorios de la persona asociada al paciente

        public PacienteRecordatoriosModel(IHttpClientFactory httpClientFactory)
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
                // Primero, obtener el IdPersona del paciente logueado
                var pacienteResponse = await _httpClient.GetAsync($"api/Pacientes/{pacienteId}");
                if (pacienteResponse.IsSuccessStatusCode)
                {
                    var paciente = await pacienteResponse.Content.ReadFromJsonAsync<Paciente>();
                    PersonaId = paciente.IdPersona;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar la información del paciente: {pacienteResponse.StatusCode}");
                    return Page();
                }

                // Luego, obtener los recordatorios para esa PersonaId
                var recordatoriosResponse = await _httpClient.GetAsync($"api/Recordatorios?idPersona={PersonaId}"); // Asumiendo que el API puede filtrar por IdPersona
                if (recordatoriosResponse.IsSuccessStatusCode)
                {
                    Recordatorios = await recordatoriosResponse.Content.ReadFromJsonAsync<List<Recordatorio>>() ?? new List<Recordatorio>();
                }
                else if (recordatoriosResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Recordatorios = new List<Recordatorio>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar los recordatorios: {recordatoriosResponse.StatusCode}");
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar los recordatorios: {ex.Message}");
            }

            return Page();
        }
    }
}