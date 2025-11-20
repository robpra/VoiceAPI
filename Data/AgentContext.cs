using Microsoft.EntityFrameworkCore;
using VoiceAPI.Models;

namespace VoiceAPI.Data
{
    public class AgentContext : DbContext
    {
        public AgentContext(DbContextOptions<AgentContext> options) : base(options)
        {
        }

        public DbSet<UsuarioTelefonia> UsuariosTelefonia { get; set; }
    }
}

