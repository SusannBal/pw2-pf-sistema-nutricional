using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class HistorialPaciente
    {
        [Key]
        public int IdHistorial { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        [Range(1, 500, ErrorMessage = "El peso debe estar entre 1 y 500 kg")]
        [Column(TypeName = "decimal(6,2)")]
        public decimal Peso { get; set; }

        [Required]
        [Range(0.5, 3.0, ErrorMessage = "La talla debe estar entre 0.5 y 3.0 m")]
        [Column(TypeName = "decimal(3,2)")]
        public decimal Talla { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "El IMC debe estar entre 1 y 100")]
        [Column(TypeName = "decimal(4,1)")]
        public decimal IMC { get; set; }

        [Required]
        public int IdPaciente { get; set; }

        [ForeignKey(nameof(IdPaciente))]
        [DeleteBehavior(DeleteBehavior.ClientSetNull)]
        [JsonIgnore]
        public Paciente? Paciente { get; set; }
    }
}
