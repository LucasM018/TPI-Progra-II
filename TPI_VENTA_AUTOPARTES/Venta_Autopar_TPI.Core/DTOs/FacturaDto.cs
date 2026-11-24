using System;
using System.Collections.Generic;

namespace Venta_Autopar_TPI.Core.DTOs
{
    public class CreateDetalleFacturaDto
    {
        public int AutoparteId { get; set; }

        public int Cantidad { get; set; }
    }

    public class CreateFacturaDto
    {
        public int ClienteId { get; set; }

        public string? Observaciones { get; set; }

        public List<CreateDetalleFacturaDto> Details { get; set; } = new();
    }

    public class DetalleFacturaResponseDto
    {
        public int AutoparteId { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal PorcentajeDescuento { get; set; }

        public decimal Subtotal { get; set; }
    }

    public class FacturaResponseDto
    {
        public int FacturaId { get; set; }

        public int ClienteId { get; set; }

        public string NumeroFactura { get; set; } = string.Empty;

        public DateTime FechaFactura { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string? Observaciones { get; set; }

        public decimal Total { get; set; }

        public List<DetalleFacturaResponseDto> Details { get; set; } = new();
    }

    
    public class FacturaPreviewResponseDto
    {
        public int ClienteId { get; set; }

        public string? Observaciones { get; set; }

        public decimal Total { get; set; }

        public List<DetalleFacturaResponseDto> Details { get; set; } = new();
    }
}
