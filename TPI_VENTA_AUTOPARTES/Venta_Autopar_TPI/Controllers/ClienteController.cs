
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.Services.Interfaces;

namespace Venta_Autopar_TPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClienteController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        
        [HttpGet("activos")]
        public async Task<IActionResult> GetAllActive()
        {
            var response = await _clienteService.GetAllActiveAsync();

            return Ok(response);
        }
    }
}
