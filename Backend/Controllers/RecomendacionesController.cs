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
    public class RecomendacionesController : ControllerBase
    {
        private readonly Data.BackendContext BackendContext;

        public RecomendacionesController(Data.BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/Recomendaciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recomendacion>>> GetRecomendaciones()
        {
            return await BackendContext.Recomendaciones
                .Include(r => r.Consulta)
                .ToListAsync();
        }

        // GET: api/Recomendaciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Recomendacion>> GetRecomendacion(int id)
        {
            var recomendacion = await BackendContext.Recomendaciones
                .Include(r => r.Consulta)
                .FirstOrDefaultAsync(r => r.IdRecomendacion == id);

            if (recomendacion == null)
            {
                return NotFound(new { mensaje = "Recomendación no encontrada" });
            }

            return recomendacion;
        }

        // POST: api/Recomendaciones
        [HttpPost]
        public async Task<ActionResult<Recomendacion>> PostRecomendacion(Recomendacion recomendacion)
        {
            var consulta = await BackendContext.Consultas.FindAsync(recomendacion.IdConsulta);
            if (consulta == null)
            {
                return BadRequest(new { mensaje = "La consulta especificada no existe" });
            }
            BackendContext.Recomendaciones.Add(recomendacion);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetRecomendacion", new { id = recomendacion.IdRecomendacion },
                new
                {
                    mensaje = "Recomendación creada exitosamente",
                    recomendacion = recomendacion
                });
        }

        // PUT: api/Recomendaciones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecomendacion(int id, Recomendacion recomendacion)
        {
            if (id != recomendacion.IdRecomendacion)
            {
                return BadRequest(new { mensaje = "El ID de la recomendación no coincide" });
            }

            // Asegurarse de que la consulta asociada no cambie
            var existingRecomendacion = await BackendContext.Recomendaciones.AsNoTracking().FirstOrDefaultAsync(r => r.IdRecomendacion == id);
            if (existingRecomendacion == null)
            {
                return NotFound(new { mensaje = "Recomendación no encontrada" });
            }
            if (existingRecomendacion.IdConsulta != recomendacion.IdConsulta)
            {
                return BadRequest(new { mensaje = "No se permite cambiar la consulta asociada a una recomendación existente." });
            }

            BackendContext.Entry(recomendacion).State = EntityState.Modified;

            try
            {
                await BackendContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecomendacionExists(id))
                {
                    return NotFound(new { mensaje = "Recomendación no encontrada" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Recomendación actualizada exitosamente" });
        }

        // DELETE: api/Recomendaciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecomendacion(int id)
        {
            var recomendacion = await BackendContext.Recomendaciones.FindAsync(id);
            if (recomendacion == null)
            {
                return NotFound(new { mensaje = "Recomendación no encontrada" });
            }

            BackendContext.Recomendaciones.Remove(recomendacion);
            await BackendContext.SaveChangesAsync();

            return Ok(new { mensaje = "Recomendación eliminada exitosamente" });
        }

        private bool RecomendacionExists(int id)
        {
            return BackendContext.Recomendaciones.Any(e => e.IdRecomendacion == id);
        }
    }
}