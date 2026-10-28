using Microsoft.AspNetCore.Mvc;
using OnlineCodingAPI.Dtos;
using OnlineCodingAPI.Services;

namespace OnlineCodingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            AuthResponseDto? result = await _authService.RegisterAsync(dto);

            if (result is null)
                return Conflict(new { message = "Username or email is already taken." });

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result is null)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(result);
        }
    }
}
