
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Interfaces
{
    public interface IFacturaRepository
    {
      
        Task<Cliente?> GetActiveClienteByIdAsync(int clienteId);

        
        Task<Autoparte?> GetAutoparteByIdAsync(int autoparteId);

        
        Task<HistorialPrecio?> GetCurrentPriceAsync(int autoparteId);

        
        Task<List<DescuentoMayoristum>> GetApplicableDiscountsAsync(
            int autoparteId,
            int cantidad,
            DateOnly saleDate);

        
        Task<Inventario?> GetInventarioByAutoparteIdAsync(int autoparteId);

        
        Task<List<Factura>> GetAllAsync();

        
        Task<Factura?> GetByIdAsync(int facturaId);

        
        Task<List<Factura>> GetByClienteIdAsync(int clienteId);

        Task<List<Factura>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate);

        Task CreateAsync(Factura factura);

        Task CancelAsync(int facturaId);
    }
}

