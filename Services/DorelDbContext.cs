using DorelAppBackend.Models.DbModels;
using Microsoft.EntityFrameworkCore;

namespace DorelAppBackend.Services
{
    public class DorelDbContext: DbContext
    {
        public DorelDbContext(DbContextOptions<DorelDbContext> options) : base(options)
        {
        }
        public DbSet<DBUserLoginInfoModel> Users { get; set; }

        public DbSet<DBServiciuModel> Servicii { get; set; }

        public DbSet<DBJudetModel> Judete { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define for servicii

            modelBuilder.Entity<JunctionServicii>()
                .HasKey(js => new { js.UserID, js.ServiciuIdID });

            modelBuilder.Entity<JunctionServicii>()
                .HasOne(js => js.User)
                .WithMany(u => u.JunctionServicii)
                .HasForeignKey(js => js.UserID);

            modelBuilder.Entity<JunctionServicii>()
                .HasOne(js => js.Serviciu)
                .WithMany(s => s.JunctionServicii)
                .HasForeignKey(js => js.ServiciuIdID);

            // Define for judete

            modelBuilder.Entity<JunctionJudete>()
                .HasKey(js => new { js.UserID, js.JudetID });

            modelBuilder.Entity<JunctionJudete>()
                .HasOne(js => js.User)
                .WithMany(u => u.JunctionJudete)
                .HasForeignKey(js => js.UserID);

            modelBuilder.Entity<JunctionJudete>()
                .HasOne(js => js.Judet)
                .WithMany(s => s.JunctionJudete)
                .HasForeignKey(js => js.JudetID);
        }
    }
}
