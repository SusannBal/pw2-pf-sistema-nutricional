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
    public class ConsultasController : ControllerBase
    {
        private readonly Data.BackendContext BackendContext;

        public ConsultasController(Data.BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }
        // GET: api/Consultas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Consulta>>> GetConsultas()
        {
            return await BackendContext.Consultas
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Persona) // Incluir Persona del Paciente
                .Include(c => c.Nutricionista)
                    .ThenInclude(n => n.Persona) // Incluir Persona del Nutricionista
                .ToListAsync();
        }

        // GET: api/Consultas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Consulta>> GetConsulta(int id)
        {
            var consulta = await BackendContext.Consultas
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Persona)
                .Include(c => c.Nutricionista)
                    .ThenInclude(n => n.Persona)
                .FirstOrDefaultAsync(c => c.IdConsulta == id);

            if (consulta == null)
            {
                return NotFound(new { mensaje = "Consulta no encontrada" });
            }

            return consulta;
        }

        // GET: api/Consultas/byNutricionista/5
        [HttpGet("byNutricionista/{nutricionistaId}")]
        public async Task<ActionResult<IEnumerable<Consulta>>> GetConsultasByNutricionistaId(int nutricionistaId)
        {
            var consultas = await BackendContext.Consultas
                .Where(c => c.IdNutricionista == nutricionistaId)
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Persona)
                .Include(c => c.Nutricionista)
                    .ThenInclude(n => n.Persona)
                .ToListAsync();

            if (!consultas.Any())
            {
                return NotFound(new { mensaje = "No se encontraron consultas para el nutricionista especificado." });
            }

            return consultas;
        }

        // GET: api/Consultas/byPaciente/5
        [HttpGet("byPaciente/{pacienteId}")]
        public async Task<ActionResult<IEnumerable<Consulta>>> GetConsultasByPacienteId(int pacienteId)
        {
            var consultas = await BackendContext.Consultas
                .Where(c => c.IdPaciente == pacienteId)
                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Persona)
                .Include(c => c.Nutricionista)
                    .ThenInclude(n => n.Persona)
                .ToListAsync();

            if (!consultas.Any())
            {
                return NotFound(new { mensaje = "No se encontraron consultas para el paciente especificado." });
            }

            return consultas;
        }

        // POST: api/Consultas
        [HttpPost]
        public async Task<ActionResult<Consulta>> PostConsulta(Consulta consulta)
        {
            // Validar que el paciente existe
            var paciente = await BackendContext.Pacientes.FindAsync(consulta.IdPaciente);
            if (paciente == null)
            {
                return BadRequest(new { mensaje = "El paciente especificado no existe" });
            }

            // Validar que el nutricionista existe
            var nutricionista = await BackendContext.Nutricionistas.FindAsync(consulta.IdNutricionista);
            if (nutricionista == null)
            {
                return BadRequest(new { mensaje = "El nutricionista especificado no existe" });
            }

            consulta.Estado = consulta.Estado ?? "Programada";
            BackendContext.Consultas.Add(consulta);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetConsulta", new { id = consulta.IdConsulta },
                new
                {
                    mensaje = "Consulta creada exitosamente",
                    consulta = consulta
                });
        }

        // PUT: api/Consultas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConsulta(int id, Consulta consulta)
        {
            if (id != consulta.IdConsulta)
            {
                return BadRequest(new { mensaje = "El ID de la consulta no coincide" });
            }

            // Asegurarse de que paciente y nutricionista no cambien
            var existingConsulta = await BackendContext.Consultas.AsNoTracking().FirstOrDefaultAsync(c => c.IdConsulta == id);
            if (existingConsulta == null)
            {
                return NotFound(new { mensaje = "Consulta no encontrada" });
            }
            if (existingConsulta.IdPaciente != consulta.IdPaciente || existingConsulta.IdNutricionista != consulta.IdNutricionista)
            {
                return BadRequest(new { mensaje = "No se permite cambiar el paciente o nutricionista de una consulta existente." });
            }

            BackendContext.Entry(consulta).State = EntityState.Modified;

            try
            {
                await BackendContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsultaExists(id))
                {
                    return NotFound(new { mensaje = "Consulta no encontrada" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Consulta actualizada exitosamente" });
        }

        // DELETE: api/Consultas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsulta(int id)
        {
            var consulta = await BackendContext.Consultas.FindAsync(id);
            if (consulta == null)
            {
                return NotFound(new { mensaje = "Consulta no encontrada" });
            }

            // Verificar si la consulta tiene diagnósticos, planes nutricionales o recomendaciones asociados
            var hasDiagnosticos = await BackendContext.Diagnosticos.AnyAsync(d => d.IdConsulta == id);
            var hasPlanesNutricionales = await BackendContext.PlanesNutricionales.AnyAsync(pn => pn.IdConsulta == id);
            var hasRecomendaciones = await BackendContext.Recomendaciones.AnyAsync(r => r.IdConsulta == id);

            if (hasDiagnosticos || hasPlanesNutricionales || hasRecomendaciones)
            {
                return BadRequest(new { mensaje = "No se puede eliminar la consulta porque tiene diagnósticos, planes nutricionales o recomendaciones asociados." });
            }

            BackendContext.Consultas.Remove(consulta);
            await BackendContext.SaveChangesAsync();

            return Ok(new { mensaje = "Consulta eliminada exitosamente" });
        }

        private bool ConsultaExists(int id)
        {
            return BackendContext.Consultas.Any(e => e.IdConsulta == id);
        }
    }
}