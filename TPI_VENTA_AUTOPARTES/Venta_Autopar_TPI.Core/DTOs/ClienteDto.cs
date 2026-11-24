namespace Venta_Autopar_TPI.Core.DTOs
{
    public class ClienteDto
    {
        public int ClienteId { get; set; }

        public int TipoClienteId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string RazonSocial { get; set; } = string.Empty;

        public string Cuit { get; set; } = string.Empty;

        public string Dni { get; set; } = string.Empty;
    }
}