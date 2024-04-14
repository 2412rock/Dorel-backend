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
        [Route("api/getImaginiForServiciu")]
        public async Task<IActionResult> GetImaginiForServiciu([FromQuery] int serviciuId)
        {
            var result = await _dataService.GetImaginiServiciuUser(serviciuId, (string)HttpContext.Items["Email"]);
            return Ok(result);
        }

        [AuthorizationFilter]
        [HttpGet]
        [Route("api/getJudeteForServiciu")]
        public IActionResult GetJudeteForServiciu([FromQuery] int serviciuId)
        {
            var result = _dataService.GetJudeteForServiciu(serviciuId, (string)HttpContext.Items["Email"]);
            return Ok(result);
        }

        [HttpGet]
        [Route("api/getSearchResult")]
        public async Task<IActionResult> GetSearchResult([FromQuery] int serviciuId, int judetId, int pageNumber)
        {
            var result = await _dataService.GetServiciiForJudet(serviciuId, judetId, pageNumber);
            return Ok(result);
        }

        [AuthorizationFilter]
        [HttpGet]
        [Route("api/getDescriereForServiciu")]
        public IActionResult GetDescriereForServiciu([FromQuery] int serviciuId)
        {
            var result = _dataService.GetDescriereForServiciu(serviciuId, (string)HttpContext.Items["Email"]);
            return Ok(result);
        }

        [HttpGet]
        [Route("api/getImaginiServiciuUser")]
        public async Task<IActionResult> GetImaginiServiciuUser([FromQuery] int serviciuId, int judetId, int userId)
        {
            var result = await _dataService.GetImaginiForServiciuOfUser(serviciuId, judetId, userId);
            return Ok(result);
        }

        [AuthorizationFilter]
        [HttpGet]
        [Route("api/getServiciiUser")]
        public IActionResult GetServiciiUser()
        {
            var result = _dataService.GetServiciiForUser((string)HttpContext.Items["Email"]);
            return Ok(result);
        }

        [HttpGet]
        [Route("api/getAllJunctions")]
        public async Task<IActionResult> GetAllJunctions()
        {
            var result = await _dataService.GetAllJunctions();
            return Ok(result);
        }

        [AuthorizationFilter]
        [HttpGet]
        [Route("api/getServiciiUserAsSearchResults")]
        public async Task<IActionResult> GetServiciiUserAsSearchResults()
        {
            var result = await _dataService.GetServiciiForUserAsSearchResults((string)HttpContext.Items["Email"]);
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
            var result = await _dataService.AssignServiciu((string)HttpContext.Items["Email"], request.ServiciuId, request.JudeteIds, request.Descriere, request.Imagini);
            return Ok(result);
        }

        [AuthorizationFilter]
        [HttpPost]
        [Route("api/editUserServiciu")]
        public async Task<IActionResult> EditUserServiciu(AssignRequest request)
        {
            var result = await _dataService.EditServiciu((string)HttpContext.Items["Email"], request.ServiciuId, request.JudeteIds, request.Descriere, request.Imagini);
            return Ok(result);
        }

        [AuthorizationFilter]
        [HttpDelete]
        [Route("api/deleteUserServiciu")]
        public IActionResult DeleteUserServiciu([FromQuery] int serviciuId)
        {
            var result =  _dataService.DeleteUserServiciu((string)HttpContext.Items["Email"], serviciuId);
            return Ok(result);
        }
    }
}
