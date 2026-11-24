using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Implementations
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AutopartesContext _context;

        public DashboardRepository(AutopartesContext context)
        {
            _context = context;
        }

        public async Task<List<Inventario>> GetStockCriticoAsync()
        {
            return await _context.Inventarios
                .AsNoTracking()
                .Include(i => i.Autoparte)
                .Where(i =>
                    i.StockActual <= i.StockMinimo &&
                    i.Autoparte.Estado)
                .OrderBy(i => i.StockActual)
                .ToListAsync();
        }

        public async Task<List<Factura>> GetFacturasEmitidasByDateRangeAsync(
            DateTime startDateUtc,
            DateTime endDateUtc)
        {
            return await _context.Facturas
                .AsNoTracking()
                .Where(f =>
                    f.Estado == "Emitida" &&
                    f.FechaFactura >= startDateUtc &&
                    f.FechaFactura < endDateUtc)
                .Include(f => f.DetalleFacturas)
                .ToListAsync();
        }

        public async Task<List<DetalleFactura>> GetDetallesFacturasEmitidasAsync()
        {
            return await _context.DetalleFacturas
                .AsNoTracking()
                .Where(d => d.Factura.Estado == "Emitida")
                .Include(d => d.Autoparte)
                .ToListAsync();
        }
    }
}