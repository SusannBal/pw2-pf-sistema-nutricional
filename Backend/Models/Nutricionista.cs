using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Nutricionista
    {
        [Key]
        public int IdNutricionista { get; set; }

        [Required]
        public int IdPersona { get; set; }

        [ForeignKey(nameof(IdPersona))]
        [JsonIgnore]
        public Persona? Persona { get; set; }

        [Required(ErrorMessage = "La matrícula es obligatoria")]
        [StringLength(50)]
        public string? Matricula { get; set; }

        [StringLength(100)]
        public string? Especialidad { get; set; }

        [Range(0, 100, ErrorMessage = "Los años de experiencia deben estar entre 0 y 100")]
        public int AnosExperiencia { get; set; }
    }
}
