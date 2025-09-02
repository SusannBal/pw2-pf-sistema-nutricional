using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadisticasController : ControllerBase
    {
        private readonly Data.BackendContext BackendContext;

        public EstadisticasController(Data.BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/Estadisticas/paciente/5
        [HttpGet("paciente/{idPaciente}")]
        public async Task<ActionResult<object>> GetEstadisticasPaciente(int idPaciente)
        {
            var paciente = await BackendContext.Pacientes
                .Include(p => p.Persona)
                .FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);

            if (paciente == null)
            {
                return NotFound(new { mensaje = "Paciente no encontrado" });
            }

            var consultasCount = await BackendContext.Consultas
                .CountAsync(c => c.IdPaciente == idPaciente);

            var ultimoHistorial = await BackendContext.HistorialesPacientes
                .Where(h => h.IdPaciente == idPaciente)
                .OrderByDescending(h => h.Fecha)
                .FirstOrDefaultAsync();

            var actividadesCount = await BackendContext.RegistrosActividades
                .CountAsync(r => r.IdPaciente == idPaciente);

            return new
            {
                paciente = new
                {
                    paciente.IdPaciente,
                    paciente.Persona.Nombre,
                    paciente.Persona.ApellidoPaterno,
                    paciente.Objetivo,
                    paciente.PesoInicial,
                    paciente.TallaInicial
                },
                estadisticas = new
                {
                    totalConsultas = consultasCount,
                    totalActividades = actividadesCount,
                    ultimoPeso = ultimoHistorial?.Peso,
                    ultimoIMC = ultimoHistorial?.IMC,
                    ultimaFechaMedicion = ultimoHistorial?.Fecha
                }
            };
        }
    }
}