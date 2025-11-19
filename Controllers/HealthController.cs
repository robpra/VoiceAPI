using Microsoft.AspNetCore.Mvc;
using VoiceAPI.Services;

namespace VoiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly PBXManager _pbx;

        public HealthController(PBXManager pbx)
        {
            _pbx = pbx;
        }

        [HttpGet]
        public IActionResult Check()
        {
            return Ok(new
            {
                api = "VoiceAPI running",
                pbxs = _pbx.PingPBXs()
            });
        }
    }
}

