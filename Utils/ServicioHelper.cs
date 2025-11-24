using System.Text.Json;
using VoiceAPI.Models.Agent;

namespace VoiceAPI.Utils
{
    public static class ServicioHelper
    {
        public static List<ServicioAgente> FromJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<ServicioAgente>();

            try
            {
                return JsonSerializer.Deserialize<List<ServicioAgente>>(json)
                       ?? new List<ServicioAgente>();
            }
            catch
            {
                return new List<ServicioAgente>();
            }
        }

        public static string ToJson(List<ServicioAgente> list)
        {
            return JsonSerializer.Serialize(list);
        }
    }
}

