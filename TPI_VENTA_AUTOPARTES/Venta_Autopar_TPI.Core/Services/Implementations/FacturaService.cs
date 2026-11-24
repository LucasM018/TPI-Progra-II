using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.DTOs;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Core.Services.Interfaces;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Services.Implementations
{
    public class FacturaService : IFacturaService
    {
        private readonly IFacturaRepository _facturaRepository;

        public FacturaService(IFacturaRepository facturaRepository)
        {
            _facturaRepository = facturaRepository;
        }

        public async Task<FacturaPreviewResponseDto> PreviewAsync(
            CreateFacturaDto dto)
        {
            var result = await PrepareFacturaDetailsAsync(dto);

            List<DetalleFacturaResponseDto> details =
                MapDetailsToResponse(result.Details);

            return new FacturaPreviewResponseDto
            {
                ClienteId = result.Cliente.ClienteId,
                Observaciones = dto.Observaciones,
                Total = details.Sum(d => d.Subtotal),
                Details = details
            };
        }

        public async Task<FacturaResponseDto> CreateAsync(
            CreateFacturaDto dto)
        {
            var result = await PrepareFacturaDetailsAsync(dto);

            List<MovimientoStock> movimientos = new();

            foreach (DetalleFactura detalle in result.Details)
            {
                MovimientoStock movimiento = new()
                {
                    AutoparteId = detalle.AutoparteId,
                    TipoMovimiento = "S",
                    Cantidad = detalle.Cantidad,
                    FechaMovimiento = result.SaleDate,
                    Observaciones = "Venta de autoparte"
                };

                movimientos.Add(movimiento);
            }

            Factura factura = new()
            {
                ClienteId = result.Cliente.ClienteId,
                FechaFactura = result.SaleDate,
                Estado = "Emitida",
                Observaciones = dto.Observaciones,
                DetalleFacturas = result.Details,
                MovimientoStocks = movimientos
            };

            await _facturaRepository.CreateAsync(factura);

            return MapToResponse(factura);
        }

        private async Task<(
            Cliente Cliente,
            List<DetalleFactura> Details,
            DateTime SaleDate)> PrepareFacturaDetailsAsync(
                CreateFacturaDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (dto.ClienteId <= 0)
            {
                throw new ArgumentException(
                    "El cliente seleccionado no es válido");
            }

            if (dto.Details == null || !dto.Details.Any())
            {
                throw new ArgumentException(
                    "La factura debe contener al menos una autoparte");
            }

            if (dto.Details.GroupBy(d => d.AutoparteId)
                .Any(g => g.Count() > 1))
            {
                throw new ArgumentException(
                    "No se permiten autopartes repetidas en los detalles de la factura");
            }

            if (dto.Details.Any(d =>
                d.AutoparteId <= 0 || d.Cantidad <= 0))
            {
                throw new ArgumentException(
                    "Las autopartes y las cantidades deben ser válidas");
            }

            Cliente? cliente = await _facturaRepository
                .GetActiveClienteByIdAsync(dto.ClienteId);

            if (cliente == null)
            {
                throw new KeyNotFoundException(
                    "El cliente no existe o está inactivo");
            }

            DateTime saleDate = DateTime.UtcNow;
            DateOnly saleDateOnly = DateOnly.FromDateTime(saleDate);

            List<DetalleFactura> details = new();

            foreach (CreateDetalleFacturaDto item in dto.Details)
            {
                Autoparte? autoparte = await _facturaRepository
                    .GetAutoparteByIdAsync(item.AutoparteId);

                if (autoparte == null)
                {
                    throw new KeyNotFoundException(
                        $"La autoparte con ID {item.AutoparteId} no existe o está inactiva");
                }

                HistorialPrecio? historialPrecio =
                    await _facturaRepository.GetCurrentPriceAsync(
                        item.AutoparteId);

                if (historialPrecio == null)
                {
                    throw new InvalidOperationException(
                        $"La autoparte {autoparte.Nombre} no tiene un precio vigente");
                }

                decimal discountPercentage = 0;

                if (cliente.TipoClienteId == 4)
                {
                    List<DescuentoMayoristum> discounts =
                        await _facturaRepository.GetApplicableDiscountsAsync(
                            item.AutoparteId,
                            item.Cantidad,
                            saleDateOnly);

                    if (discounts.Any())
                    {
                        discountPercentage =
                            discounts.First().PorcentajeDescuento;
                    }
                }

                DetalleFactura detalle = new()
                {
                    AutoparteId = item.AutoparteId,
                    Autoparte = autoparte,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = historialPrecio.Precio,
                    PorcentajeDescuento = discountPercentage
                };

                details.Add(detalle);
            }

            return (cliente, details, saleDate);
        }

        public async Task<List<FacturaResponseDto>> GetAllAsync()
        {
            List<Factura> facturas =
                await _facturaRepository.GetAllAsync();

            return facturas
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<FacturaResponseDto?> GetByIdAsync(int facturaId)
        {
            if (facturaId <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la factura no es válido");
            }

            Factura? factura =
                await _facturaRepository.GetByIdAsync(facturaId);

            if (factura == null)
            {
                return null;
            }

            return MapToResponse(factura);
        }

        public async Task<List<FacturaResponseDto>> GetByClienteIdAsync(
            int clienteId)
        {
            if (clienteId <= 0)
            {
                throw new ArgumentException(
                    "El identificador del cliente no es válido");
            }

            List<Factura> facturas =
                await _facturaRepository.GetByClienteIdAsync(clienteId);

            return facturas
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<List<FacturaResponseDto>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate)
        {
            if (startDate.Date > endDate.Date)
            {
                throw new ArgumentException(
                    "La fecha inicial no puede ser posterior a la fecha final");
            }

            TimeZoneInfo argentinaTimeZone;

            try
            {
                argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
                    "Argentina Standard Time"
                );
            }
            catch (TimeZoneNotFoundException)
            {
                argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
                    "America/Argentina/Buenos_Aires"
                );
            }

            DateTime localStartDate = DateTime.SpecifyKind(
                startDate.Date,
                DateTimeKind.Unspecified
            );

            DateTime localEndDate = DateTime.SpecifyKind(
                endDate.Date.AddDays(1),
                DateTimeKind.Unspecified
            );

            DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(
                localStartDate,
                argentinaTimeZone
            );

            DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(
                localEndDate,
                argentinaTimeZone
            );

            List<Factura> facturas =
                await _facturaRepository.GetByDateRangeAsync(
                    utcStartDate,
                    utcEndDate
                );

            return facturas
                .Select(MapToResponse)
                .ToList();
        }

        public async Task CancelAsync(int facturaId)
        {
            if (facturaId <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la factura no es válido");
            }

            await _facturaRepository.CancelAsync(facturaId);
        }

        private static List<DetalleFacturaResponseDto> MapDetailsToResponse(
            ICollection<DetalleFactura> details)
        {
            return details.Select(d =>
                new DetalleFacturaResponseDto
                {
                    AutoparteId = d.AutoparteId,
                    Codigo = d.Autoparte.Codigo,
                    Nombre = d.Autoparte.Nombre,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    PorcentajeDescuento = d.PorcentajeDescuento,
                    Subtotal = Math.Round(
                        d.Cantidad *
                        d.PrecioUnitario *
                        (1 - d.PorcentajeDescuento / 100),
                        2)
                }).ToList();
        }

        private static FacturaResponseDto MapToResponse(Factura factura)
        {
            List<DetalleFacturaResponseDto> details =
                MapDetailsToResponse(factura.DetalleFacturas);

            DateTime fechaUtc = DateTime.SpecifyKind(
                factura.FechaFactura,
                DateTimeKind.Utc
            );

            return new FacturaResponseDto
            {
                FacturaId = factura.FacturaId,
                ClienteId = factura.ClienteId,
                NumeroFactura = factura.NumeroFactura,
                FechaFactura = fechaUtc,
                Estado = factura.Estado,
                Observaciones = factura.Observaciones,
                Total = details.Sum(d => d.Subtotal),
                Details = details
            };
        }
    }
}
