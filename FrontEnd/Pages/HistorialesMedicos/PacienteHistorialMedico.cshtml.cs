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

namespace Frontend.Pages.HistorialesMedicos
{
    public class PacienteHistorialMedicoModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public List<HistorialMedico> HistorialesMedicos { get; set; } = new();

        public PacienteHistorialMedicoModel(IHttpClientFactory httpClientFactory)
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
                var response = await _httpClient.GetAsync($"api/HistorialMedico/byPaciente/{targetPacienteId}");
                if (response.IsSuccessStatusCode)
                {
                    HistorialesMedicos = await response.Content.ReadFromJsonAsync<List<HistorialMedico>>() ?? new List<HistorialMedico>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    HistorialesMedicos = new List<HistorialMedico>();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar el historial médico: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar el historial médico: {ex.Message}");
            }

            return Page();
        }
    }
}