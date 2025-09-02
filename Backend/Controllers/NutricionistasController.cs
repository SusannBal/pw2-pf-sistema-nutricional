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
    public class NutricionistasController : ControllerBase
    {
        private readonly Data.BackendContext BackendContext;

        public NutricionistasController(Data.BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }
        // GET: api/Nutricionistas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Nutricionista>>> GetNutricionistas()
        {
            return await BackendContext.Nutricionistas
                .Include(n => n.Persona)
                .ToListAsync();
        }

        // GET: api/Nutricionistas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Nutricionista>> GetNutricionista(int id)
        {
            var nutricionista = await BackendContext.Nutricionistas
                .Include(n => n.Persona)
                .FirstOrDefaultAsync(n => n.IdNutricionista == id);

            if (nutricionista == null)
            {
                return NotFound(new { mensaje = "Nutricionista no encontrado" });
            }

            return nutricionista;
        }

        // GET: api/Nutricionistas/byci/{ci}
        [HttpGet("byci/{ci}")]
        public async Task<ActionResult<Nutricionista>> GetNutricionistaByCI(string ci)
        {
            var nutricionista = await BackendContext.Nutricionistas
                .Include(n => n.Persona)
                .FirstOrDefaultAsync(n => n.Persona.CI == ci);

            if (nutricionista == null)
            {
                return NotFound(new { mensaje = "Nutricionista no encontrado con el CI proporcionado" });
            }

            return nutricionista;
        }

        // POST: api/Nutricionistas
        [HttpPost]
        public async Task<ActionResult<Nutricionista>> PostNutricionista(Nutricionista nutricionista)
        {
            var persona = await BackendContext.Personas.FindAsync(nutricionista.IdPersona);
            if (persona == null)
            {
                return BadRequest(new { mensaje = "La persona especificada no existe" });
            }

            // Validar que la persona no sea ya nutricionista
            var nutricionistaExistente = await BackendContext.Nutricionistas
                .FirstOrDefaultAsync(n => n.IdPersona == nutricionista.IdPersona);

            if (nutricionistaExistente != null)
            {
                return BadRequest(new { mensaje = "Esta persona ya está registrada como nutricionista" });
            }

            // Validar matrícula única
            var matriculaExistente = await BackendContext.Nutricionistas
                .FirstOrDefaultAsync(n => n.Matricula == nutricionista.Matricula);

            if (matriculaExistente != null)
            {
                return BadRequest(new { mensaje = "Ya existe un nutricionista con esta matrícula" });
            }

            BackendContext.Nutricionistas.Add(nutricionista);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetNutricionista", new { id = nutricionista.IdNutricionista },
                new
                {
                    mensaje = "Nutricionista creado exitosamente",
                    nutricionista = nutricionista
                });
        }

        // PUT: api/Nutricionistas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNutricionista(int id, Nutricionista nutricionista)
        {
            if (id != nutricionista.IdNutricionista)
            {
                return BadRequest(new { mensaje = "El ID del nutricionista no coincide" });
            }

            // Asegurarse de que la persona asociada no cambie
            var existingNutricionista = await BackendContext.Nutricionistas.AsNoTracking().FirstOrDefaultAsync(n => n.IdNutricionista == id);
            if (existingNutricionista == null)
            {
                return NotFound(new { mensaje = "Nutricionista no encontrado" });
            }
            if (existingNutricionista.IdPersona != nutricionista.IdPersona)
            {
                return BadRequest(new { mensaje = "No se permite cambiar la persona asociada a un nutricionista existente." });
            }

            // Validar matrícula única (si se cambia)
            var matriculaExistente = await BackendContext.Nutricionistas
                .FirstOrDefaultAsync(n => n.Matricula == nutricionista.Matricula && n.IdNutricionista != id);
            if (matriculaExistente != null)
            {
                return BadRequest(new { mensaje = "Ya existe un nutricionista con esta matrícula" });
            }

            BackendContext.Entry(nutricionista).State = EntityState.Modified;

            try
            {
                await BackendContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NutricionistaExists(id))
                {
                    return NotFound(new { mensaje = "Nutricionista no encontrado" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Nutricionista actualizado exitosamente" });
        }

        // DELETE: api/Nutricionistas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNutricionista(int id)
        {
            var nutricionista = await BackendContext.Nutricionistas.FindAsync(id);
            if (nutricionista == null)
            {
                return NotFound(new { mensaje = "Nutricionista no encontrado" });
            }

            // Verificar si el nutricionista tiene consultas asociadas
            var hasConsultas = await BackendContext.Consultas.AnyAsync(c => c.IdNutricionista == id);
            if (hasConsultas)
            {
                return BadRequest(new { mensaje = "No se puede eliminar el nutricionista porque tiene consultas asociadas." });
            }

            BackendContext.Nutricionistas.Remove(nutricionista);
            await BackendContext.SaveChangesAsync();

            return Ok(new { mensaje = "Nutricionista eliminado exitosamente" });
        }

        private bool NutricionistaExists(int id)
        {
            return BackendContext.Nutricionistas.Any(e => e.IdNutricionista == id);
        }
    }
}