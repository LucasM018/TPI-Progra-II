using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<List<Inventario>> GetStockCriticoAsync();

        Task<List<Factura>> GetFacturasEmitidasByDateRangeAsync(
            DateTime startDateUtc,
            DateTime endDateUtc);

        Task<List<DetalleFactura>> GetDetallesFacturasEmitidasAsync();
    }
}