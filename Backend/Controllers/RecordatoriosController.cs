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
    public class RecordatoriosController : ControllerBase
    {
        private readonly BackendContext BackendContext;

        public RecordatoriosController(BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/Recordatorios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recordatorio>>> GetRecordatorios()
        {
            return await BackendContext.Recordatorios
                .Include(r => r.Persona)
                .ToListAsync();
        }

        // GET: api/Recordatorios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Recordatorio>> GetRecordatorio(int id)
        {
            var recordatorio = await BackendContext.Recordatorios
                .Include(r => r.Persona)
                .FirstOrDefaultAsync(r => r.IdRecordatorio == id);

            if (recordatorio == null)
            {
                return NotFound(new { mensaje = "Recordatorio no encontrado" });
            }

            return recordatorio;
        }

        // POST: api/Recordatorios
        [HttpPost]
        public async Task<ActionResult<Recordatorio>> PostRecordatorio(Recordatorio recordatorio)
        {
            var persona = await BackendContext.Personas.FindAsync(recordatorio.IdPersona);
            if (persona == null)
            {
                return BadRequest(new { mensaje = "La persona especificada no existe" });
            }

            // Validar que la fecha/hora no sea en el pasado
            if (recordatorio.FechaHora < DateTime.Now)
            {
                return BadRequest(new { mensaje = "La fecha y hora del recordatorio no puede ser en el pasado" });
            }

            BackendContext.Recordatorios.Add(recordatorio);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetRecordatorio", new { id = recordatorio.IdRecordatorio },
                new
                {
                    mensaje = "Recordatorio creado exitosamente",
                    recordatorio = recordatorio
                });
        }
    }
}
