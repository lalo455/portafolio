using Microsoft.AspNetCore.Mvc;
using Utilities;

namespace AplicacionWebPing.Controllers
{
    [Route("api/ping-check")]
    [ApiController]
    public class PingApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Check()
        {
            bool ok = PingClass.IsValid();
            return Ok(new
            {
                ok,
                error = ok ? "" : PingClass.GetError()
            });
        }
    }
}
