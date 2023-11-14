using DorelAppBackend.Models.DbModels;
using Microsoft.EntityFrameworkCore;

namespace DorelAppBackend.Services
{
    public class LoginDbContext: DbContext
    {
        public LoginDbContext(DbContextOptions<LoginDbContext> options) : base(options)
        {
        }
        public DbSet<UserLoginInfoModel> Users { get; set; }
    }
}
