using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.DTOs;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Core.Services.Interfaces;

namespace Venta_Autopar_TPI.Core.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        private static readonly TimeZoneInfo ArgentinaTimeZone =
            GetArgentinaTimeZone();

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<List<StockCriticoResponseDto>> GetStockCriticoAsync()
        {
            var inventarios = await _dashboardRepository.GetStockCriticoAsync();

            return inventarios.Select(i => new StockCriticoResponseDto
            {
                AutoparteId = i.AutoparteId,
                Codigo = i.Autoparte.Codigo,
                Nombre = i.Autoparte.Nombre,
                StockActual = i.StockActual,
                StockMinimo = i.StockMinimo,
                UnidadesFaltantes = i.StockMinimo - i.StockActual
            }).ToList();
        }

        public async Task<FacturacionTotalResponseDto> GetFacturacionDiariaAsync()
        {
            DateTime argentinaNow = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                ArgentinaTimeZone);

            DateOnly today = DateOnly.FromDateTime(argentinaNow);

            DateTime startDate = today.ToDateTime(TimeOnly.MinValue);
            DateTime endDate = today.AddDays(1).ToDateTime(TimeOnly.MinValue);

            return await CalculateFacturacionAsync(startDate, endDate);
        }

        public async Task<FacturacionTotalResponseDto> GetFacturacionMensualAsync()
        {
            DateTime argentinaNow = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                ArgentinaTimeZone);

            DateOnly today = DateOnly.FromDateTime(argentinaNow);

            DateOnly firstDayOfMonth = new DateOnly(
                today.Year,
                today.Month,
                1);

            DateOnly firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            DateTime startDate = firstDayOfMonth.ToDateTime(TimeOnly.MinValue);
            DateTime endDate = firstDayOfNextMonth.ToDateTime(TimeOnly.MinValue);

            return await CalculateFacturacionAsync(startDate, endDate);
        }

        public async Task<FacturacionTotalResponseDto> GetFacturacionPorPeriodoAsync(
            DateOnly startDate,
            DateOnly endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException(
                    "La fecha inicial no puede ser posterior a la fecha final");
            }

            DateTime startDateTime = startDate.ToDateTime(TimeOnly.MinValue);

            DateTime endDateTime = endDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            return await CalculateFacturacionAsync(
                startDateTime,
                endDateTime);
        }

        public async Task<List<AutoparteMasVendidaResponseDto>> GetAutopartesMasVendidasAsync(
            int limit)
        {
            if (limit <= 0)
            {
                throw new ArgumentException(
                    "El límite debe ser mayor que cero",
                    nameof(limit));
            }

            var detalles = await _dashboardRepository
                .GetDetallesFacturasEmitidasAsync();

            return detalles
                .GroupBy(d => new
                {
                    d.AutoparteId,
                    d.Autoparte.Codigo,
                    d.Autoparte.Nombre
                })
                .Select(g => new AutoparteMasVendidaResponseDto
                {
                    AutoparteId = g.Key.AutoparteId,
                    Codigo = g.Key.Codigo,
                    Nombre = g.Key.Nombre,
                    CantidadVendida = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(a => a.CantidadVendida)
                .ThenBy(a => a.Nombre)
                .Take(limit)
                .ToList();
        }

        private async Task<FacturacionTotalResponseDto> CalculateFacturacionAsync(
            DateTime startDate,
            DateTime endDate)
        {
            DateTime startDateUtc = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(startDate, DateTimeKind.Unspecified),
                ArgentinaTimeZone);

            DateTime endDateUtc = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(endDate, DateTimeKind.Unspecified),
                ArgentinaTimeZone);

            var facturas = await _dashboardRepository
                .GetFacturasEmitidasByDateRangeAsync(
                    startDateUtc,
                    endDateUtc);

            decimal total = facturas
                .SelectMany(f => f.DetalleFacturas)
                .Sum(d => Math.Round(
                    d.Cantidad *
                    d.PrecioUnitario *
                    (1m - d.PorcentajeDescuento / 100m),
                    2));

            return new FacturacionTotalResponseDto
            {
                TotalFacturado = total
            };
        }

        private static TimeZoneInfo GetArgentinaTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    "Argentina Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    "America/Argentina/Buenos_Aires");
            }
        }
    }
}