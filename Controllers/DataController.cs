using DorelAppBackend.Models.Requests;
using DorelAppBackend.Services.Implementation;
using DorelAppBackend.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DorelAppBackend.Controllers
{
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly IDataService _dataService;
        public DataController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpPost]
        [Route("api/getJudete")]
        public IActionResult GetJudete(StartsWithRequest request)
        {
            var result = _dataService.GetJudete(request.StartsWith);
            return Ok(result);
        }

        [HttpPost]
        [Route("api/getServicii")]
        public IActionResult GetServicii(StartsWithRequest request)
        {
            var result = _dataService.GetServicii(request.StartsWith);
            return Ok(result);
        }
    }
}
