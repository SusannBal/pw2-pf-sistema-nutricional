using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Nutricionista
    {
        [Key, ForeignKey(nameof(Persona))]
        public int IdNutricionista { get; set; }

        [Required(ErrorMessage = "La matrícula es obligatoria")]
        [StringLength(50)]
        public string? Matricula { get; set; }

        [StringLength(100)]
        public string? Especialidad { get; set; }

        [Range(0, 100, ErrorMessage = "Los años de experiencia deben estar entre 0 y 50")]
        public int AnosExperiencia { get; set; }
        public Persona? Persona { get; set; }
    }
}
