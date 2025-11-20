using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoiceAPI.Models
{
    [Table("UsuariosTelefonia")]
    public class UsuarioTelefonia
    {
        public string? PbxId { get; set; }
        public string? Cliente { get; set; }
        public string? IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }

        [Required]
        public string Rol { get; set; } = "";

        [Key]
        [Required]
        public string IdAgente { get; set; } = "";

        public string? Interno { get; set; }

        // JSON con la lista de servicios
        public string? Servicios { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}

