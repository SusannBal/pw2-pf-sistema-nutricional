using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Recomendacion
    {
        [Key]
        public int IdRecomendacion { get; set; }

        [Required(ErrorMessage = "El detalle de la recomendación es obligatorio")]
        public string? Detalle { get; set; }

        [Required]
        public int IdConsulta { get; set; }

        [ForeignKey(nameof(IdConsulta))]
        public Consulta? Consulta { get; set; }
    }
}
