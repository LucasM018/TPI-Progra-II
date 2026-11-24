using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AutopartesContext _context;

        public AuthRepository(AutopartesContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rols)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario?> GetByIdAsync(int usuarioId)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rols)
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
        }

        public async Task<RefreshToken?> GetRefreshTokenByHashAsync(
            string tokenHash)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.Usuario)
                .ThenInclude(u => u.Rols)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }

        public async Task CreateRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task InvalidateRefreshTokenAsync(
            long refreshTokenId,
            DateTime revokedAt)
        {
            await _context.RefreshTokens
                .Where(rt =>
                    rt.RefreshTokenId == refreshTokenId &&
                    rt.FechaRevocacion == null)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(
                        rt => rt.FechaRevocacion,
                        revokedAt));
        }

        public async Task UpdateLastAccessAsync(
            int usuarioId,
            DateTime lastAccess)
        {
            await _context.Usuarios
                .Where(u => u.UsuarioId == usuarioId)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(
                        u => u.UltimoAcceso,
                        lastAccess));
        }
    }
}
