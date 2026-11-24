using System;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<Usuario?> GetByEmailAsync(string email);

        Task<Usuario?> GetByIdAsync(int usuarioId);

        Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash);

        Task CreateRefreshTokenAsync(RefreshToken refreshToken);

        Task InvalidateRefreshTokenAsync(
            long refreshTokenId,
            DateTime revokedAt);

        Task UpdateLastAccessAsync(
            int usuarioId,
            DateTime lastAccess);
    }
}