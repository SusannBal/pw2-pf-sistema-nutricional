using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Recordatorio
    {
        [Key]
        public int IdRecordatorio { get; set; }

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        [StringLength(200)]
        public string? Mensaje { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime FechaHora { get; set; }

        [Required]
        public int IdPersona { get; set; }

        [ForeignKey(nameof(IdPersona))]
        [DeleteBehavior(DeleteBehavior.ClientSetNull)]
        public Persona? Persona { get; set; }
    }
}
