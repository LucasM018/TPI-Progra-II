
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Implementations
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AutopartesContext _context;

        public ClienteRepository(AutopartesContext context)
        {
            _context = context;
        }

        
        public async Task<List<Cliente>> GetAllActiveAsync()
        {
            return await _context.Clientes
                .AsNoTracking()
                .Where(c => c.Estado)
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .ThenBy(c => c.RazonSocial)
                .ToListAsync();
        }
    }
}
