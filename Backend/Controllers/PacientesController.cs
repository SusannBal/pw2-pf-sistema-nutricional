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
    public class PacientesController : ControllerBase
    {
        private readonly BackendContext BackendContext;

        public PacientesController(BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }
        // GET: api/Pacientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Paciente>>> GetPacientes()
        {
            return await BackendContext.Pacientes
                .Include(p => p.Persona)
                .ToListAsync();
        }

        // GET: api/Pacientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Paciente>> GetPaciente(int id)
        {
            var paciente = await BackendContext.Pacientes
                .Include(p => p.Persona)
                .FirstOrDefaultAsync(p => p.IdPaciente == id);

            if (paciente == null)
            {
                return NotFound(new { mensaje = "Paciente no encontrado" });
            }

            return paciente;
        }

        // GET: api/Pacientes/byci/{ci}
        [HttpGet("byci/{ci}")]
        public async Task<ActionResult<Paciente>> GetPacienteByCI(string ci)
        {
            var paciente = await BackendContext.Pacientes
                .Include(p => p.Persona)
                .FirstOrDefaultAsync(p => p.Persona.CI == ci);

            if (paciente == null)
            {
                return NotFound(new { mensaje = "Paciente no encontrado con el CI proporcionado" });
            }

            return paciente;
        }

        // POST: api/Pacientes
        [HttpPost]
        public async Task<ActionResult<Paciente>> PostPaciente(Paciente paciente)
        {
            var persona = await BackendContext.Personas.FindAsync(paciente.IdPersona);
            if (persona == null)
            {
                return BadRequest(new { mensaje = "La persona especificada no existe" });
            }

            // Validar que la persona no sea ya paciente
            var pacienteExistente = await BackendContext.Pacientes
                .FirstOrDefaultAsync(p => p.IdPersona == paciente.IdPersona);
            if (pacienteExistente != null)
            {
                return BadRequest(new { mensaje = "Esta persona ya está registrada como paciente" });
            }
            paciente.FechaRegistro = DateTime.Now;
            BackendContext.Pacientes.Add(paciente);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetPaciente", new { id = paciente.IdPaciente },
                new
                {
                    mensaje = "Paciente creado exitosamente",
                    paciente = paciente
                });
        }

        // PUT: api/Pacientes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaciente(int id, Paciente paciente)
        {
            if (id != paciente.IdPaciente)
            {
                return BadRequest(new { mensaje = "El ID del paciente no coincide" });
            }

            // Asegurarse de que la persona asociada no cambie
            var existingPaciente = await BackendContext.Pacientes.AsNoTracking().FirstOrDefaultAsync(p => p.IdPaciente == id);
            if (existingPaciente == null)
            {
                return NotFound(new { mensaje = "Paciente no encontrado" });
            }
            if (existingPaciente.IdPersona != paciente.IdPersona)
            {
                return BadRequest(new { mensaje = "No se permite cambiar la persona asociada a un paciente existente." });
            }

            BackendContext.Entry(paciente).State = EntityState.Modified;

            try
            {
                await BackendContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PacienteExists(id))
                {
                    return NotFound(new { mensaje = "Paciente no encontrado" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Paciente actualizado exitosamente" });
        }

        // DELETE: api/Pacientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaciente(int id)
        {
            var paciente = await BackendContext.Pacientes.FindAsync(id);
            if (paciente == null)
            {
                return NotFound(new { mensaje = "Paciente no encontrado" });
            }

            // Verificar si el paciente tiene consultas, historiales, etc. asociados
            var hasConsultas = await BackendContext.Consultas.AnyAsync(c => c.IdPaciente == id);
            var hasHistorialMedico = await BackendContext.HistorialesMedicos.AnyAsync(hm => hm.IdPaciente == id);
            var hasHistorialPaciente = await BackendContext.HistorialesPacientes.AnyAsync(hp => hp.IdPaciente == id);
            var hasRegistrosActividad = await BackendContext.RegistrosActividades.AnyAsync(ra => ra.IdPaciente == id);

            if (hasConsultas || hasHistorialMedico || hasHistorialPaciente || hasRegistrosActividad)
            {
                return BadRequest(new { mensaje = "No se puede eliminar el paciente porque tiene datos asociados (consultas, historiales, etc.)." });
            }

            BackendContext.Pacientes.Remove(paciente);
            await BackendContext.SaveChangesAsync();

            return Ok(new { mensaje = "Paciente eliminado exitosamente" });
        }

        private bool PacienteExists(int id)
        {
            return BackendContext.Pacientes.Any(e => e.IdPaciente == id);
        }
    }
}