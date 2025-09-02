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
    public class DiagnosticosController : ControllerBase
    {
        private readonly BackendContext BackendContext;

        public DiagnosticosController(BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/Diagnosticos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Diagnostico>>> GetDiagnosticos()
        {
            return await BackendContext.Diagnosticos
                .Include(d => d.Consulta)
                    .ThenInclude(c => c.Paciente)
                        .ThenInclude(p => p.Persona) // Incluir Persona del Paciente
                .ToListAsync();
        }

        // GET: api/Diagnosticos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Diagnostico>> GetDiagnostico(int id)
        {
            var diagnostico = await BackendContext.Diagnosticos
                .Include(d => d.Consulta)
                    .ThenInclude(c => c.Paciente)
                        .ThenInclude(p => p.Persona)
                .FirstOrDefaultAsync(d => d.IdDiagnostico == id);

            if (diagnostico == null)
            {
                return NotFound(new { mensaje = "Diagnóstico no encontrado" });
            }

            return diagnostico;
        }

        // POST: api/Diagnosticos
        [HttpPost]
        public async Task<ActionResult<Diagnostico>> PostDiagnostico(Diagnostico diagnostico)
        {
            // Validar que la consulta existe
            var consulta = await BackendContext.Consultas.FindAsync(diagnostico.IdConsulta);
            if (consulta == null)
            {
                return BadRequest(new { mensaje = "La consulta especificada no existe" });
            }
            var diagnosticoExistente = await BackendContext.Diagnosticos
                .FirstOrDefaultAsync(d => d.IdConsulta == diagnostico.IdConsulta);

            if (diagnosticoExistente != null)
            {
                return BadRequest(new { mensaje = "Ya existe un diagnóstico para esta consulta" });
            }

            BackendContext.Diagnosticos.Add(diagnostico);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetDiagnostico", new { id = diagnostico.IdDiagnostico },
                new
                {
                    mensaje = "Diagnóstico creado exitosamente",
                    diagnostico = diagnostico
                });
        }

        // PUT: api/Diagnosticos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDiagnostico(int id, Diagnostico diagnostico)
        {
            if (id != diagnostico.IdDiagnostico)
            {
                return BadRequest(new { mensaje = "El ID del diagnóstico no coincide" });
            }

            // Asegurarse de que la consulta asociada no cambie
            var existingDiagnostico = await BackendContext.Diagnosticos.AsNoTracking().FirstOrDefaultAsync(d => d.IdDiagnostico == id);
            if (existingDiagnostico == null)
            {
                return NotFound(new { mensaje = "Diagnóstico no encontrado" });
            }
            if (existingDiagnostico.IdConsulta != diagnostico.IdConsulta)
            {
                return BadRequest(new { mensaje = "No se permite cambiar la consulta asociada a un diagnóstico existente." });
            }

            BackendContext.Entry(diagnostico).State = EntityState.Modified;

            try
            {
                await BackendContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DiagnosticoExists(id))
                {
                    return NotFound(new { mensaje = "Diagnóstico no encontrado" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Diagnóstico actualizado exitosamente" });
        }

        // DELETE: api/Diagnosticos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiagnostico(int id)
        {
            var diagnostico = await BackendContext.Diagnosticos.FindAsync(id);
            if (diagnostico == null)
            {
                return NotFound(new { mensaje = "Diagnóstico no encontrado" });
            }

            BackendContext.Diagnosticos.Remove(diagnostico);
            await BackendContext.SaveChangesAsync();

            return Ok(new { mensaje = "Diagnóstico eliminado exitosamente" });
        }

        private bool DiagnosticoExists(int id)
        {
            return BackendContext.Diagnosticos.Any(e => e.IdDiagnostico == id);
        }
    }
}