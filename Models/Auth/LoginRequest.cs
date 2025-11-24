namespace VoiceAPI.Models.Auth
{
    public class LoginRequest
    {
        public string? Usuario { get; set; }
        public string? IdUsuario { get; set; }
        public string? IdAgente { get; set; }
        public string? Servicio { get; set; }
        public string? Cliente { get; set; }
        public string? Rol { get; set; }
    }
}

