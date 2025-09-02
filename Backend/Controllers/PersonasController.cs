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

        // GET: api/Personas/byci/{ci}
        [HttpGet("byci/{ci}")]
        public async Task<ActionResult<Persona>> GetPersonaByCI(string ci)
        {
            var persona = await BackendContext.Personas.FirstOrDefaultAsync(p => p.CI == ci);

            if (persona == null)
            {
                return NotFound(new { mensaje = "Persona no encontrada con el CI proporcionado" });
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

        // PUT: api/Personas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPersona(int id, Persona persona)
        {
            if (id != persona.IdPersona)
            {
                return BadRequest(new { mensaje = "El ID de la persona no coincide" });
            }

            // No permitir cambiar el CI
            var existingPersona = await BackendContext.Personas.AsNoTracking().FirstOrDefaultAsync(p => p.IdPersona == id);
            if (existingPersona == null)
            {
                return NotFound(new { mensaje = "Persona no encontrada" });
            }

            if (existingPersona.CI != persona.CI)
            {
                return BadRequest(new { mensaje = "No se permite cambiar el CI de una persona existente." });
            }

            // Validar email único (si se cambia)
            var emailExistente = await BackendContext.Personas
                .FirstOrDefaultAsync(p => p.Email == persona.Email && p.IdPersona != id);
            if (emailExistente != null)
            {
                return BadRequest(new { mensaje = "Ya existe una persona con este email" });
            }

            BackendContext.Entry(persona).State = EntityState.Modified;

            try
            {
                await BackendContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonaExists(id))
                {
                    return NotFound(new { mensaje = "Persona no encontrada" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Persona actualizada exitosamente" });
        }

        // DELETE: api/Personas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePersona(int id)
        {
            var persona = await BackendContext.Personas.FindAsync(id);
            if (persona == null)
            {
                return NotFound(new { mensaje = "Persona no encontrada" });
            }

            // Verificar si la persona está asociada a un paciente o nutricionista
            var isPaciente = await BackendContext.Pacientes.AnyAsync(p => p.IdPersona == id);
            var isNutricionista = await BackendContext.Nutricionistas.AnyAsync(n => n.IdPersona == id);
            var isRecordatorio = await BackendContext.Recordatorios.AnyAsync(r => r.IdPersona == id);

            if (isPaciente || isNutricionista || isRecordatorio)
            {
                return BadRequest(new { mensaje = "No se puede eliminar la persona porque está asociada a un paciente, nutricionista o recordatorio." });
            }

            BackendContext.Personas.Remove(persona);
            await BackendContext.SaveChangesAsync();

            return Ok(new { mensaje = "Persona eliminada exitosamente" });
        }

        private bool PersonaExists(int id)
        {
            return BackendContext.Personas.Any(e => e.IdPersona == id);
        }
    }
}