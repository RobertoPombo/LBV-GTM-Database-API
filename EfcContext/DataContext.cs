using Microsoft.EntityFrameworkCore;

using LBV_Basics.Models;

namespace LBV_GTM_Database_API.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs) { fk.DeleteBehavior = DeleteBehavior.NoAction; }

            base.OnModelCreating(modelBuilder);
        }
    }
}
