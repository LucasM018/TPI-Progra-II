using Microsoft.EntityFrameworkCore;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Implementations
{
    public class AutoparteRepository : IAutoparteRepository
    {
        private readonly AutopartesContext _context;

        public AutoparteRepository(AutopartesContext context)
        {
            _context = context;
        }

        
        public async Task<List<Autoparte>> GetAllAsync()
        {
            return await _context.Autopartes
                .AsNoTracking()
                .Include(a => a.Inventario)
                .Include(a => a.HistorialPrecios
                    .Where(h => h.FechaHasta == null))
                .ToListAsync();
        }

        
        public async Task<Autoparte?> GetByIdAsync(int id)
        {
            return await _context.Autopartes
                .AsNoTracking()
                .Include(a => a.Inventario)
                .Include(a => a.HistorialPrecios
                    .Where(h => h.FechaHasta == null))
                .FirstOrDefaultAsync(a => a.AutoparteId == id);
        }

        
        public async Task<Autoparte?> GetByCodeAsync(string code)
        {
            return await _context.Autopartes
                .FirstOrDefaultAsync(a => a.Codigo == code);
        }

        
        public async Task<bool> CodeExistsAsync(
            string code,
            int? excludeAutoparteId = null)
        {
            return await _context.Autopartes.AnyAsync(a =>
                a.Codigo == code &&
                (!excludeAutoparteId.HasValue ||
                 a.AutoparteId != excludeAutoparteId.Value));
        }

        
        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.CategoriaAutopartes
                .AnyAsync(c => c.CategoriaAutoparteId == categoryId);
        }

        
        public async Task AddAsync(Autoparte autoparte)
        {
            await _context.Autopartes.AddAsync(autoparte);
            await _context.SaveChangesAsync();
        }

        //
        public async Task AddWithInitialDataAsync(
            Autoparte autoparte,
            Inventario inventario,
            HistorialPrecio historialPrecio,
            MovimientoStock? movimientoStock)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                
                await _context.Autopartes.AddAsync(autoparte);
                await _context.SaveChangesAsync();

               
                inventario.AutoparteId = autoparte.AutoparteId;
                historialPrecio.AutoparteId = autoparte.AutoparteId;

                await _context.Inventarios.AddAsync(inventario);
                await _context.HistorialPrecios.AddAsync(historialPrecio);

                
                if (movimientoStock != null)
                {
                    movimientoStock.AutoparteId = autoparte.AutoparteId;
                    await _context.MovimientoStocks.AddAsync(movimientoStock);
                }

                await _context.SaveChangesAsync();

               
                await transaction.CommitAsync();
            }
            catch
            {
                
                await transaction.RollbackAsync();
                throw;
            }
        }

        
        public async Task<bool> AddStockAsync(
            int autoparteId,
            int quantity,
            string observations)
        {
            if (quantity <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var affectedRows = await _context.Inventarios
                    .Where(i => i.AutoparteId == autoparteId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(i => i.StockActual, i => i.StockActual + quantity)
                        .SetProperty(i => i.UltimaActualizacion, DateTime.Now));

                if (affectedRows == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                var movimiento = new MovimientoStock
                {
                    AutoparteId = autoparteId,
                    TipoMovimiento = "E",
                    Cantidad = quantity,
                    FechaMovimiento = DateTime.Now,
                    Observaciones = observations
                };

                await _context.MovimientoStocks.AddAsync(movimiento);
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

        
        public async Task<bool> RemoveStockAsync(
            int autoparteId,
            int quantity,
            string observations)
        {
            if (quantity <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                
                var affectedRows = await _context.Inventarios
                    .Where(i =>
                        i.AutoparteId == autoparteId &&
                        i.StockActual >= quantity)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(i => i.StockActual, i => i.StockActual - quantity)
                        .SetProperty(i => i.UltimaActualizacion, DateTime.Now));

                if (affectedRows == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                var movimiento = new MovimientoStock
                {
                    AutoparteId = autoparteId,
                    TipoMovimiento = "S",
                    Cantidad = quantity,
                    FechaMovimiento = DateTime.Now,
                    Observaciones = observations
                };

                await _context.MovimientoStocks.AddAsync(movimiento);
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

        
        public async Task UpdateAsync(Autoparte autoparte)
        {
            _context.Autopartes.Update(autoparte);
            await _context.SaveChangesAsync();
        }

        
        public async Task<bool> ChangeStatusAsync(int id, bool status)
        {
            var autoparte = await _context.Autopartes
                .FirstOrDefaultAsync(a => a.AutoparteId == id);

            if (autoparte == null)
                return false;

            autoparte.Estado = status;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
