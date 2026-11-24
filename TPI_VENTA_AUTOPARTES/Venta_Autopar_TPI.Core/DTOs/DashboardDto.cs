using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venta_Autopar_TPI.Core.DTOs
{
    public class StockCriticoResponseDto
    {
        public int AutoparteId { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public int StockActual { get; set; }

        public int StockMinimo { get; set; }

        public int UnidadesFaltantes { get; set; }
    }
    public class FacturacionTotalResponseDto
    {
        public decimal TotalFacturado { get; set; }
    }

    public class AutoparteMasVendidaResponseDto
    {
        public int AutoparteId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
    }
}

