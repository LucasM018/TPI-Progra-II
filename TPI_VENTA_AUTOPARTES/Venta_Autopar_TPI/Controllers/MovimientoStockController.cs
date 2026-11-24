
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Venta_Autopar_TPI.Core.DTOs;
using Venta_Autopar_TPI.Core.Services.Interfaces;

namespace Venta_Autopar_TPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MovimientoStockController : ControllerBase
    {
        private readonly IMovimientoStockService _movimientoStockService;

        public MovimientoStockController(
            IMovimientoStockService movimientoStockService)
        {
            _movimientoStockService = movimientoStockService;
        }

        
        [HttpGet("autoparte/{autoparteId:int}")]
        public async Task<IActionResult> GetByAutoparteId(int autoparteId)
        {
            try
            {
                List<MovimientoStockDto> response =
                    await _movimientoStockService.GetByAutoparteIdAsync(
                        autoparteId);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
