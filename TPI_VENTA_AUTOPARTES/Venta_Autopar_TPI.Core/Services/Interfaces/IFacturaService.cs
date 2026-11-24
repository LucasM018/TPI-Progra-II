using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.DTOs;

namespace Venta_Autopar_TPI.Core.Services.Interfaces
{
    public interface IFacturaService
    {
        Task<FacturaPreviewResponseDto> PreviewAsync(CreateFacturaDto dto);

        Task<FacturaResponseDto> CreateAsync(CreateFacturaDto dto);

        Task<List<FacturaResponseDto>> GetAllAsync();

        Task<FacturaResponseDto?> GetByIdAsync(int facturaId);

        Task<List<FacturaResponseDto>> GetByClienteIdAsync(int clienteId);

        Task<List<FacturaResponseDto>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate);

        Task CancelAsync(int facturaId);
    }
}
