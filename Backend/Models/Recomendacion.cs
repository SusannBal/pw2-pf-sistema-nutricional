using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        public Consulta? Consulta { get; set; }
    }
}
