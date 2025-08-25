using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class HistorialMedico
    {
        [Key]
        public int IdHistorialMedico { get; set; }

        [Required(ErrorMessage = "El nombre de la enfermedad es obligatorio")]
        [StringLength(150)]
        public string? Enfermedad { get; set; }

        public string? Descripcion { get; set; }
        [DataType(DataType.Date)]
        public DateTime? FechaDiagnostico { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaCuracion { get; set; }
        public string? Alergias { get; set; }
        public string? Intolerancias { get; set; }
        public string? Tratamiento { get; set; }

        public string? Medicamentos { get; set; }

        [Required]
        public int IdPaciente { get; set; }

        [ForeignKey(nameof(IdPaciente))]
        [DeleteBehavior(DeleteBehavior.ClientSetNull)]
        [JsonIgnore]
        public Paciente? Paciente { get; set; }

    }
}
