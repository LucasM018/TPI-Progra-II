
using System.Collections.Generic;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.DTOs;

namespace Venta_Autopar_TPI.Core.Services.Interfaces
{
    public interface IAutoparteService
    {
        Task<List<AutoparteResponseDto>> GetAllAsync();

        Task<AutoparteResponseDto?> GetByIdAsync(int id);

        Task CreateAsync(CreateAutoparteDto autoparteDto);

        Task UpdateAsync(int id, UpdateAutoparteDto autoparteDto);

        Task<bool> ChangeStatusAsync(int id, bool status);

        Task<bool> AddStockAsync(int autoparteId, StockDto stockDto);

        Task<bool> RemoveStockAsync(int autoparteId, StockDto stockDto);

        Task<bool> UpdatePriceAsync(int autoparteId, PrecioDto precioDto);
        Task<List<HistorialPrecioDto>> GetPriceHistoryAsync(int autoparteId);
    }
}
