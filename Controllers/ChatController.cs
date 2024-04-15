using Microsoft.AspNetCore.Mvc;

namespace DorelAppBackend.Controllers
{
    public class ChatController : ControllerBase
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
