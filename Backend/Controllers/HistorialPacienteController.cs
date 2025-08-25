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
    public class HistorialPacienteController : ControllerBase
    {

        private readonly BackendContext BackendContext;

        public HistorialPacienteController(BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/HistorialPaciente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HistorialPaciente>>> GetHistorialPaciente()
        {
            return await BackendContext.HistorialesPacientes
                .Include(h => h.Paciente)
                .ToListAsync();
        }

        // GET: api/HistorialPaciente/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HistorialPaciente>> GetHistorialPaciente(int id)
        {
            var historial = await BackendContext.HistorialesPacientes
                .Include(h => h.Paciente)
                .FirstOrDefaultAsync(h => h.IdHistorial == id);

            if (historial == null)
            {
                return NotFound(new { mensaje = "Historial del paciente no encontrado" });
            }

            return historial;
        }

        // POST: api/HistorialPaciente
        [HttpPost]
        public async Task<ActionResult<HistorialPaciente>> PostHistorialPaciente(HistorialPaciente historialPaciente)
        {
            var paciente = await BackendContext.Pacientes.FindAsync(historialPaciente.IdPaciente);
            if (paciente == null)
            {
                return BadRequest(new { mensaje = "El paciente especificado no existe" });
            }

            // Validar que la fecha no sea futura
            if (historialPaciente.Fecha > DateTime.Now)
            {
                return BadRequest(new { mensaje = "La fecha no puede ser futura" });
            }

            // Calcular IMC automáticamente si no se proporciona
            if (historialPaciente.IMC == 0 && historialPaciente.Peso > 0 && historialPaciente.Talla > 0)
            {
                historialPaciente.IMC = historialPaciente.Peso / (historialPaciente.Talla * historialPaciente.Talla);
            }

            BackendContext.HistorialesPacientes.Add(historialPaciente);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetHistorialPaciente", new { id = historialPaciente.IdHistorial },
                new
                {
                    mensaje = "Historial del paciente creado exitosamente",
                    historial = historialPaciente
                });
        }


    }
}
