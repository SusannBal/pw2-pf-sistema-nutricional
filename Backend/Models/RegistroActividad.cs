using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class RegistroActividad
    {
        [Key]
        public int IdRegistro { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        [Range(1, 1440, ErrorMessage = "La duración debe estar entre 1 y 1440 minutos")]
        public int DuracionMin { get; set; }

        [Required]
        public int IdPaciente { get; set; }

        [ForeignKey(nameof(IdPaciente))]
        [DeleteBehavior(DeleteBehavior.ClientSetNull)]
        [JsonIgnore]
        public Paciente? Paciente { get; set; }

        [Required]
        public int IdActividad { get; set; }

        [ForeignKey(nameof(IdActividad))]
        [DeleteBehavior(DeleteBehavior.ClientSetNull)]
        [JsonIgnore]
        public ActividadFisica? Actividad { get; set; }
    }
}
