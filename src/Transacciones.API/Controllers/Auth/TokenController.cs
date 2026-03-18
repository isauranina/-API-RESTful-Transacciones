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
		var adminUser =
			Environment.GetEnvironmentVariable("AUTH_ADMIN_USER")
			?? _configuration["Auth:AdminUser"]
			?? throw new InvalidOperationException("Credenciales de administrador no configuradas.");

		var adminPassword =
			Environment.GetEnvironmentVariable("AUTH_ADMIN_PASSWORD")
			?? _configuration["Auth:AdminPassword"]
			?? throw new InvalidOperationException("Credenciales de administrador no configuradas.");

		if (request.User != adminUser || request.Password != adminPassword) {
			return Unauthorized(new { error = "Credenciales inválidas" });
		}

		var secretKey =
			Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
			?? throw new InvalidOperationException("La clave secreta JWT no está configurada. Define la variable de entorno JWT_SECRET_KEY.");

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


