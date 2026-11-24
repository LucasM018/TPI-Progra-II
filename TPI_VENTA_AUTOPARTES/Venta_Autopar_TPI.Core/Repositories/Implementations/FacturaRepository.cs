using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Implementations
{
    public class FacturaRepository : IFacturaRepository
    {
        private readonly AutopartesContext _context;

        public FacturaRepository(AutopartesContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> GetActiveClienteByIdAsync(int clienteId)
        {
            return await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.ClienteId == clienteId && c.Estado);
        }

        public async Task<Autoparte?> GetAutoparteByIdAsync(int autoparteId)
        {
            return await _context.Autopartes
                .AsNoTracking()
                .FirstOrDefaultAsync(a =>
                    a.AutoparteId == autoparteId && a.Estado);
        }

        public async Task<HistorialPrecio?> GetCurrentPriceAsync(int autoparteId)
        {
            DateTime currentDate = DateTime.UtcNow;

            return await _context.HistorialPrecios
                .AsNoTracking()
                .Where(p =>
                    p.AutoparteId == autoparteId &&
                    p.FechaDesde <= currentDate &&
                    (p.FechaHasta == null || p.FechaHasta >= currentDate))
                .OrderByDescending(p => p.FechaDesde)
                .FirstOrDefaultAsync();
        }

        public async Task<List<DescuentoMayoristum>> GetApplicableDiscountsAsync(
            int autoparteId,
            int cantidad,
            DateOnly saleDate)
        {
            return await _context.DescuentoMayorista
                .AsNoTracking()
                .Where(d =>
                    d.Estado &&
                    (d.AutoparteId == null || d.AutoparteId == autoparteId) &&
                    d.CantidadMinima <= cantidad &&
                    (d.CantidadMaxima == null || d.CantidadMaxima >= cantidad) &&
                    d.FechaDesde <= saleDate &&
                    (d.FechaHasta == null || d.FechaHasta >= saleDate))
                .OrderByDescending(d => d.AutoparteId == autoparteId)
                .ThenByDescending(d => d.FechaDesde)
                .ThenByDescending(d => d.CantidadMinima)
                .ToListAsync();
        }

        public async Task<Inventario?> GetInventarioByAutoparteIdAsync(
            int autoparteId)
        {
            return await _context.Inventarios
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.AutoparteId == autoparteId);
        }

        public async Task<List<Factura>> GetAllAsync()
        {
            return await _context.Facturas
                .AsNoTracking()
                .Include(f => f.Cliente)
                .Include(f => f.DetalleFacturas)
                    .ThenInclude(d => d.Autoparte)
                .OrderByDescending(f => f.FechaFactura)
                .ToListAsync();
        }

        public async Task<Factura?> GetByIdAsync(int facturaId)
        {
            return await _context.Facturas
                .AsNoTracking()
                .Include(f => f.Cliente)
                .Include(f => f.DetalleFacturas)
                    .ThenInclude(d => d.Autoparte)
                .FirstOrDefaultAsync(f => f.FacturaId == facturaId);
        }

        public async Task<List<Factura>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.Facturas
                .AsNoTracking()
                .Include(f => f.Cliente)
                .Include(f => f.DetalleFacturas)
                    .ThenInclude(d => d.Autoparte)
                .Where(f => f.ClienteId == clienteId)
                .OrderByDescending(f => f.FechaFactura)
                .ToListAsync();
        }

        public async Task<List<Factura>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate)
        {
            return await _context.Facturas
                .AsNoTracking()
                .Include(f => f.Cliente)
                .Include(f => f.DetalleFacturas)
                    .ThenInclude(d => d.Autoparte)
                .Where(f =>
                    f.FechaFactura >= startDate &&
                    f.FechaFactura < endDate)
                .OrderByDescending(f => f.FechaFactura)
                .ToListAsync();
        }

        public async Task CreateAsync(Factura factura)
        {
            if (factura == null)
            {
                throw new ArgumentNullException(nameof(factura));
            }

            if (factura.DetalleFacturas == null ||
                !factura.DetalleFacturas.Any())
            {
                throw new ArgumentException(
                    "La factura debe contener al menos un detalle.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (DetalleFactura detalle in
                    factura.DetalleFacturas.OrderBy(d => d.AutoparteId))
                {
                    int affectedRows = await _context.Inventarios
                        .Where(i =>
                            i.AutoparteId == detalle.AutoparteId &&
                            i.StockActual >= detalle.Cantidad)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(
                                i => i.StockActual,
                                i => i.StockActual - detalle.Cantidad)
                            .SetProperty(
                                i => i.UltimaActualizacion,
                                factura.FechaFactura));

                    if (affectedRows != 1)
                    {
                        throw new InvalidOperationException(
                            $"No hay stock suficiente para la autoparte ID {detalle.AutoparteId}.");
                    }
                }

                factura.NumeroFactura =
                    "T" + Guid.NewGuid().ToString("N")[..29];

                factura.Estado = "Emitida";

                foreach (DetalleFactura detalle in factura.DetalleFacturas)
                {
                    if (detalle.Autoparte != null)
                    {
                        _context.Entry(detalle.Autoparte).State =
                            EntityState.Unchanged;
                    }
                }

                _context.Facturas.Add(factura);

                await _context.SaveChangesAsync();

                factura.NumeroFactura = $"FAC-{factura.FacturaId:D6}";

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelAsync(int facturaId)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                Factura? factura = await _context.Facturas
                    .AsNoTracking()
                    .Include(f => f.DetalleFacturas)
                    .FirstOrDefaultAsync(f => f.FacturaId == facturaId);

                if (factura == null)
                {
                    throw new KeyNotFoundException(
                        $"No existe la factura con ID {facturaId}.");
                }

                int affectedRows = await _context.Facturas
                    .Where(f =>
                        f.FacturaId == facturaId &&
                        f.Estado == "Emitida")
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(f => f.Estado, "Anulada"));

                if (affectedRows != 1)
                {
                    throw new InvalidOperationException(
                        "La factura ya se encuentra anulada o no puede anularse.");
                }

                DateTime cancellationDate = DateTime.UtcNow;

                foreach (DetalleFactura detalle in
                    factura.DetalleFacturas.OrderBy(d => d.AutoparteId))
                {
                    int affectedInventoryRows = await _context.Inventarios
                        .Where(i => i.AutoparteId == detalle.AutoparteId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(
                                i => i.StockActual,
                                i => i.StockActual + detalle.Cantidad)
                            .SetProperty(
                                i => i.UltimaActualizacion,
                                cancellationDate));

                    if (affectedInventoryRows != 1)
                    {
                        throw new InvalidOperationException(
                            $"No se pudo restituir el inventario de la autoparte ID {detalle.AutoparteId}.");
                    }

                    MovimientoStock movement = new MovimientoStock
                    {
                        AutoparteId = detalle.AutoparteId,
                        TipoMovimiento = "E",
                        Cantidad = detalle.Cantidad,
                        FechaMovimiento = cancellationDate,
                        FacturaId = facturaId,
                        Observaciones =
                            $"Anulación de factura {factura.NumeroFactura}"
                    };

                    _context.MovimientoStocks.Add(movement);
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
    }
}
