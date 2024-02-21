using DorelAppBackend.Filters;
using DorelAppBackend.Models.Requests;
using DorelAppBackend.Services.Implementation;
using DorelAppBackend.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

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

        [AuthorizationFilter]
        [HttpGet]
        [Route("api/getServiciiUser")]
        public IActionResult GetServiciiUser()
        {
            var result = _dataService.GetServiciiForUser((string)HttpContext.Items["Email"]);
            return Ok(result);
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

        [AuthorizationFilter]
        [HttpPost]
        [Route("api/assignUserServiciiAndJudet")]
        public async Task<IActionResult> AssignUserServicii(AssignRequest request)
        {
            // 'token' now contains the JWT token from the request
            var result = await _dataService.AssignServiciu((string)HttpContext.Items["Email"], request.ServiciuId, request.JudeteIds, request.Descriere, request.Imagini);
            return Ok(result);
        }
    }
}
