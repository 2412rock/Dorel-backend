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

        public DbSet<JunctionServiciuJudete> JunctionServiciuJudete { get; set; }

        public DbSet<DBReviewModel> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JunctionServiciuJudete>().HasKey(j => new { j.UserID, j.ServiciuIdID, j.JudetID });

        }
    }
}
