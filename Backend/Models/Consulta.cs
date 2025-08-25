using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Consulta
    {
        [Key]
        public int IdConsulta { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DataType(DataType.DateTime)]
        public DateTime Fecha { get; set; }

        [StringLength(200)]
        public string? Motivo { get; set; }

        public string? Observaciones { get; set; }

        [StringLength(20)]
        public string? Estado { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? FechaFin { get; set; }

        [Range(0, 10000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Costo { get; set; }

        public string? Sintomas { get; set; }

        public string? ExamenFisico { get; set; }

        [Required]
        public int IdPaciente { get; set; }

        [ForeignKey(nameof(IdPaciente))]
        [DeleteBehavior(DeleteBehavior.ClientSetNull)]
        [JsonIgnore]
        public Paciente? Paciente { get; set; }

        [Required]
        public int IdNutricionista { get; set; }

        [ForeignKey(nameof(IdNutricionista))]
        [DeleteBehavior(DeleteBehavior.ClientSetNull)]
        [JsonIgnore]
        public Nutricionista? Nutricionista { get; set; }
    }
}
