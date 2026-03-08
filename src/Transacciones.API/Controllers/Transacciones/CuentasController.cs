using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transacciones.Core.DTOs.Transacciones;
using Transacciones.Core.Interfaces.IServices.Transacciones;

namespace Transacciones.API.Controllers.Transacciones;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CuentasController : ControllerBase {
	private readonly ICuentaService _cuentaService;

	public CuentasController(ICuentaService cuentaService) {
		_cuentaService = cuentaService;
	}

	[HttpGet("{id}")]
	[Authorize]
	public async Task<ActionResult<CuentaDto>> GetById(int id) {
		var cuenta = await _cuentaService.GetByIdAsync(id);
		if (cuenta == null) {
			return NotFound();
		}
		return Ok(cuenta);
	}

	[HttpGet("numero/{numeroCuenta}")]
	[Authorize]
	public async Task<ActionResult<CuentaDto>> GetByNumeroCuenta(string numeroCuenta) {
		var cuenta = await _cuentaService.GetByNumeroCuentaAsync(numeroCuenta);
		if (cuenta == null) {
			return NotFound();
		}
		return Ok(cuenta);
	}

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<CuentaDto>> Create([FromBody] CrearCuentaDto crearCuentaDto) {
		var cuenta = await _cuentaService.CreateAsync(crearCuentaDto);
		return CreatedAtAction(nameof(GetById), new { id = cuenta.Id }, cuenta);
	}

}

