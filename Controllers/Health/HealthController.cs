using Microsoft.AspNetCore.Mvc;

namespace VoiceAPI.Controllers.Health
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "OK",
                message = "VoiceAPI esta Activo..."
            });
        }
    }
}

