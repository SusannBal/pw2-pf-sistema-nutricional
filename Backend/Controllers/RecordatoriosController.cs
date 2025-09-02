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
    public class RecordatoriosController : ControllerBase
    {
        private readonly BackendContext _context;

        public RecordatoriosController(BackendContext context)
        {
            _context = context;
        }

        // GET: api/Recordatorios
        // GET: api/Recordatorios?idPersona=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recordatorio>>> GetRecordatorios([FromQuery] int? idPersona)
        {
            var query = _context.Recordatorios.Include(r => r.Persona).AsQueryable();

            if (idPersona.HasValue)
            {
                query = query.Where(r => r.IdPersona == idPersona.Value);
            }

            return await query.ToListAsync();
        }

        // GET: api/Recordatorios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Recordatorio>> GetRecordatorio(int id)
        {
            var recordatorio = await _context.Recordatorios
                .Include(r => r.Persona)
                .FirstOrDefaultAsync(r => r.IdRecordatorio == id);

            if (recordatorio == null)
            {
                return NotFound(new { mensaje = "Recordatorio no encontrado" });
            }

            return recordatorio;
        }

        // POST: api/Recordatorios
        [HttpPost]
        public async Task<ActionResult<Recordatorio>> PostRecordatorio(Recordatorio recordatorio)
        {
            var persona = await _context.Personas.FindAsync(recordatorio.IdPersona);
            if (persona == null)
            {
                return BadRequest(new { mensaje = "La persona especificada no existe" });
            }

            // Validar que la fecha/hora no sea en el pasado
            if (recordatorio.FechaHora < DateTime.Now)
            {
                return BadRequest(new { mensaje = "La fecha y hora del recordatorio no puede ser en el pasado" });
            }

            _context.Recordatorios.Add(recordatorio);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRecordatorio", new { id = recordatorio.IdRecordatorio },
                new
                {
                    mensaje = "Recordatorio creado exitosamente",
                    recordatorio = recordatorio
                });
        }

        // PUT: api/Recordatorios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecordatorio(int id, Recordatorio recordatorio)
        {
            if (id != recordatorio.IdRecordatorio)
            {
                return BadRequest(new { mensaje = "El ID del recordatorio no coincide" });
            }

            // Asegurarse de que la persona asociada no cambie
            var existingRecordatorio = await _context.Recordatorios.AsNoTracking().FirstOrDefaultAsync(r => r.IdRecordatorio == id);
            if (existingRecordatorio == null)
            {
                return NotFound(new { mensaje = "Recordatorio no encontrado" });
            }
            if (existingRecordatorio.IdPersona != recordatorio.IdPersona)
            {
                return BadRequest(new { mensaje = "No se permite cambiar la persona asociada a un recordatorio existente." });
            }

            // Validar que la fecha/hora no sea en el pasado
            if (recordatorio.FechaHora < DateTime.Now)
            {
                return BadRequest(new { mensaje = "La fecha y hora del recordatorio no puede ser en el pasado" });
            }

            _context.Entry(recordatorio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecordatorioExists(id))
                {
                    return NotFound(new { mensaje = "Recordatorio no encontrado" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Recordatorio actualizado exitosamente" });
        }

        // DELETE: api/Recordatorios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecordatorio(int id)
        {
            var recordatorio = await _context.Recordatorios.FindAsync(id);
            if (recordatorio == null)
            {
                return NotFound(new { mensaje = "Recordatorio no encontrado" });
            }

            _context.Recordatorios.Remove(recordatorio);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Recordatorio eliminado exitosamente" });
        }

        private bool RecordatorioExists(int id)
        {
            return _context.Recordatorios.Any(e => e.IdRecordatorio == id);
        }
    }
}