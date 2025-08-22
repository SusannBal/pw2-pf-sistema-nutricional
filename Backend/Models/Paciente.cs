using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Paciente
    {
        [Key, ForeignKey(nameof(Persona))]
        public int IdPaciente { get; set; }

        [StringLength(200)]
        public string? Objetivo { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string? Estado { get; set; }

        [Range(0, 500)]
        [Column(TypeName = "decimal(6,2)")]
        public decimal? PesoInicial { get; set; }

        [Range(0.5, 3.0)]
        [Column(TypeName = "decimal(3,2)")]
        public decimal? TallaInicial { get; set; }

        [StringLength(50)]
        public string? TipoSangre { get; set; }

        public string? PreferenciasAlimentarias { get; set; }

        public Persona? Persona { get; set; }

    }
}
