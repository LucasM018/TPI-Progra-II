using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.DTOs;

namespace Venta_Autopar_TPI.Core.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);

        Task<AuthResponseDto> RefreshTokenAsync(
            RefreshTokenRequestDto dto);

        Task LogoutAsync(RefreshTokenRequestDto dto);
    }
}