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
    public class HistorialMedicoController : ControllerBase
    {
        private readonly Data.BackendContext BackendContext;
        public HistorialMedicoController(Data.BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }
        // GET: api/HistorialMedico
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HistorialMedico>>> GetHistorialMedico()
        {
            return await BackendContext.HistorialesMedicos
                .Include(h => h.Paciente)
                    .ThenInclude(p => p.Persona) // Incluir Persona del Paciente
                .ToListAsync();
        }

        // GET: api/HistorialMedico/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HistorialMedico>> GetHistorialMedico(int id)
        {
            var historial = await BackendContext.HistorialesMedicos
                .Include(h => h.Paciente)
                    .ThenInclude(p => p.Persona)
                .FirstOrDefaultAsync(h => h.IdHistorialMedico == id);

            if (historial == null)
            {
                return NotFound(new { mensaje = "Historial médico no encontrado" });
            }

            return historial;
        }

        // GET: api/HistorialMedico/byPaciente/5
        [HttpGet("byPaciente/{pacienteId}")]
        public async Task<ActionResult<IEnumerable<HistorialMedico>>> GetHistorialMedicoByPacienteId(int pacienteId)
        {
            var historiales = await BackendContext.HistorialesMedicos
                .Where(hm => hm.IdPaciente == pacienteId)
                .Include(hm => hm.Paciente)
                    .ThenInclude(p => p.Persona)
                .ToListAsync();

            if (!historiales.Any())
            {
                return NotFound(new { mensaje = "No se encontraron historiales médicos para el paciente especificado." });
            }

            return historiales;
        }

        // POST: api/HistorialMedico
        [HttpPost]
        public async Task<ActionResult<HistorialMedico>> PostHistorialMedico(HistorialMedico historialMedico)
        {
            var paciente = await BackendContext.Pacientes.FindAsync(historialMedico.IdPaciente);
            if (paciente == null)
            {
                return BadRequest(new { mensaje = "El paciente especificado no existe" });
            }

            BackendContext.HistorialesMedicos.Add(historialMedico);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetHistorialMedico", new { id = historialMedico.IdHistorialMedico },
                new
                {
                    mensaje = "Historial médico creado exitosamente",
                    historialMedico = historialMedico
                });
        }

        // PUT: api/HistorialMedico/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHistorialMedico(int id, HistorialMedico historialMedico)
        {
            if (id != historialMedico.IdHistorialMedico)
            {
                return BadRequest(new { mensaje = "El ID del historial médico no coincide" });
            }

            // Asegurarse de que el paciente asociado no cambie
            var existingHistorial = await BackendContext.HistorialesMedicos.AsNoTracking().FirstOrDefaultAsync(hm => hm.IdHistorialMedico == id);
            if (existingHistorial == null)
            {
                return NotFound(new { mensaje = "Historial médico no encontrado" });
            }
            if (existingHistorial.IdPaciente != historialMedico.IdPaciente)
            {
                return BadRequest(new { mensaje = "No se permite cambiar el paciente asociado a un historial médico existente." });
            }

            BackendContext.Entry(historialMedico).State = EntityState.Modified;

            try
            {
                await BackendContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HistorialMedicoExists(id))
                {
                    return NotFound(new { mensaje = "Historial médico no encontrado" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Historial médico actualizado exitosamente" });
        }

        // DELETE: api/HistorialMedico/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHistorialMedico(int id)
        {
            var historial = await BackendContext.HistorialesMedicos.FindAsync(id);
            if (historial == null)
            {
                return NotFound(new { mensaje = "Historial médico no encontrado" });
            }

            BackendContext.HistorialesMedicos.Remove(historial);
            await BackendContext.SaveChangesAsync();

            return Ok(new { mensaje = "Historial médico eliminado exitosamente" });
        }

        private bool HistorialMedicoExists(int id)
        {
            return BackendContext.HistorialesMedicos.Any(e => e.IdHistorialMedico == id);
        }
    }
}