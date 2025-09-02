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
        private readonly BackendContext BackendContext;

        public NutricionistasController(BackendContext BackendContext)
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

    }
}
