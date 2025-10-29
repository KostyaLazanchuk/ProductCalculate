using Lab3.Models;
using Lab3.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lab3.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController(AuthService auth) : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            if (!auth.Validate(dto.Username, dto.Password))
                return Unauthorized();

            var token = auth.GenerateJwt(dto.Username);
            return Ok(new { token });
        }
    }

}
