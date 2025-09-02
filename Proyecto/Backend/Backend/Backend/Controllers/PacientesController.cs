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
    }
}