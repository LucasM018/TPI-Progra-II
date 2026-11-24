using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.DTOs;

namespace Venta_Autopar_TPI.Core.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<List<StockCriticoResponseDto>> GetStockCriticoAsync();

        Task<FacturacionTotalResponseDto> GetFacturacionDiariaAsync();

        Task<FacturacionTotalResponseDto> GetFacturacionMensualAsync();

        Task<FacturacionTotalResponseDto> GetFacturacionPorPeriodoAsync(
            DateOnly startDate,
            DateOnly endDate);

        Task<List<AutoparteMasVendidaResponseDto>> GetAutopartesMasVendidasAsync(int limit);
    }
}