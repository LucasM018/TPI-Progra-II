
using System.Collections.Generic;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Interfaces
{
    public interface IClienteRepository
    {
        
        Task<List<Cliente>> GetAllActiveAsync();
    }
}
