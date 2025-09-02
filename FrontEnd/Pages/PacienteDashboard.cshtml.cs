using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Frontend.Pages
{
    public class PacienteDashboardModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;

        public PacienteDashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _httpClientFactory = httpClientFactory;
        }

        public string UserName { get; set; }
        public int UserId { get; set; }
        public int PersonaId { get; set; }

        public List<Comida> ProximasComidas { get; set; } = new();
        public List<Recordatorio> ProximosRecordatorios { get; set; } = new();
        public List<RegistroActividad> ProximasActividades { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar si el usuario está logueado y es paciente
            if (HttpContext.Session.GetString("UserRole") != "Paciente")
            {
                return RedirectToPage("/Login");
            }

            UserName = HttpContext.Session.GetString("UserName");
            UserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (UserId == 0)
            {
                ModelState.AddModelError(string.Empty, "ID de paciente no válido.");
                return Page();
            }

            try
            {
                // Obtener el IdPersona del paciente
                var pacienteResponse = await _httpClient.GetAsync($"api/Pacientes/{UserId}");
                if (pacienteResponse.IsSuccessStatusCode)
                {
                    var paciente = await pacienteResponse.Content.ReadFromJsonAsync<Paciente>();
                    PersonaId = paciente?.IdPersona ?? 0;
                }

                // Cargar datos para el dashboard
                await CargarProximasComidas();
                await CargarProximosRecordatorios();
                await CargarProximasActividades();
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.Message}");
            }

            return Page();
        }

        private async Task CargarProximasComidas()
        {
            try
            {
                // Obtener todas las comidas del paciente
                var comidasResponse = await _httpClient.GetAsync($"api/Comidas/byPaciente/{UserId}");
                if (comidasResponse.IsSuccessStatusCode)
                {
                    var todasComidas = await comidasResponse.Content.ReadFromJsonAsync<List<Comida>>() ?? new List<Comida>();

                    // Para este ejemplo, mostramos las próximas 5 comidas (sin filtro de fecha específico)
                    ProximasComidas = todasComidas
                        .OrderBy(c => c.Hora)
                        .Take(5)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar comidas: {ex.Message}");
            }
        }

        private async Task CargarProximosRecordatorios()
        {
            try
            {
                // Obtener recordatorios del paciente
                var recordatoriosResponse = await _httpClient.GetAsync($"api/Recordatorios/byPaciente/{UserId}");
                if (recordatoriosResponse.IsSuccessStatusCode)
                {
                    var todosRecordatorios = await recordatoriosResponse.Content.ReadFromJsonAsync<List<Recordatorio>>() ?? new List<Recordatorio>();

                    ProximosRecordatorios = todosRecordatorios
                        .Where(r => r.FechaHora >= DateTime.Now)
                        .OrderBy(r => r.FechaHora)
                        .Take(3)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar recordatorios: {ex.Message}");
            }
        }

        private async Task CargarProximasActividades()
        {
            try
            {
                // Obtener actividades del paciente
                var actividadesResponse = await _httpClient.GetAsync($"api/RegistrosActividades/byPaciente/{UserId}");
                if (actividadesResponse.IsSuccessStatusCode)
                {
                    var todasActividades = await actividadesResponse.Content.ReadFromJsonAsync<List<RegistroActividad>>() ?? new List<RegistroActividad>();

                    ProximasActividades = todasActividades
                        .Where(ra => ra.Fecha >= DateTime.Today)
                        .OrderBy(ra => ra.Fecha)
                        .Take(3)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar actividades: {ex.Message}");
            }
        }
    }
}