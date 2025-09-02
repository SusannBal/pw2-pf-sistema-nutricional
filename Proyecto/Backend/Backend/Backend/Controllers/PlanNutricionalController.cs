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
        private readonly BackendContext BackendContext;

        public PlanNutricionalController(BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/PlanNutricional
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlanNutricional>>> GetPlanesNutricionales()
        {
            return await BackendContext.PlanesNutricionales
                .Include(p => p.Consulta)
                .ToListAsync();
        }

        // GET: api/PlanNutricional/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlanNutricional>> GetPlanNutricional(int id)
        {
            var plan = await BackendContext.PlanesNutricionales
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
            var consulta = await BackendContext.Consultas.FindAsync(planNutricional.IdConsulta);
            if (consulta == null)
            {
                return BadRequest(new { mensaje = "La consulta especificada no existe" });
            }

            // Validar nombre único por consulta
            var planExistente = await BackendContext.PlanesNutricionales
                .FirstOrDefaultAsync(p => p.Nombre == planNutricional.Nombre && p.IdConsulta == planNutricional.IdConsulta);

            if (planExistente != null)
            {
                return BadRequest(new { mensaje = "Ya existe un plan con este nombre para la misma consulta" });
            }

            BackendContext.PlanesNutricionales.Add(planNutricional);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetPlanNutricional", new { id = planNutricional.IdPlan },
                new
                {
                    mensaje = "Plan nutricional creado exitosamente",
                    plan = planNutricional
                });
        }
    }
}
