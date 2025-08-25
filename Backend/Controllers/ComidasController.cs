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
        private readonly BackendContext BackendContext;

        public ComidasController(BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/Comidas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comida>>> GetComidas()
        {
            return await BackendContext.Comidas
                .Include(c => c.PlanNutricional)
                .ToListAsync();
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


            if (comida.Hora.TotalDays >= 1)
            {
                return BadRequest(new { mensaje = "La hora debe estar en formato de tiempo válido" });
            }

            BackendContext.Comidas.Add(comida);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetComida", new { id = comida.IdComida },
                new
                {
                    mensaje = "Comida creada exitosamente",
                    comida = comida
                });
        }

    }
}
