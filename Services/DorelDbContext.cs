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

        public DbSet<DBMessage> Messages { get; set; }

        public DbSet<DBAccessLog> AccessLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          

        }
    }
}
