using System;

namespace Venta_Autopar_TPI.Core.DTOs
{
    public class MovimientoStockDto
    {
        public long MovimientoStockId { get; set; }

        public int AutoparteId { get; set; }

        public string TipoMovimiento { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public DateTime FechaMovimiento { get; set; }

        public int StockResultante { get; set; }

        public int? FacturaId { get; set; }

        public int? OrdenReposicionId { get; set; }

        public string? Observaciones { get; set; }
    }
}
