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
        private readonly Data.BackendContext BackendContext;

        public ActividadFisicaController(Data.BackendContext BackendContext)
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

        // PUT: api/ActividadFisica/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutActividadFisica(int id, ActividadFisica actividadFisica)
        {
            if (id != actividadFisica.IdActividad)
            {
                return BadRequest(new { mensaje = "El ID de la actividad física no coincide" });
            }

            // Validar nombre único (si se cambia)
            var actividadExistente = await BackendContext.ActividadesFisicas
                .FirstOrDefaultAsync(a => a.Nombre == actividadFisica.Nombre && a.IdActividad != id);
            if (actividadExistente != null)
            {
                return BadRequest(new { mensaje = "Ya existe una actividad física con este nombre" });
            }

            BackendContext.Entry(actividadFisica).State = EntityState.Modified;

            try
            {
                await BackendContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActividadFisicaExists(id))
                {
                    return NotFound(new { mensaje = "Actividad física no encontrada" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { mensaje = "Actividad física actualizada exitosamente" });
        }

        // DELETE: api/ActividadFisica/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActividadFisica(int id)
        {
            var actividad = await BackendContext.ActividadesFisicas.FindAsync(id);
            if (actividad == null)
            {
                return NotFound(new { mensaje = "Actividad física no encontrada" });
            }

            // Verificar si la actividad tiene registros asociados
            var hasRegistros = await BackendContext.RegistrosActividades.AnyAsync(ra => ra.IdActividad == id);
            if (hasRegistros)
            {
                return BadRequest(new { mensaje = "No se puede eliminar la actividad física porque tiene registros asociados." });
            }

            BackendContext.ActividadesFisicas.Remove(actividad);
            await BackendContext.SaveChangesAsync();

            return Ok(new { mensaje = "Actividad física eliminada exitosamente" });
        }

        private bool ActividadFisicaExists(int id)
        {
            return BackendContext.ActividadesFisicas.Any(e => e.IdActividad == id);
        }
    }
}