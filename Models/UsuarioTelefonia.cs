using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoiceAPI.Models
{
    [Table("UsuariosTelefonia")]
    public class UsuarioTelefonia
    {
        [StringLength(5)]
        public string? PbxId { get; set; }

        [StringLength(30)]
        public string? Cliente { get; set; }

        [StringLength(30)]
        public string? IdUsuario { get; set; }

        [StringLength(50)]
        public string? Nombre { get; set; }

        [StringLength(50)]
        public string? Apellido { get; set; }

        [StringLength(20)]
        public string? Rol { get; set; } // agente o administrativo

        [Key]
        [StringLength(5)]
        public string IdAgente { get; set; } = default!;

        [StringLength(10)]
        public string? Interno { get; set; }

        [StringLength(50)]
        public string? Servicios { get; set; }

        [StringLength(1)]
        public string? Prioridad { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}

