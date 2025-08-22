using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Diagnostico
    {
        [Key]
        public int IdDiagnostico { get; set; }

        [Required(ErrorMessage = "El detalle del diagnóstico es obligatorio")]
        public string? Detalle { get; set; }

        [Required]
        public int IdConsulta { get; set; }

        [ForeignKey(nameof(IdConsulta))]
        public Consulta? Consulta { get; set; }
    }
}
