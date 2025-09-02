using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonasController : ControllerBase
    {
        private readonly BackendContext BackendContext;
        public PersonasController(BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }
        // GET: api/Personas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Persona>>> GetPersonas()
        {
            return await BackendContext.Personas.ToListAsync();
        }

        // GET: api/Personas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Persona>> GetPersona(int id)
        {
            var persona = await BackendContext.Personas.FindAsync(id);

            if (persona == null)
            {
                return NotFound(new { mensaje = "Persona no encontrada" });
            }

            return persona;
        }

        // POST: api/Personas
        [HttpPost]
        public async Task<ActionResult<Persona>> PostPersona(Persona persona)
        {
            // Validar CI único
            var personaExistente = await BackendContext.Personas
                .FirstOrDefaultAsync(p => p.CI == persona.CI);

            if (personaExistente != null)
            {
                return BadRequest(new { mensaje = "Ya existe una persona con este CI" });
            }

            // Validar email único
            var emailExistente = await BackendContext.Personas
                .FirstOrDefaultAsync(p => p.Email == persona.Email);

            if (emailExistente != null)
            {
                return BadRequest(new { mensaje = "Ya existe una persona con este email" });
            }

            BackendContext.Personas.Add(persona);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetPersona", new { id = persona.IdPersona },
                new
                {
                    mensaje = "Persona creada exitosamente",
                    persona = persona
                });
        }

        
    }
}