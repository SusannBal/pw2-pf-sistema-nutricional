using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Persona
    {
        [Key]
        public int IdPersona { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string? ApellidoPaterno { get; set; }

        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string? ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "El CI es obligatorio")]
        [StringLength(20, MinimumLength = 7, ErrorMessage = "El CI debe tener entre 7 y 20 caracteres")]
        public string? CI { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }
    }
}
