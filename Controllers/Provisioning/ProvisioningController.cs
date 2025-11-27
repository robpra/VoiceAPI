using Microsoft.AspNetCore.Mvc;
using VoiceAPI.Models.Provisioning;
using VoiceAPI.Utils;

namespace VoiceAPI.Controllers.Provisioning
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvisioningController : ControllerBase
    {
        [HttpPost("findPBX")]
        public IActionResult FindPBX([FromBody] ProvisionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Servicio))
                return BadRequest("Servicio requerido.");

            // Obtener cliente si viene en el request
            var cliente = request.Cliente?.Trim().ToUpper() ?? "";

            // Nuevo método GetPBX(cliente, servicio)
            var pbx = ServicioHelper.GetPBX(cliente, request.Servicio);

            if (pbx == null)
            {
                // Fallback automático: buscar solo por servicio
                pbx = ServicioHelper.GetPBX("", request.Servicio);
            }

            if (pbx == null)
                return NotFound($"No se encontró PBX para el servicio '{request.Servicio}'.");

            return Ok(new
            {
                pbx.Id,
                pbx.Cliente,
                pbx.Host,
                pbx.WssPort,
                pbx.ApiInternos
            });
        }
    }
}

