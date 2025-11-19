using Microsoft.EntityFrameworkCore;
using VoiceAPI.Models;

namespace VoiceAPI.Data
{
    public class AgentDbContext : DbContext
    {
        public AgentDbContext(DbContextOptions<AgentDbContext> options) : base(options)
        {
        }

        public DbSet<UsuarioTelefonia> UsuariosTelefonia { get; set; }
    }
}

