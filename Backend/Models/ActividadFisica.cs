using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ActividadFisica
    {
        [Key]
        public int IdActividad { get; set; }

        [Required(ErrorMessage = "El nombre de la actividad es obligatorio")]
        [StringLength(100)]
        public string? Nombre { get; set; }

        public string? Descripcion { get; set; }
    }
}
