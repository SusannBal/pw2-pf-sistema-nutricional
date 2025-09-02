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
    public class ComidasController : ControllerBase
    {
        private readonly Data.BackendContext BackendContext;

        public ComidasController(Data.BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/Comidas?idPlan=5  (idPlan es opcional)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comida>>> GetComidas([FromQuery] int? idPlan)
        {
            var query = BackendContext.Comidas.Include(c => c.PlanNutricional).AsQueryable();

            if (idPlan.HasValue)
            {
                query = query.Where(c => c.IdPlan == idPlan.Value);
            }

            return await query.ToListAsync();
        }

        // POST: api/Comidas
        [HttpPost]
        public async Task<ActionResult<Comida>> PostComida(Comida comida)
        {
            // Validar que el plan nutricional existe
            var plan = await BackendContext.PlanesNutricionales.FindAsync(comida.IdPlan);
            if (plan == null)
            {
                return BadRequest(new { mensaje = "El plan nutricional especificado no existe" });
            }

            // Validar que la hora sea válida
            if (comida.Hora < TimeSpan.Zero || comida.Hora > TimeSpan.FromHours(24))
            {
                return BadRequest(new { mensaje = "La hora debe ser un valor de tiempo válido (entre 00:00 y 24:00)." });
            }

            BackendContext.Comidas.Add(comida);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComida), new { id = comida.IdComida },
                new
                {
                    mensaje = "Comida creada exitosamente",
                    comida = comida
                });
        }

        // GET: api/Comidas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Comida>> GetComida(int id)
        {
            var comida = await BackendContext.Comidas
                .Include(c => c.PlanNutricional)
                .FirstOrDefaultAsync(c => c.IdComida == id);

            if (comida == null)
            {
                return NotFound(new { mensaje = "Comida no encontrada" });
            }

            return comida;
        }

        // PUT: api/Comidas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComida(int id, Comida comida)
        {
            if (id != comida.IdComida)
            {
                return BadRequest(new { mensaje = "El ID de la comida no coincide" });
            }

            // Asegurarse de que el plan nutricional no cambie
            var existingComida = await BackendContext.Comidas.AsNoTracking().FirstOrDefaultAsync(c => c.IdComida == id);
            if (existingComida == null)
            {
                return NotFound(new { mensaje = "Comida no encontrada" });
            }
            if (existingComida.IdPlan != comida.IdPlan)
            {
                return BadRequest(new { mensaje = "No se permite cambiar el plan nutricional asociado a una comida existente." });
            }

            // Validar que la hora sea válida
            if (comida.Hora < TimeSpan.Zero || comida.Hora > TimeSpan.FromHours(24))
            {
                return BadRequest(new { mensaje = "La hora debe ser un valor de tiempo válido (entre 00:00 y 24:00)." });
            }

            BackendContext.Entry(comida).State = EntityState.Modified;

            try
            {
                await BackendContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComidaExists(id))
                {
                    return NotFound(new { mensaje = "Comida no encontrada" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Comida actualizada exitosamente" });
        }

        // DELETE: api/Comidas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComida(int id)
        {
            var comida = await BackendContext.Comidas.FindAsync(id);
            if (comida == null)
            {
                return NotFound(new { mensaje = "Comida no encontrada" });
            }

            BackendContext.Comidas.Remove(comida);
            await BackendContext.SaveChangesAsync();

            return Ok(new { mensaje = "Comida eliminada exitosamente" });
        }

        private bool ComidaExists(int id)
        {
            return BackendContext.Comidas.Any(e => e.IdComida == id);
        }
    }
}
