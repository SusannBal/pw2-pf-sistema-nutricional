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
    public class ActividadFisicaController : ControllerBase
    {
        private readonly BackendContext BackendContext;

        public ActividadFisicaController(BackendContext BackendContext)
        {
            this.BackendContext = BackendContext;
        }

        // GET: api/ActividadFisica
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActividadFisica>>> GetActividadesFisicas()
        {
            return await BackendContext.ActividadesFisicas.ToListAsync();
        }

        // GET: api/ActividadFisica/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ActividadFisica>> GetActividadFisica(int id)
        {
            var actividad = await BackendContext.ActividadesFisicas.FindAsync(id);

            if (actividad == null)
            {
                return NotFound(new { mensaje = "Actividad física no encontrada" });
            }

            return actividad;
        }

        // POST: api/ActividadFisica
        [HttpPost]
        public async Task<ActionResult<ActividadFisica>> PostActividadFisica(ActividadFisica actividadFisica)
        {
            // Validar nombre único
            var actividadExistente = await BackendContext.ActividadesFisicas
                .FirstOrDefaultAsync(a => a.Nombre == actividadFisica.Nombre);

            if (actividadExistente != null)
            {
                return BadRequest(new { mensaje = "Ya existe una actividad física con este nombre" });
            }

            BackendContext.ActividadesFisicas.Add(actividadFisica);
            await BackendContext.SaveChangesAsync();

            return CreatedAtAction("GetActividadFisica", new { id = actividadFisica.IdActividad },
                new
                {
                    mensaje = "Actividad física creada exitosamente",
                    actividad = actividadFisica
                });
        }

    }
}
