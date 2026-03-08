using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using Transacciones.Core.DTOs.Transacciones;
using Transacciones.Core.Entities;
using Transacciones.Core.Exceptions.Transacciones;
using Transacciones.Core.Interfaces.IData;
using Transacciones.Core.Interfaces.IRepositories.Transacciones;
using Transacciones.Core.Interfaces.IServices.Transacciones;

namespace Transacciones.Core.Services.Transacciones;

public class TransaccionService : ITransaccionService {
	private readonly ITransaccionRepository _transaccionRepository;
	private readonly ICuentaRepository _cuentaRepository;
	private readonly IMapper _mapper;
	private readonly ITransaccionesDbContext _context;

	public TransaccionService(
		ITransaccionRepository transaccionRepository,
		ICuentaRepository cuentaRepository,
		IMapper mapper,
		ITransaccionesDbContext context) {
		_transaccionRepository = transaccionRepository;
		_cuentaRepository = cuentaRepository;
		_mapper = mapper;
		_context = context;
	}

	public async Task<TransaccionDto> ProcesarAbonoAsync(CrearAbonoDto crearAbonoDto) {
		// Iniciar transacción explícita de base de datos
		using var transaction = await _context.BeginTransactionAsync();

		try {
			// Obtener la cuenta
			var cuenta = await _cuentaRepository.GetByIdAsync(crearAbonoDto.CuentaId);
			if (cuenta == null) {
				throw new CuentaNoEncontradaException(crearAbonoDto.CuentaId);
			}

			// Verificar que la cuenta esté activa
			if (!cuenta.Activa) {
				throw new CuentaInactivaException("el abono");
			}

			// Validar que el monto sea mayor a cero
			if (crearAbonoDto.Monto <= 0) {
				throw new MontoInvalidoException("abonar");
			}

			// Calcular nuevos saldos
			decimal saldoAnterior = cuenta.Saldo;
			decimal saldoNuevo = saldoAnterior + crearAbonoDto.Monto;

			// Validación: No permitir saldos negativos en ningún momento
			if (saldoNuevo < 0) {
				throw new SaldoNegativoException("el abono");
			}

			// Crear la transacción con FechaTransaccion = fecha actual
			var transaccion = new Transaccion {
				CuentaId = crearAbonoDto.CuentaId,
				TipoTransaccion = TipoTransaccion.Abono,
				Monto = crearAbonoDto.Monto,
				Descripcion = crearAbonoDto.Descripcion,
				FechaTransaccion = DateTime.UtcNow,
				SaldoAnterior = saldoAnterior,
				SaldoNuevo = saldoNuevo
			};

			// Actualizar el saldo de la cuenta
			cuenta.Saldo = saldoNuevo;

			// Guardar cambios dentro de la transacción de base de datos
			await _transaccionRepository.CreateAsync(transaccion);
			await _cuentaRepository.UpdateAsync(cuenta);

			// Confirmar transacción (COMMIT)
			await transaction.CommitAsync();

			return _mapper.Map<TransaccionDto>(transaccion);
		} catch {
			// Revertir transacción (ROLLBACK) en caso de error
			await transaction.RollbackAsync();
			throw;
		}
	}

