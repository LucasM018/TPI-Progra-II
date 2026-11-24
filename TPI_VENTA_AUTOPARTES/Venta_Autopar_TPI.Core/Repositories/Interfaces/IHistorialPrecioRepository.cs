using System.Collections.Generic;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Interfaces
{
    public interface IHistorialPrecioRepository
    {
        Task<bool> AddNewPriceAsync(int autoparteId, decimal price);

        Task<List<HistorialPrecio>> GetByAutoparteIdAsync(int autoparteId);
    }
}
