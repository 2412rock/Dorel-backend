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
        public async Task<IActionResult> GetImaginiForServiciu([FromQuery] int serviciuId, bool ofer)
        {
            var result = await _dataService.GetImaginiServiciuUser(serviciuId, (string)HttpContext.Items["Email"], ofer);
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
        public async Task<IActionResult> GetSearchResult([FromQuery] int serviciuId, int judetId, int pageNumber, bool ofer)
        {
            var result = await _dataService.GetServiciiForJudet(serviciuId, judetId, pageNumber, ofer);
            return Ok(result);
        }

        [HttpGet]
        [AuthorizationFilter]
        [Route("api/getAllUsers")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int serviciuId, int judetId, int pageNumber)
        {
            var result = await _dataService.GetAllUsers();
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
        public async Task<IActionResult> GetImaginiServiciuUser([FromQuery] int serviciuId, int judetId, int userId, bool ofer)
        {
            var result = await _dataService.GetImaginiForServiciuOfUser(serviciuId, judetId, userId, ofer);
            return Ok(result);
        }

        [AuthorizationFilter]
        [HttpGet]
        [Route("api/getServiciiUser")]
        public IActionResult GetServiciiUser([FromQuery] bool ofer)
        {
            var result = _dataService.GetServiciiForUser((string)HttpContext.Items["Email"], ofer);
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
        public async Task<IActionResult> GetServiciiUserAsSearchResults([FromQuery] bool ofer)
        {
            var result = await _dataService.GetServiciiForUserAsSearchResults((string)HttpContext.Items["Email"], ofer);
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
            var result = await _dataService.AssignServiciu((string)HttpContext.Items["Email"], request.ServiciuId, request.JudeteIds, request.Descriere, request.Imagini, request.Ofer);
            return Ok(result);
        }

        [AuthorizationFilter]
        [HttpPost]
        [Route("api/editUserServiciu")]
        public async Task<IActionResult> EditUserServiciu(AssignRequest request)
        {
            var result = await _dataService.EditServiciu((string)HttpContext.Items["Email"], request.ServiciuId, request.JudeteIds, request.Descriere, request.Imagini, request.Ofer);
            return Ok(result);
        }

        [AuthorizationFilter]
        [HttpDelete]
        [Route("api/deleteUserServiciu")]
        public async Task<IActionResult> DeleteUserServiciu([FromQuery] int serviciuId, bool ofer)
        {
            var result =  await _dataService.DeleteUserServiciu((string)HttpContext.Items["Email"], serviciuId, ofer);
            return Ok(result);
        }
    }
}
