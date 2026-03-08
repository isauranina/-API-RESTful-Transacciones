using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transacciones.Core.DTOs.Transacciones;
using Transacciones.Core.Interfaces.IServices.Transacciones;

namespace Transacciones.API.Controllers.Transacciones;

[ApiController]
[Route("api/[controller]")]
public class TransaccionesController : ControllerBase {
	private readonly ITransaccionService _transaccionService;

	public TransaccionesController(ITransaccionService transaccionService) {
		_transaccionService = transaccionService;
	}

	[HttpPost]
	[Authorize]
	public async Task<ActionResult<TransaccionDto>> ProcesarTransaccion([FromBody] CrearTransaccionDto crearTransaccionDto) {
		var transaccion = await _transaccionService.ProcesarTransaccionAsync(crearTransaccionDto);
		return Created($"/api/transacciones/{transaccion.Id}", transaccion);
	}

	[HttpPost("abono")]
	[Authorize]
	public async Task<ActionResult<TransaccionDto>> ProcesarAbono([FromBody] CrearAbonoDto crearAbonoDto) {
		var transaccion = await _transaccionService.ProcesarAbonoAsync(crearAbonoDto);
		return Created($"/api/transacciones/{transaccion.Id}", transaccion);
	}

	[HttpPost("retiro")]
	[Authorize]
	public async Task<ActionResult<TransaccionDto>> ProcesarRetiro([FromBody] CrearRetiroDto crearRetiroDto) {
		var transaccion = await _transaccionService.ProcesarRetiroAsync(crearRetiroDto);
		return Created($"/api/transacciones/{transaccion.Id}", transaccion);
	}

	[HttpGet("cuenta/{id}")]
	[Authorize]
	public async Task<ActionResult<IEnumerable<TransaccionDto>>> GetByCuentaId(int id) {
		var transacciones = await _transaccionService.GetByCuentaIdAsync(id);
		return Ok(transacciones);
	}
}





