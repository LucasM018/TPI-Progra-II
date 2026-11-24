using System;

namespace Venta_Autopar_TPI.Core.DTOs
{
    public class UpdateAutoparteDto
    {
        public int CategoriaAutoparteId { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string? Marca { get; set; }

        public string? AplicacionModelo { get; set; }
    }

    public class CreateAutoparteDto
    {
        public int CategoriaAutoparteId { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string? Marca { get; set; }

        public string? AplicacionModelo { get; set; }

        
        public int StockInicial { get; set; }

        public int StockMinimo { get; set; }

        
        public decimal PrecioInicial { get; set; }
    }

    public class AutoparteResponseDto
    {
        public int AutoparteId { get; set; }

        public int CategoriaAutoparteId { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string? Marca { get; set; }

        public string? AplicacionModelo { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaAlta { get; set; }

        
        public int StockActual { get; set; }

        public int StockMinimo { get; set; }

        
        public decimal PrecioActual { get; set; }
    }
}
