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

namespace Frontend.Pages.HistorialesPacientes
{
    public class PacienteHistorialPacienteModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<HistorialPaciente> HistorialesPacientes { get; set; } = new();

        public PacienteHistorialPacienteModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<IActionResult> OnGetAsync(int? idPaciente)
        {
            if (HttpContext.Session.GetString("UserRole") != "Paciente" && idPaciente == null)
            {
                return RedirectToPage("/Login");
            }

            int targetPacienteId;
            if (idPaciente.HasValue)
            {
                // Si se proporciona un idPaciente en la URL (ej. desde el nutricionista)
                targetPacienteId = idPaciente.Value;
            }
            else
            {
                // Si el paciente está viendo su propio historial
                var loggedInPacienteId = HttpContext.Session.GetInt32("UserId");
                if (loggedInPacienteId == null)
                {
                    ModelState.AddModelError(string.Empty, "ID de paciente no encontrado en la sesión.");
                    return Page();
                }
                targetPacienteId = loggedInPacienteId.Value;
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/HistorialPaciente/byPaciente/{targetPacienteId}");
                if (response.IsSuccessStatusCode)
                {
                    HistorialesPacientes = await response.Content.ReadFromJsonAsync<List<HistorialPaciente>>() ?? new List<HistorialPaciente>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    HistorialesPacientes = new List<HistorialPaciente>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar el historial de paciente: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar el historial de paciente: {ex.Message}");
            }

            return Page();
        }
    }
}