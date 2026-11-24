
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.DTOs;
using Venta_Autopar_TPI.Core.Services.Interfaces;

namespace Venta_Autopar_TPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AutoparteController : ControllerBase
    {
        private readonly IAutoparteService _autoparteService;

        public AutoparteController(IAutoparteService autoparteService)
        {
            _autoparteService = autoparteService;
        }

        
        [HttpGet]
        public async Task<ActionResult<List<AutoparteResponseDto>>> GetAll()
        {
            var autopartes = await _autoparteService.GetAllAsync();

            return Ok(autopartes);
        }

        
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AutoparteResponseDto>> GetById(int id)
        {
            var autoparte = await _autoparteService.GetByIdAsync(id);

            if (autoparte == null)
                return NotFound(new
                {
                    message = "No se encontró la autoparte."
                });

            return Ok(autoparte);
        }

        
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateAutoparteDto autoparteDto)
        {
            try
            {
                await _autoparteService.CreateAsync(autoparteDto);

                return StatusCode(201, new
                {
                    message = "Autoparte registrada correctamente."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

       
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateAutoparteDto autoparteDto)
        {
            try
            {
                await _autoparteService.UpdateAsync(id, autoparteDto);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        
        [HttpPost("{id:int}/stock/entrada")]
        public async Task<IActionResult> AddStock(
            int id,
            [FromBody] StockDto stockDto)
        {
            try
            {
                bool result = await _autoparteService.AddStockAsync(
                    id,
                    stockDto
                );

                if (!result)
                    return NotFound(new
                    {
                        message = "No se encontró el inventario de la autoparte."
                    });

                return Ok(new
                {
                    message = "Entrada de stock registrada correctamente."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        
        [HttpPost("{id:int}/stock/salida")]
        public async Task<IActionResult> RemoveStock(
            int id,
            [FromBody] StockDto stockDto)
        {
            try
            {
                bool result = await _autoparteService.RemoveStockAsync(
                    id,
                    stockDto
                );

                if (!result)
                    return Conflict(new
                    {
                        message = "No existe inventario o no hay stock suficiente."
                    });

                return Ok(new
                {
                    message = "Salida de stock registrada correctamente."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        
        [HttpPost("{id:int}/precios")]
        public async Task<IActionResult> UpdatePrice(
            int id,
            [FromBody] PrecioDto precioDto)
        {
            try
            {
                bool result = await _autoparteService.UpdatePriceAsync(
                    id,
                    precioDto
                );

                if (!result)
                    return Conflict(new
                    {
                        message = "No se pudo actualizar el precio. Verifique que exista un precio vigente."
                    });

                return Ok(new
                {
                    message = "Precio actualizado correctamente."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        
        [HttpPatch("{id:int}/estado")]
        public async Task<IActionResult> ChangeStatus(
            int id,
            [FromBody] bool status)
        {
            bool updated =
                await _autoparteService.ChangeStatusAsync(id, status);

            if (!updated)
                return NotFound(new
                {
                    message = "No se encontró la autoparte."
                });

            return Ok(new
            {
                message = status
                    ? "Autoparte reactivada correctamente."
                    : "Autoparte dada de baja correctamente."
            });
        }

        [HttpGet("{id:int}/precios")]
        public async Task<IActionResult> GetPriceHistory(int id)
        {
            try
            {
                var history = await _autoparteService.GetPriceHistoryAsync(id);

                return Ok(history);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
        }

    }
}
