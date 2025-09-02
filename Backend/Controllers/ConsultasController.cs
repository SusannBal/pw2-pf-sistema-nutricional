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
    public class ConsultasController : ControllerBase
    {
        private readonly BackendContext BackendContext;

        public ConsultasController(BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }
        // GET: api/Consultas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Consulta>>> GetConsultas()
        {
            return await BackendContext.Consultas
                .Include(c => c.Paciente)
                .Include(c => c.Nutricionista)
                .ToListAsync();
        }

        // GET: api/Consultas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Consulta>> GetConsulta(int id)
        {
            var consulta = await BackendContext.Consultas
                .Include(c => c.Paciente)
                .Include(c => c.Nutricionista)
                .FirstOrDefaultAsync(c => c.IdConsulta == id);

            if (consulta == null)
            {
                return NotFound(new { mensaje = "Consulta no encontrada" });
            }

            return consulta;
        }

        // POST: api/Consultas
        [HttpPost]
        public async Task<ActionResult<Consulta>> PostConsulta(Consulta consulta)
        {
            // Validar que el paciente existe
            var paciente = await BackendContext.Pacientes.FindAsync(consulta.IdPaciente);
            if (paciente == null)
            {
                return BadRequest(new { mensaje = "El paciente especificado no existe" });
            }

            // Validar que el nutricionista existe
            var nutricionista = await BackendContext.Nutricionistas.FindAsync(consulta.IdNutricionista);
            if (nutricionista == null)
            {
                return BadRequest(new { mensaje = "El nutricionista especificado no existe" });
            }

            consulta.Estado = consulta.Estado ?? "Programada";
            BackendContext.Consultas.Add(consulta);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetConsulta", new { id = consulta.IdConsulta },
                new
                {
                    mensaje = "Consulta creada exitosamente",
                    consulta = consulta
                });
        }


    }
}
