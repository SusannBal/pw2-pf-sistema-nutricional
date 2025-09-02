using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        public Consulta? Consulta { get; set; }
    }
}
