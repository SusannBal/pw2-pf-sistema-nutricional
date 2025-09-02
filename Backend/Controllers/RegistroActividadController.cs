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
    public class RegistroActividadController : ControllerBase
    {
        private readonly BackendContext BackendContext;

        public RegistroActividadController(BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/RegistroActividad
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistroActividad>>> GetRegistrosActividad()
        {
            return await BackendContext.RegistrosActividades
                .Include(r => r.Paciente)
                .Include(r => r.Actividad)
                .ToListAsync();
        }

        // GET: api/RegistroActividad/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RegistroActividad>> GetRegistroActividad(int id)
        {
            var registro = await BackendContext.RegistrosActividades
                .Include(r => r.Paciente)
                .Include(r => r.Actividad)
                .FirstOrDefaultAsync(r => r.IdRegistro == id);

            if (registro == null)
            {
                return NotFound(new { mensaje = "Registro de actividad no encontrado" });
            }

            return registro;
        }

        // POST: api/RegistroActividad
        [HttpPost]
        public async Task<ActionResult<RegistroActividad>> PostRegistroActividad(RegistroActividad registroActividad)
        {
            // Validar que el paciente existe
            var paciente = await BackendContext.Pacientes.FindAsync(registroActividad.IdPaciente);
            if (paciente == null)
            {
                return BadRequest(new { mensaje = "El paciente especificado no existe" });
            }

            // Validar que la actividad existe
            var actividad = await BackendContext.ActividadesFisicas.FindAsync(registroActividad.IdActividad);
            if (actividad == null)
            {
                return BadRequest(new { mensaje = "La actividad física especificada no existe" });
            }

            // Validar que la fecha no sea futura
            if (registroActividad.Fecha > DateTime.Now)
            {
                return BadRequest(new { mensaje = "La fecha no puede ser futura" });
            }

            // Validar duración mínima y máxima
            if (registroActividad.DuracionMin < 1 || registroActividad.DuracionMin > 1440)
            {
                return BadRequest(new { mensaje = "La duración debe estar entre 1 y 1440 minutos" });
            }

            BackendContext.RegistrosActividades.Add(registroActividad);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetRegistroActividad", new { id = registroActividad.IdRegistro },
                new
                {
                    mensaje = "Registro de actividad creado exitosamente",
                    registro = registroActividad
                });
        }   
    }
}
