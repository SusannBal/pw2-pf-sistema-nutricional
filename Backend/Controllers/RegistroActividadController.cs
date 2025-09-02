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
    public class RegistroActividadController : ControllerBase
    {
        private readonly Data.BackendContext BackendContext;

        public RegistroActividadController(Data.BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/RegistroActividad
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistroActividad>>> GetRegistrosActividad()
        {
            return await BackendContext.RegistrosActividades
                .Include(r => r.Paciente)
                    .ThenInclude(p => p.Persona) // Incluir Persona del Paciente
                .Include(r => r.Actividad)
                .ToListAsync();
        }

        // GET: api/RegistroActividad/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RegistroActividad>> GetRegistroActividad(int id)
        {
            var registro = await BackendContext.RegistrosActividades
                .Include(r => r.Paciente)
                    .ThenInclude(p => p.Persona)
                .Include(r => r.Actividad)
                .FirstOrDefaultAsync(r => r.IdRegistro == id);

            if (registro == null)
            {
                return NotFound(new { mensaje = "Registro de actividad no encontrado" });
            }

            return registro;
        }

        // GET: api/RegistroActividad/byPaciente/5
        [HttpGet("byPaciente/{pacienteId}")]
        public async Task<ActionResult<IEnumerable<RegistroActividad>>> GetRegistrosActividadByPacienteId(int pacienteId)
        {
            var registros = await BackendContext.RegistrosActividades
                .Where(ra => ra.IdPaciente == pacienteId)
                .Include(ra => ra.Paciente)
                    .ThenInclude(p => p.Persona)
                .Include(ra => ra.Actividad)
                .ToListAsync();

            if (!registros.Any())
            {
                return NotFound(new { mensaje = "No se encontraron registros de actividad para el paciente especificado." });
            }

            return registros;
        }

        // POST: api/RegistroActividad
        [HttpPost]
        public async Task<ActionResult<RegistroActividad>> PostRegistroActividad(RegistroActividad registroActividad)
        {
            // Validar que el paciente existe
            var paciente = await BackendContext.Pacientes.FindAsync(registroActividad.IdPaciente);
            if (paciente == null)
            {
                return BadRequest(new { mensaje = "El paciente especificado no existe" });
            }

            // Validar que la actividad existe
            var actividad = await BackendContext.ActividadesFisicas.FindAsync(registroActividad.IdActividad);
            if (actividad == null)
            {
                return BadRequest(new { mensaje = "La actividad física especificada no existe" });
            }

            // Validar que la fecha no sea futura
            if (registroActividad.Fecha > DateTime.Now)
            {
                return BadRequest(new { mensaje = "La fecha no puede ser futura" });
            }

            // Validar duración mínima y máxima
            if (registroActividad.DuracionMin < 1 || registroActividad.DuracionMin > 1440)
            {
                return BadRequest(new { mensaje = "La duración debe estar entre 1 y 1440 minutos" });
            }

            BackendContext.RegistrosActividades.Add(registroActividad);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetRegistroActividad", new { id = registroActividad.IdRegistro },
                new
                {
                    mensaje = "Registro de actividad creado exitosamente",
                    registro = registroActividad
                });
        }

        // PUT: api/RegistroActividad/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegistroActividad(int id, RegistroActividad registroActividad)
        {
            if (id != registroActividad.IdRegistro)
            {
                return BadRequest(new { mensaje = "El ID del registro de actividad no coincide" });
            }

            // Asegurarse de que paciente y actividad no cambien
            var existingRegistro = await BackendContext.RegistrosActividades.AsNoTracking().FirstOrDefaultAsync(ra => ra.IdRegistro == id);
            if (existingRegistro == null)
            {
                return NotFound(new { mensaje = "Registro de actividad no encontrado" });
            }
            if (existingRegistro.IdPaciente != registroActividad.IdPaciente || existingRegistro.IdActividad != registroActividad.IdActividad)
            {
                return BadRequest(new { mensaje = "No se permite cambiar el paciente o la actividad asociada a un registro existente." });
            }

            // Validar que la fecha no sea futura
            if (registroActividad.Fecha > DateTime.Now)
            {
                return BadRequest(new { mensaje = "La fecha no puede ser futura" });
            }

            // Validar duración mínima y máxima
            if (registroActividad.DuracionMin < 1 || registroActividad.DuracionMin > 1440)
            {
                return BadRequest(new { mensaje = "La duración debe estar entre 1 y 1440 minutos" });
            }

            BackendContext.Entry(registroActividad).State = EntityState.Modified;

            try
            {
                await BackendContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RegistroActividadExists(id))
                {
                    return NotFound(new { mensaje = "Registro de actividad no encontrado" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Registro de actividad actualizado exitosamente" });
        }

        // DELETE: api/RegistroActividad/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegistroActividad(int id)
        {
            var registro = await BackendContext.RegistrosActividades.FindAsync(id);
            if (registro == null)
            {
                return NotFound(new { mensaje = "Registro de actividad no encontrado" });
            }

            BackendContext.RegistrosActividades.Remove(registro);
            await BackendContext.SaveChangesAsync();

            return Ok(new { mensaje = "Registro de actividad eliminado exitosamente" });
        }

        private bool RegistroActividadExists(int id)
        {
            return BackendContext.RegistrosActividades.Any(e => e.IdRegistro == id);
        }
    }
}