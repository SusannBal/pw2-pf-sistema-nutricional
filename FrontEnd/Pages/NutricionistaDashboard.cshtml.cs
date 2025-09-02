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
    public class NutricionistaDashboardModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public NutricionistaDashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public string UserName { get; set; }
        public int UserId { get; set; }

        public List<Consulta> ProximasConsultas { get; set; } = new();
        public List<Recordatorio> ProximosRecordatorios { get; set; } = new();
        public List<Consulta> ConsultasParaCalendario { get; set; } = new(); // For FullCalendar

        public async Task<IActionResult> OnGet()
        {
            // Verificar si el usuario está logueado y es nutricionista
            if (HttpContext.Session.GetString("UserRole") != "Nutricionista")
            {
                return RedirectToPage("/Login"); // Redirigir si no es nutricionista
            }

            UserName = HttpContext.Session.GetString("UserName");
            UserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            try
            {
                // --- Cargar datos para el panel de "Tu Agenda y Tareas" ---

                // 1. Próximas Consultas (del nutricionista logueado)
                var consultasResponse = await _httpClient.GetAsync($"api/Consultas/byNutricionista/{UserId}");
                if (consultasResponse.IsSuccessStatusCode)
                {
                    var allConsultas = await consultasResponse.Content.ReadFromJsonAsync<List<Consulta>>() ?? new List<Consulta>();

                    // Load related data for consultations (Patients and Personas)
                    var pacientesResponse = await _httpClient.GetAsync("api/Pacientes");
                    var personasResponse = await _httpClient.GetAsync("api/Personas");

                    var allPacientes = await pacientesResponse.Content.ReadFromJsonAsync<List<Paciente>>() ?? new List<Paciente>();
                    var allPersonas = await personasResponse.Content.ReadFromJsonAsync<List<Persona>>() ?? new List<Persona>();

                    foreach (var consulta in allConsultas)
                    {
                        consulta.Paciente = allPacientes.FirstOrDefault(p => p.IdPaciente == consulta.IdPaciente);
                        if (consulta.Paciente != null)
                        {
                            consulta.Paciente.Persona = allPersonas.FirstOrDefault(p => p.IdPersona == consulta.Paciente.IdPersona);
                        }
                    }

                    ProximasConsultas = allConsultas
                        .Where(c => c.Fecha >= DateTime.Now && c.Fecha <= DateTime.Now.AddDays(7)) // Next 7 days
                        .OrderBy(c => c.Fecha)
                        .Take(5) // Limit to a few upcoming consultations
                        .ToList();

                    ConsultasParaCalendario = allConsultas; // All consultations for the calendar
                }
                else if (consultasResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar las consultas: {consultasResponse.StatusCode}");
                }

                // 2. Próximos Recordatorios (creados por este nutricionista o para sus pacientes)
                // This assumes your API can filter reminders by the creator (nutricionista) or by associated patients.
                // For simplicity, let's fetch all reminders and filter them by associated persons.
                var recordatoriosResponse = await _httpClient.GetAsync("api/Recordatorios");
                if (recordatoriosResponse.IsSuccessStatusCode)
                {
                    var allRecordatorios = await recordatoriosResponse.Content.ReadFromJsonAsync<List<Recordatorio>>() ?? new List<Recordatorio>();

                    // Load all persons to link with reminders
                    var allPersonas = await _httpClient.GetAsync("api/Personas");
                    var personasList = await allPersonas.Content.ReadFromJsonAsync<List<Persona>>() ?? new List<Persona>();

                    foreach (var recordatorio in allRecordatorios)
                    {
                        recordatorio.Persona = personasList.FirstOrDefault(p => p.IdPersona == recordatorio.IdPersona);
                    }

                    ProximosRecordatorios = allRecordatorios
                        .Where(r => r.FechaHora >= DateTime.Now && r.FechaHora <= DateTime.Now.AddDays(7))
                        .OrderBy(r => r.FechaHora)
                        .Take(5)
                        .ToList();
                }
                else if (recordatoriosResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar los recordatorios: {recordatoriosResponse.StatusCode}");
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API para cargar la información del dashboard.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar la información del dashboard: {ex.Message}");
            }

            return Page();
        }
    }
}