using DorelAppBackend.Filters;
using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DorelAppBackend.Controllers
{
    [Controller]
    public class DebugController: ControllerBase
    {
        private readonly DorelDbContext _dbContext;
        public DebugController(DorelDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        [Route("api/getJunctions")]
        [AuthorizationFilter(Role = "admin")]
        public async Task<JunctionServiciuJudete[]> GetJunctions()
        {
           return  await _dbContext.JunctionServiciuJudete.ToArrayAsync();
        }
       
    }
}
