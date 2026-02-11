using DataGen_v1.Models;
using Microsoft.EntityFrameworkCore;

namespace DataGen_v1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Generator> Generators { get; set; }
        public DbSet<NodeConfig> NodeConfigs { get; set; }
        public DbSet<ApiConfig> ApiConfigs { get; set; }
        public DbSet<DbConnectionConfig> DbConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NodeConfig>()
                .HasOne(n => n.Parent)
                .WithMany(n => n.Children)
                .HasForeignKey(n => n.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
