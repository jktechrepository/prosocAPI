using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Models.DTOs.Authentication;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken ct)
        {
            var result = await _auth.LoginAsync(request, ct);
            if (result == null)
                return Unauthorized();

            return Ok(result);
        }
    }
}
