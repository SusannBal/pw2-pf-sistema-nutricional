using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

// ... (existing usings)
using Microsoft.AspNetCore.Mvc.Rendering; // Add this for SelectList

namespace Frontend.Pages.Comidas
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public Comida Comida { get; set; } = new();

        public List<PlanNutricional> PlanesNutricionales { get; set; } = new();
        public SelectList PlanesNutricionalesOptions { get; set; } // Added for dropdown

        [TempData]
        public string Mensaje { get; set; }

        public async Task OnGetAsync(int? idPlan) // Added idPlan
        {
            await LoadPlanesNutricionales();

            if (idPlan.HasValue)
            {
                Comida.IdPlan = idPlan.Value;
            }

            PlanesNutricionalesOptions = new SelectList(PlanesNutricionales, "IdPlan", "DisplayInfo", Comida.IdPlan);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadPlanesNutricionales();
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Comidas", Comida);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ResponseCreate>();
                    Mensaje = result?.mensaje ?? "Comida creada exitosamente";
                    return RedirectToPage("Index");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ResponseError>();
                    ModelState.AddModelError(string.Empty, error?.mensaje ?? "Error al crear la comida");
                    await LoadPlanesNutricionales();
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio API.");
                await LoadPlanesNutricionales();
                return Page();
            }
        }

        private async Task LoadPlanesNutricionales()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/PlanNutricional");
                if (response.IsSuccessStatusCode)
                {
                    PlanesNutricionales = await response.Content.ReadFromJsonAsync<List<PlanNutricional>>() ?? new List<PlanNutricional>();
                    // Assuming PlanNutricional model has a 'Nombre' property
                    foreach (var plan in PlanesNutricionales)
                    {
                        plan.DisplayInfo = $"Plan ID: {plan.IdPlan} - Nombre: {plan.Nombre}";
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Error al cargar planes nutricionales: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                ModelState.AddModelError(string.Empty, "El servicio API no está disponible para cargar planes nutricionales. Por favor, verifica que el backend esté en ejecución.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error inesperado al cargar planes nutricionales: {ex.Message}");
            }
        }

        private class ResponseCreate
        {
            public string mensaje { get; set; }
            public Comida comida { get; set; }
        }

        private class ResponseError
        {
            public string mensaje { get; set; }
        }
    }
}
