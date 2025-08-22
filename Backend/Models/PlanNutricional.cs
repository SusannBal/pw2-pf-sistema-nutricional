using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public Consulta? Consulta { get; set; }
    }
}
