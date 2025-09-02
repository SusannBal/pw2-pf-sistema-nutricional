using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class PlanNutricional
    {
        [Key]
        public int IdPlan { get; set; }

        [Required(ErrorMessage = "El nombre del plan es obligatorio")]
        [StringLength(100)]
        public string? Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        public int IdConsulta { get; set; }

        [ForeignKey(nameof(IdConsulta))]
        [JsonIgnore]
        public Consulta? Consulta { get; set; }
        public string? DisplayInfo { get; set; }
    }
}
