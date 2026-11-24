
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.DTOs;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Core.Services.Interfaces;

namespace Venta_Autopar_TPI.Core.Services.Implementations
{
    public class MovimientoStockService : IMovimientoStockService
    {
        private readonly IMovimientoStockRepository _movimientoStockRepository;

        public MovimientoStockService(
            IMovimientoStockRepository movimientoStockRepository)
        {
            _movimientoStockRepository = movimientoStockRepository;
        }

        public async Task<List<MovimientoStockDto>> GetByAutoparteIdAsync(
            int autoparteId)
        {
            if (autoparteId <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la autoparte no es válido");
            }

            int? currentStock = await _movimientoStockRepository
                .GetCurrentStockAsync(autoparteId);

            if (!currentStock.HasValue)
            {
                throw new KeyNotFoundException(
                    "No se encontró el inventario de la autoparte solicitada");
            }

            var movements = await _movimientoStockRepository
                .GetByAutoparteIdAsync(autoparteId);

            List<MovimientoStockDto> response = new();

            int stock = currentStock.Value;

            foreach (var movement in movements)
            {
                MovimientoStockDto dto = new()
                {
                    MovimientoStockId = movement.MovimientoStockId,
                    AutoparteId = movement.AutoparteId,
                    TipoMovimiento = movement.TipoMovimiento,
                    Cantidad = movement.Cantidad,
                    FechaMovimiento = movement.FechaMovimiento,
                    StockResultante = stock,
                    FacturaId = movement.FacturaId,
                    OrdenReposicionId = movement.OrdenReposicionId,
                    Observaciones = movement.Observaciones
                };

                response.Add(dto);

                if (movement.TipoMovimiento == "E")
                {
                    stock -= movement.Cantidad;
                }
                else if (movement.TipoMovimiento == "S")
                {
                    stock += movement.Cantidad;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"El tipo de movimiento '{movement.TipoMovimiento}' no es válido");
                }
            }

            return response;
        }
    }
}
