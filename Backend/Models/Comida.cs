using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Comida
    {
        [Key]
        public int IdComida { get; set; }

        [Required(ErrorMessage = "El nombre de la comida es obligatorio")]
        [StringLength(100)]
        public string? Nombre { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        public int IdPlan { get; set; }

        [ForeignKey(nameof(IdPlan))]
        public PlanNutricional? PlanNutricional { get; set; }
    }
}
