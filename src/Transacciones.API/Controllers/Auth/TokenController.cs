using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Transacciones.API.Controllers.Auth;


[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase {
	private readonly IConfiguration _configuration;

	public TokenController(IConfiguration configuration) {
		_configuration = configuration;
	}

	[HttpPost("generate")]
	[AllowAnonymous]
	public IActionResult GenerateToken([FromBody] TokenRequest request) {
		if (request.User != "admin" || request.Password != "Clave*") {
			return Unauthorized(new { error = "Credenciales inválidas" });
		}

		var secretKey = _configuration["Bearer:SecretKey"]
			?? throw new InvalidOperationException("debes agregar SecretKey en appsettings.json");

		var tokenHandler = new JwtSecurityTokenHandler();
		var byteKey = Encoding.UTF8.GetBytes(secretKey);

		var tokenDescriptor = new SecurityTokenDescriptor {
			Subject = new ClaimsIdentity(new Claim[]
			{
				new Claim(ClaimTypes.Name, request.User)
			}),
			Expires = DateTime.UtcNow.AddMonths(1),
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey(byteKey),
				SecurityAlgorithms.HmacSha256Signature)
		};

		var token = tokenHandler.CreateToken(tokenDescriptor);
		var tokenString = tokenHandler.WriteToken(token);

		return Ok(new {
			token = tokenString,
			expiresIn = 86400, // 1 dia la duracion del toke
			tokenType = "Bearer"
		});
	}

	[HttpPost("validate")]
	[Authorize]
	public IActionResult ValidateToken() {
		var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
		return Ok(new {
			isValid = true,
			claims = claims,
			username = User.Identity?.Name
		});
	}
}

public class TokenRequest {
	public string User { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}


