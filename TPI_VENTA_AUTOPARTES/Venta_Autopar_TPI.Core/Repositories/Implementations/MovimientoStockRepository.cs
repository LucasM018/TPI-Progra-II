
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Implementations
{
    public class MovimientoStockRepository : IMovimientoStockRepository
    {
        private readonly AutopartesContext _context;

        public MovimientoStockRepository(AutopartesContext context)
        {
            _context = context;
        }

        
        public async Task<List<MovimientoStock>> GetByAutoparteIdAsync(
            int autoparteId)
        {
            return await _context.MovimientoStocks
                .AsNoTracking()
                .Where(m => m.AutoparteId == autoparteId)
                .OrderByDescending(m => m.FechaMovimiento)
                .ThenByDescending(m => m.MovimientoStockId)
                .ToListAsync();
        }

  
        public async Task<int?> GetCurrentStockAsync(int autoparteId)
        {
            return await _context.Inventarios
                .AsNoTracking()
                .Where(i => i.AutoparteId == autoparteId)
                .Select(i => (int?)i.StockActual)
                .FirstOrDefaultAsync();
        }
    }
}
