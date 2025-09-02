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
    public class HistorialMedicoController : ControllerBase
    {
        private readonly BackendContext BackendContext;
        public HistorialMedicoController(BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }
        // GET: api/HistorialMedico
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HistorialMedico>>> GetHistorialMedico()
        {
            return await BackendContext.HistorialesMedicos
                .Include(h => h.Paciente)
                .ToListAsync();
        }

        // GET: api/HistorialMedico/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HistorialMedico>> GetHistorialMedico(int id)
        {
            var historial = await BackendContext.HistorialesMedicos
                .Include(h => h.Paciente)
                .FirstOrDefaultAsync(h => h.IdHistorialMedico == id);

            if (historial == null)
            {
                return NotFound(new { mensaje = "Historial médico no encontrado" });
            }

            return historial;
        }

        // POST: api/HistorialMedico
        [HttpPost]
        public async Task<ActionResult<HistorialMedico>> PostHistorialMedico(HistorialMedico historialMedico)
        {
            var paciente = await BackendContext.Pacientes.FindAsync(historialMedico.IdPaciente);
            if (paciente == null)
            {
                return BadRequest(new { mensaje = "El paciente especificado no existe" });
            }

            BackendContext.HistorialesMedicos.Add(historialMedico);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetHistorialMedico", new { id = historialMedico.IdHistorialMedico },
                new
                {
                    mensaje = "Historial médico creado exitosamente",
                    historialMedico = historialMedico
                });
        }


    }
}
