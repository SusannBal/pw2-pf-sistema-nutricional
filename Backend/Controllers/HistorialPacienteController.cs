using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistorialPacienteController : ControllerBase
    {

        private readonly Data.BackendContext BackendContext;

        public HistorialPacienteController(Data.BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/HistorialPaciente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HistorialPaciente>>> GetHistorialPaciente()
        {
            return await BackendContext.HistorialesPacientes
                .Include(h => h.Paciente)
                    .ThenInclude(p => p.Persona) // Incluir Persona del Paciente
                .ToListAsync();
        }

        // GET: api/HistorialPaciente/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HistorialPaciente>> GetHistorialPaciente(int id)
        {
            var historial = await BackendContext.HistorialesPacientes
                .Include(h => h.Paciente)
                    .ThenInclude(p => p.Persona)
                .FirstOrDefaultAsync(h => h.IdHistorial == id);

            if (historial == null)
            {
                return NotFound(new { mensaje = "Historial del paciente no encontrado" });
            }

            return historial;
        }

        // GET: api/HistorialPaciente/byPaciente/5
        [HttpGet("byPaciente/{pacienteId}")]
        public async Task<ActionResult<IEnumerable<HistorialPaciente>>> GetHistorialPacienteByPacienteId(int pacienteId)
        {
            var historiales = await BackendContext.HistorialesPacientes
                .Where(hp => hp.IdPaciente == pacienteId)
                .Include(hp => hp.Paciente)
                    .ThenInclude(p => p.Persona)
                .ToListAsync();

            if (!historiales.Any())
            {
                return NotFound(new { mensaje = "No se encontraron historiales de paciente para el paciente especificado." });
            }

            return historiales;
        }

        // POST: api/HistorialPaciente
        [HttpPost]
        public async Task<ActionResult<HistorialPaciente>> PostHistorialPaciente(HistorialPaciente historialPaciente)
        {
            var paciente = await BackendContext.Pacientes.FindAsync(historialPaciente.IdPaciente);
            if (paciente == null)
            {
                return BadRequest(new { mensaje = "El paciente especificado no existe" });
            }

            // Validar que la fecha no sea futura
            if (historialPaciente.Fecha > DateTime.Now)
            {
                return BadRequest(new { mensaje = "La fecha no puede ser futura" });
            }

            // Calcular IMC automáticamente si no se proporciona
            if (historialPaciente.IMC == 0 && historialPaciente.Peso > 0 && historialPaciente.Talla > 0)
            {
                historialPaciente.IMC = historialPaciente.Peso / (historialPaciente.Talla * historialPaciente.Talla);
            }

            BackendContext.HistorialesPacientes.Add(historialPaciente);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetHistorialPaciente", new { id = historialPaciente.IdHistorial },
                new
                {
                    mensaje = "Historial del paciente creado exitosamente",
                    historial = historialPaciente
                });
        }

        // PUT: api/HistorialPaciente/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHistorialPaciente(int id, HistorialPaciente historialPaciente)
        {
            if (id != historialPaciente.IdHistorial)
            {
                return BadRequest(new { mensaje = "El ID del historial del paciente no coincide" });
            }

            // Asegurarse de que el paciente asociado no cambie
            var existingHistorial = await BackendContext.HistorialesPacientes.AsNoTracking().FirstOrDefaultAsync(hp => hp.IdHistorial == id);
            if (existingHistorial == null)
            {
                return NotFound(new { mensaje = "Historial del paciente no encontrado" });
            }
            if (existingHistorial.IdPaciente != historialPaciente.IdPaciente)
            {
                return BadRequest(new { mensaje = "No se permite cambiar el paciente asociado a un historial de paciente existente." });
            }

            // Validar que la fecha no sea futura
            if (historialPaciente.Fecha > DateTime.Now)
            {
                return BadRequest(new { mensaje = "La fecha no puede ser futura" });
            }

            // Recalcular IMC si los valores de peso o talla cambian y el IMC no se proporciona
            if (historialPaciente.IMC == 0 && historialPaciente.Peso > 0 && historialPaciente.Talla > 0)
            {
                historialPaciente.IMC = historialPaciente.Peso / (historialPaciente.Talla * historialPaciente.Talla);
            }

            BackendContext.Entry(historialPaciente).State = EntityState.Modified;

            try
            {
                await BackendContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HistorialPacienteExists(id))
                {
                    return NotFound(new { mensaje = "Historial del paciente no encontrado" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Historial del paciente actualizado exitosamente" });
        }

        // DELETE: api/HistorialPaciente/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHistorialPaciente(int id)
        {
            var historial = await BackendContext.HistorialesPacientes.FindAsync(id);
            if (historial == null)
            {
                return NotFound(new { mensaje = "Historial del paciente no encontrado" });
            }

            BackendContext.HistorialesPacientes.Remove(historial);
            await BackendContext.SaveChangesAsync();

            return Ok(new { mensaje = "Historial del paciente eliminado exitosamente" });
        }

        private bool HistorialPacienteExists(int id)
        {
            return BackendContext.HistorialesPacientes.Any(e => e.IdHistorial == id);
        }
    }
}