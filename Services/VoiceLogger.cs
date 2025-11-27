using System.Text;

namespace VoiceAPI.Services
{
    public class VoiceLogger
    {
        private readonly string _basePath = "/var/log/voiceapi/";

        public VoiceLogger()
        {
            // Crear carpeta si no existe
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        private void Write(string file, string text)
        {
            string path = Path.Combine(_basePath, file);
            string line = text + Environment.NewLine;

            File.AppendAllText(path, line, Encoding.UTF8);
            Console.WriteLine(line); // también sale por consola (journalctl)
        }

        // ====== MODOS DE LOG ======

        public void Auth(string text)           => Write("auth.log", text);
        public void Provisioning(string text)   => Write("provisioning.log", text);
        public void Hooks(string text)          => Write("hooks.log", text);
        public void Error(string text)          => Write("errors.log", text);
        public void System(string text)         => Write("system.log", text);

        // ====== Plantilla de Auditoría ======

        public static string Audit(string tag, string body)
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{tag}]\n{body}\n";
        }
    }
}

