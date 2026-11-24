using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Venta_Autopar_TPI.Core.DTOs;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Core.Services.Interfaces;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IAuthRepository authRepository,
            IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
           
            var user = await _authRepository.GetByEmailAsync(dto.Email.Trim());

            if (user == null || !user.Activo)
            {
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }
            
            if (!PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            var now = DateTime.UtcNow;

            await _authRepository.UpdateLastAccessAsync(user.UsuarioId, now);

            return await GenerateAuthResponseAsync(user, now);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var tokenHash = HashRefreshToken(dto.RefreshToken);

            var storedToken = await _authRepository.GetRefreshTokenByHashAsync(tokenHash);

            var now = DateTime.UtcNow;

            if (storedToken == null ||
                storedToken.FechaRevocacion != null ||
                storedToken.FechaExpiracion <= now ||
                !storedToken.Usuario.Activo)
            {
                throw new UnauthorizedAccessException("Refresh Token inválido o vencido.");
            }

            await _authRepository.InvalidateRefreshTokenAsync(
                storedToken.RefreshTokenId,
                now);

            return await GenerateAuthResponseAsync(storedToken.Usuario, now);
        }

        public async Task LogoutAsync(RefreshTokenRequestDto dto)
        {
            var tokenHash = HashRefreshToken(dto.RefreshToken);

            var storedToken = await _authRepository.GetRefreshTokenByHashAsync(tokenHash);

            if (storedToken == null || storedToken.FechaRevocacion != null)
            {
                throw new UnauthorizedAccessException("Refresh Token inválido.");
            }

            await _authRepository.InvalidateRefreshTokenAsync(
                storedToken.RefreshTokenId,
                DateTime.UtcNow);
        }

        private async Task<AuthResponseDto> GenerateAuthResponseAsync(
            Usuario user,
            DateTime now)
        {
            var accessTokenExpiration = GetAccessTokenExpiration();
            var refreshTokenExpiration = GetRefreshTokenExpiration();

            var accessToken = GenerateAccessToken(
                user,
                now,
                accessTokenExpiration);

            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UsuarioId = user.UsuarioId,
                TokenHash = HashRefreshToken(refreshToken),
                FechaCreacion = now,
                FechaExpiracion = now.AddDays(refreshTokenExpiration)
            };

            await _authRepository.CreateRefreshTokenAsync(refreshTokenEntity);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = accessTokenExpiration,
                UsuarioId = user.UsuarioId,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email,
                Roles = user.Rols.Select(r => r.Nombre).ToList()
            };
        }

        private string GenerateAccessToken(
            Usuario user,
            DateTime now,
            DateTime expiration)
        {
            var secretKey = _configuration["Jwt:SecretKey"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException(
                    "No se configuró la clave secreta de JWT.");
            }

            var keyBytes = Convert.FromBase64String(secretKey);

            var securityKey = new SymmetricSecurityKey(keyBytes);

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UsuarioId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.Nombre),
                new Claim(ClaimTypes.Surname, user.Apellido)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims.Concat(
                    user.Rols.Select(r => new Claim(ClaimTypes.Role, r.Nombre))),
                notBefore: now,
                expires: expiration,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        private string HashRefreshToken(string refreshToken)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(refreshToken);

            var hashBytes = SHA256.HashData(tokenBytes);

            return Convert.ToBase64String(hashBytes);
        }

        private DateTime GetAccessTokenExpiration()
        {
            var minutes = int.Parse(
                _configuration["Jwt:AccessTokenExpirationMinutes"]
                ?? throw new InvalidOperationException(
                    "Falta configurar AccessTokenExpirationMinutes."));

            if (minutes <= 0)
            {
                throw new InvalidOperationException(
                    "La duración del Access Token debe ser mayor a cero.");
            }

            return DateTime.UtcNow.AddMinutes(minutes);
        }

        private int GetRefreshTokenExpiration()
        {
            var days = int.Parse(
                _configuration["Jwt:RefreshTokenExpirationDays"]
                ?? throw new InvalidOperationException(
                    "Falta configurar RefreshTokenExpirationDays."));

            if (days <= 0)
            {
                throw new InvalidOperationException(
                    "La duración del Refresh Token debe ser mayor a cero.");
            }

            return days;
        }
    }
}