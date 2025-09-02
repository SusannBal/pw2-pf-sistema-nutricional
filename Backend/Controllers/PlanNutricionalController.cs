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
    public class PlanNutricionalController : ControllerBase
    {
        private readonly BackendContext _context;

        public PlanNutricionalController(BackendContext context)
        {
            _context = context;
        }

        // GET: api/PlanNutricional
        // GET: api/PlanNutricional?idConsulta=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlanNutricional>>> GetPlanesNutricionales([FromQuery] int? idConsulta)
        {
            var query = _context.PlanesNutricionales.Include(p => p.Consulta).AsQueryable();

            if (idConsulta.HasValue)
            {
                query = query.Where(p => p.IdConsulta == idConsulta.Value);
            }

            return await query.ToListAsync();
        }

        // GET: api/PlanNutricional/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlanNutricional>> GetPlanNutricional(int id)
        {
            var plan = await _context.PlanesNutricionales
                .Include(p => p.Consulta)
                .FirstOrDefaultAsync(p => p.IdPlan == id);

            if (plan == null)
            {
                return NotFound(new { mensaje = "Plan nutricional no encontrado" });
            }

            return plan;
        }

        // POST: api/PlanNutricional
        [HttpPost]
        public async Task<ActionResult<PlanNutricional>> PostPlanNutricional(PlanNutricional planNutricional)
        {
            // Validar que la consulta existe
            var consulta = await _context.Consultas.FindAsync(planNutricional.IdConsulta);
            if (consulta == null)
            {
                return BadRequest(new { mensaje = "La consulta especificada no existe" });
            }

            // Validar nombre único por consulta
            var planExistente = await _context.PlanesNutricionales
                .FirstOrDefaultAsync(p => p.Nombre == planNutricional.Nombre && p.IdConsulta == planNutricional.IdConsulta);

            if (planExistente != null)
            {
                return BadRequest(new { mensaje = "Ya existe un plan con este nombre para la misma consulta" });
            }

            _context.PlanesNutricionales.Add(planNutricional);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlanNutricional", new { id = planNutricional.IdPlan },
                new
                {
                    mensaje = "Plan nutricional creado exitosamente",
                    plan = planNutricional
                });
        }

        // PUT: api/PlanNutricional/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlanNutricional(int id, PlanNutricional planNutricional)
        {
            if (id != planNutricional.IdPlan)
            {
                return BadRequest(new { mensaje = "El ID del plan nutricional no coincide" });
            }

            // Asegurarse de que la consulta asociada no cambie
            var existingPlan = await _context.PlanesNutricionales.AsNoTracking().FirstOrDefaultAsync(p => p.IdPlan == id);
            if (existingPlan == null)
            {
                return NotFound(new { mensaje = "Plan nutricional no encontrado" });
            }
            if (existingPlan.IdConsulta != planNutricional.IdConsulta)
            {
                return BadRequest(new { mensaje = "No se permite cambiar la consulta asociada a un plan nutricional existente." });
            }

            // Validar nombre único por consulta (si se cambia)
            var planExistente = await _context.PlanesNutricionales
                .FirstOrDefaultAsync(p => p.Nombre == planNutricional.Nombre && p.IdConsulta == planNutricional.IdConsulta && p.IdPlan != id);
            if (planExistente != null)
            {
                return BadRequest(new { mensaje = "Ya existe un plan con este nombre para la misma consulta" });
            }

            _context.Entry(planNutricional).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlanNutricionalExists(id))
                {
                    return NotFound(new { mensaje = "Plan nutricional no encontrado" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Plan nutricional actualizado exitosamente" });
        }

        // DELETE: api/PlanNutricional/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlanNutricional(int id)
        {
            var plan = await _context.PlanesNutricionales.FindAsync(id);
            if (plan == null)
            {
                return NotFound(new { mensaje = "Plan nutricional no encontrado" });
            }

            // Verificar si el plan tiene comidas asociadas
            var hasComidas = await _context.Comidas.AnyAsync(c => c.IdPlan == id);
            if (hasComidas)
            {
                return BadRequest(new { mensaje = "No se puede eliminar el plan nutricional porque tiene comidas asociadas." });
            }

            _context.PlanesNutricionales.Remove(plan);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Plan nutricional eliminado exitosamente" });
        }

        private bool PlanNutricionalExists(int id)
        {
            return _context.PlanesNutricionales.Any(e => e.IdPlan == id);
        }
    }
}