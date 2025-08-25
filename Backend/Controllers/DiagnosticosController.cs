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
    public class DiagnosticosController : ControllerBase
    {
        private readonly BackendContext BackendContext;

        public DiagnosticosController(BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/Diagnosticos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Diagnostico>>> GetDiagnosticos()
        {
            return await BackendContext.Diagnosticos
                .Include(d => d.Consulta)
                .ToListAsync();
        }

        // GET: api/Diagnosticos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Diagnostico>> GetDiagnostico(int id)
        {
            var diagnostico = await BackendContext.Diagnosticos
                .Include(d => d.Consulta)
                .FirstOrDefaultAsync(d => d.IdDiagnostico == id);

            if (diagnostico == null)
            {
                return NotFound(new { mensaje = "Diagnóstico no encontrado" });
            }

            return diagnostico;
        }

        // POST: api/Diagnosticos
        [HttpPost]
        public async Task<ActionResult<Diagnostico>> PostDiagnostico(Diagnostico diagnostico)
        {
            // Validar que la consulta existe
            var consulta = await BackendContext.Consultas.FindAsync(diagnostico.IdConsulta);
            if (consulta == null)
            {
                return BadRequest(new { mensaje = "La consulta especificada no existe" });
            }
            var diagnosticoExistente = await BackendContext.Diagnosticos
                .FirstOrDefaultAsync(d => d.IdConsulta == diagnostico.IdConsulta);

            if (diagnosticoExistente != null)
            {
                return BadRequest(new { mensaje = "Ya existe un diagnóstico para esta consulta" });
            }

            BackendContext.Diagnosticos.Add(diagnostico);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetDiagnostico", new { id = diagnostico.IdDiagnostico },
                new
                {
                    mensaje = "Diagnóstico creado exitosamente",
                    diagnostico = diagnostico
                });
        }
    }
}
