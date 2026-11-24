
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.DTOs;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Core.Services.Interfaces;

namespace Venta_Autopar_TPI.Core.Services.Implementations
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<List<ClienteDto>> GetAllActiveAsync()
        {
            var clientes = await _clienteRepository.GetAllActiveAsync();

            return clientes.Select(c => new ClienteDto
            {
                ClienteId = c.ClienteId,
                TipoClienteId = c.TipoClienteId,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                RazonSocial = c.RazonSocial,
                Cuit = c.Cuit,
                Dni = c.Dni
            }).ToList();
        }
    }
}