	public async Task<TransaccionDto> ProcesarRetiroAsync(CrearRetiroDto crearRetiroDto) {
		// Iniciar transacción explícita de base de datos
		using var transaction = await _context.BeginTransactionAsync();

		try {
			// Obtener la cuenta
			var cuenta = await _cuentaRepository.GetByIdAsync(crearRetiroDto.CuentaId);
			if (cuenta == null) {
				throw new CuentaNoEncontradaException(crearRetiroDto.CuentaId);
			}

			// Validar que la cuenta esté activa
			if (!cuenta.Activa) {
				throw new CuentaInactivaException("el retiro");
			}

			// Validar que el monto sea mayor a cero
			if (crearRetiroDto.Monto <= 0) {
				throw new MontoInvalidoException("retirar");
			}

			// Calcular nuevos saldos
			decimal saldoAnterior = cuenta.Saldo;

			// Validar que el saldo disponible sea suficiente antes de retiros
			if (saldoAnterior < crearRetiroDto.Monto) {
				throw new SaldoInsuficienteException(saldoAnterior, crearRetiroDto.Monto);
			}

			decimal saldoNuevo = saldoAnterior - crearRetiroDto.Monto;

			// Validación: No permitir que el saldo quede negativo
			if (saldoNuevo < 0) {
				throw new SaldoNegativoException("el retiro");
			}

			// Formatear descripción según el formato especificado: 'Se retiro un monto de {monto}, saldo anterior{monto}, saldo actual es {monto}'
			string descripcion = $"Se retiro un monto de {crearRetiroDto.Monto}, saldo anterior {saldoAnterior}, saldo actual es {saldoNuevo}";

			// Crear la transacción con FechaTransaccion = fecha actual
			var transaccion = new Transaccion {
				CuentaId = crearRetiroDto.CuentaId,
				TipoTransaccion = TipoTransaccion.Retiro,
				Monto = crearRetiroDto.Monto,
				Descripcion = descripcion,
				FechaTransaccion = DateTime.UtcNow,
				SaldoAnterior = saldoAnterior,
				SaldoNuevo = saldoNuevo
			};

			// Actualizar el saldo de la cuenta
			cuenta.Saldo = saldoNuevo;

			// Guardar cambios dentro de la transacción de base de datos
			await _transaccionRepository.CreateAsync(transaccion);
			await _cuentaRepository.UpdateAsync(cuenta);

			// Confirmar transacción (COMMIT)
			await transaction.CommitAsync();

			return _mapper.Map<TransaccionDto>(transaccion);
		} catch {
			// Revertir transacción (ROLLBACK) en caso de error
			await transaction.RollbackAsync();
			throw;
		}
	}

	public async Task<TransaccionDto> ProcesarTransaccionAsync(CrearTransaccionDto crearTransaccionDto) {
		// Iniciar transacción explícita
		using var transaction = await _context.BeginTransactionAsync();

		try {
			// Obtener la cuenta con bloqueo pesimista para evitar condiciones de carrera
			var cuenta = await _cuentaRepository.GetByIdAsync(crearTransaccionDto.CuentaId);
			if (cuenta == null) {
				throw new CuentaNoEncontradaException(crearTransaccionDto.CuentaId);
			}

			if (!cuenta.Activa) {
				throw new CuentaInactivaException("la transacción");
			}

			// Validar tipo de transacción
			if (crearTransaccionDto.TipoTransaccion != TipoTransaccion.Abono &&
				crearTransaccionDto.TipoTransaccion != TipoTransaccion.Retiro) {
				throw new TipoTransaccionInvalidoException(crearTransaccionDto.TipoTransaccion);
			}

			// Validar monto
			if (crearTransaccionDto.Monto <= 0) {
				throw new MontoInvalidoException("procesar");
			}

			// Calcular nuevos saldos
			decimal saldoAnterior = cuenta.Saldo;
			decimal saldoNuevo;

			if (crearTransaccionDto.TipoTransaccion == TipoTransaccion.Abono) {
				saldoNuevo = saldoAnterior + crearTransaccionDto.Monto;
			} else // RETIRO
			  {
				if (saldoAnterior < crearTransaccionDto.Monto) {
					throw new SaldoInsuficienteException(saldoAnterior, crearTransaccionDto.Monto);
				}
				saldoNuevo = saldoAnterior - crearTransaccionDto.Monto;
			}

			// Validación: No permitir saldos negativos en ningún momento
			if (saldoNuevo < 0) {
				throw new SaldoNegativoException("la transacción");
			}

			// Crear la transacción
			var transaccion = _mapper.Map<Transaccion>(crearTransaccionDto);
			transaccion.SaldoAnterior = saldoAnterior;
			transaccion.SaldoNuevo = saldoNuevo;

			// Actualizar el saldo de la cuenta
			cuenta.Saldo = saldoNuevo;

			// Guardar cambios dentro de la transacción
			await _transaccionRepository.CreateAsync(transaccion);
			await _cuentaRepository.UpdateAsync(cuenta);

			// Confirmar transacción
			await transaction.CommitAsync();

			return _mapper.Map<TransaccionDto>(transaccion);
		} catch {
			// Revertir transacción en caso de error
			await transaction.RollbackAsync();
			throw;
		}
	}

	public async Task<IEnumerable<TransaccionDto>> GetByCuentaIdAsync(int cuentaId) {
		var transacciones = await _transaccionRepository.GetByCuentaIdAsync(cuentaId);
		return _mapper.Map<IEnumerable<TransaccionDto>>(transacciones);
	}
}



