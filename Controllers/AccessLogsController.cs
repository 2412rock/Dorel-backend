using DorelAppBackend.Models.Requests;
using DorelAppBackend.Services.Implementation;
using DorelAppBackend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DorelAppBackend.Controllers
{
    [ApiController]
    public class AccessLogsController : ControllerBase
    {
        private readonly IAccessLogsService _accessLogsService;
        public AccessLogsController(IAccessLogsService accessLogsService)
        {
            _accessLogsService = accessLogsService;
        }

        [HttpPost]
        [Route("api/addLog")]
        public async Task<IActionResult> AddLog()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            if(ipAddress != null)
            {
                await _accessLogsService.AddLog(ipAddress.ToString());
            }

            return Ok();
           
        }
    }
}
