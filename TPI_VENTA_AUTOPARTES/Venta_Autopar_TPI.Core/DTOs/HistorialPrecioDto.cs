using System;

namespace Venta_Autopar_TPI.Core.DTOs
{
    public class HistorialPrecioDto
    {
        public decimal Precio { get; set; }

        public DateTime FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }
    }
}
