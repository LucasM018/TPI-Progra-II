using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Venta_Autopar_TPI.Core.DTOs;
using Venta_Autopar_TPI.Core.Services.Interfaces;

namespace Venta_Autopar_TPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto dto)
        {
            try
            {
                var response = await _authService.LoginAsync(dto);

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenRequestDto dto)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(dto);

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
            [FromBody] RefreshTokenRequestDto dto)
        {
            try
            {
                await _authService.LogoutAsync(dto);

                return Ok(new
                {
                    message = "Sesión cerrada correctamente."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}