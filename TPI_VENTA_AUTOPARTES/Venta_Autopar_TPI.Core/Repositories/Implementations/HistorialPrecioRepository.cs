using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Implementations
{
    public class HistorialPrecioRepository : IHistorialPrecioRepository
    {
        private readonly AutopartesContext _context;

        public HistorialPrecioRepository(AutopartesContext context)
        {
            _context = context;
        }

        public async Task<bool> AddNewPriceAsync(int autoparteId, decimal price)
        {
            
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var autoparteExists = await _context.Autopartes
                    .AnyAsync(a => a.AutoparteId == autoparteId);

                if (!autoparteExists)
                    return false;

                var currentPrice = await _context.HistorialPrecios
                    .FirstOrDefaultAsync(h =>
                        h.AutoparteId == autoparteId &&
                        h.FechaHasta == null);

                if (currentPrice == null)
                    return false;

               
                currentPrice.FechaHasta = DateTime.Now;

                
                var newPrice = new HistorialPrecio
                {
                    AutoparteId = autoparteId,
                    Precio = price,
                    FechaDesde = DateTime.Now,
                    FechaHasta = null
                };

                await _context.HistorialPrecios.AddAsync(newPrice);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<HistorialPrecio>> GetByAutoparteIdAsync(int autoparteId)
        {
            return await _context.HistorialPrecios
                .AsNoTracking()
                .Where(h => h.AutoparteId == autoparteId)
                .OrderByDescending(h => h.FechaDesde)
                .ToListAsync();
        }

    }
}