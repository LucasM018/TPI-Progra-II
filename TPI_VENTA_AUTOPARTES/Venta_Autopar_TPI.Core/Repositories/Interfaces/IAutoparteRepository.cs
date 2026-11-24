using System.Collections.Generic;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Interfaces
{
    public interface IAutoparteRepository
    {
       
        Task<List<Autoparte>> GetAllAsync();
        Task<Autoparte?> GetByIdAsync(int id);
        Task<Autoparte?> GetByCodeAsync(string code);
        Task<bool> CodeExistsAsync(
            string code,
            int? excludeAutoparteId = null
        );

        Task<bool> CategoryExistsAsync(int categoryId);

        Task AddAsync(Autoparte autoparte);

        Task AddWithInitialDataAsync(
            Autoparte autoparte,
            Inventario inventario,
            HistorialPrecio historialPrecio,
            MovimientoStock? movimientoStock
        );

        Task<bool> AddStockAsync(
            int autoparteId,
            int quantity,
            string observations
        );

        Task<bool> RemoveStockAsync(
            int autoparteId,
            int quantity,
            string observations
        );

        Task UpdateAsync(Autoparte autoparte);

        Task<bool> ChangeStatusAsync(int id, bool status);
    }
}

