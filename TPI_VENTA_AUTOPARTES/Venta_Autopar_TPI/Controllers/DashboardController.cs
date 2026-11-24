using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Venta_Autopar_TPI.Core.DTOs;
using Venta_Autopar_TPI.Core.Services.Interfaces;

namespace Venta_Autopar_TPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("stock-critico")]
        public async Task<ActionResult<List<StockCriticoResponseDto>>> GetStockCritico()
        {
            var result = await _dashboardService.GetStockCriticoAsync();

            return Ok(result);
        }

        [HttpGet("facturacion/diaria")]
        public async Task<ActionResult<FacturacionTotalResponseDto>> GetFacturacionDiaria()
        {
            var result = await _dashboardService.GetFacturacionDiariaAsync();

            return Ok(result);
        }

        [HttpGet("facturacion/mensual")]
        public async Task<ActionResult<FacturacionTotalResponseDto>> GetFacturacionMensual()
        {
            var result = await _dashboardService.GetFacturacionMensualAsync();

            return Ok(result);
        }

        [HttpGet("facturacion/periodo")]
        public async Task<ActionResult<FacturacionTotalResponseDto>> GetFacturacionPorPeriodo(
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate)
        {
            try
            {
                var result = await _dashboardService.GetFacturacionPorPeriodoAsync(
                    startDate,
                    endDate);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("autopartes-mas-vendidas")]
        public async Task<ActionResult<List<AutoparteMasVendidaResponseDto>>> GetAutopartesMasVendidas(
            [FromQuery] int limit = 5)
        {
            try
            {
                var result = await _dashboardService.GetAutopartesMasVendidasAsync(limit);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
