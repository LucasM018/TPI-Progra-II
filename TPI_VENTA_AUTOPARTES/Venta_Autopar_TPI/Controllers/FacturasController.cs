
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.DTOs;
using Venta_Autopar_TPI.Core.Services.Interfaces;

namespace Venta_Autopar_TPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FacturaController : ControllerBase
    {
        private readonly IFacturaService _facturaService;

        public FacturaController(IFacturaService facturaService)
        {
            _facturaService = facturaService;
        }

        
        [HttpPost("preview")]
        public async Task<IActionResult> Preview(
            [FromBody] CreateFacturaDto dto)
        {
            try
            {
                FacturaPreviewResponseDto response =
                    await _facturaService.PreviewAsync(dto);

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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateFacturaDto dto)
        {
            try
            {
                FacturaResponseDto response =
                    await _facturaService.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { facturaId = response.FacturaId },
                    response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<FacturaResponseDto> response =
                await _facturaService.GetAllAsync();

            return Ok(response);
        }

        
        [HttpGet("{facturaId:int}")]
        public async Task<IActionResult> GetById(int facturaId)
        {
            try
            {
                FacturaResponseDto? response =
                    await _facturaService.GetByIdAsync(facturaId);

                if (response == null)
                {
                    return NotFound(new
                    {
                        message = "La factura solicitada no existe."
                    });
                }

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpGet("cliente/{clienteId:int}")]
        public async Task<IActionResult> GetByClienteId(int clienteId)
        {
            try
            {
                List<FacturaResponseDto> response =
                    await _facturaService.GetByClienteIdAsync(clienteId);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpGet("fecha")]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                List<FacturaResponseDto> response =
                    await _facturaService.GetByDateRangeAsync(
                        startDate,
                        endDate);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpPost("{facturaId:int}/anular")]
        public async Task<IActionResult> Cancel(int facturaId)
        {
            try
            {
                await _facturaService.CancelAsync(facturaId);

                return Ok(new
                {
                    message = "La factura fue anulada correctamente."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
