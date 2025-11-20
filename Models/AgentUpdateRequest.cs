namespace VoiceAPI.Models
{
    public class ServicioUpdateItem
    {
        public required string IdServicio { get; set; }
        public required int Prioridad { get; set; }
        public string? Interno { get; set; }
    }

    public class AgentUpdateRequest
    {
        public string? nombre { get; set; }
        public string? apellido { get; set; }
        public string? interno { get; set; }
        public List<ServicioUpdateItem>? servicios { get; set; }
    }
}

