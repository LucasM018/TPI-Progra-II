
using System.Collections.Generic;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.DTOs;

namespace Venta_Autopar_TPI.Core.Services.Interfaces
{
    public interface IClienteService
    {
        Task<List<ClienteDto>> GetAllActiveAsync();
    }
}
