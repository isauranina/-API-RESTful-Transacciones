using System.Threading;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Transacciones.Core.DTOs.Transacciones;
using Transacciones.Core.Entities;
using Transacciones.Core.Exceptions.Transacciones;
using Transacciones.Core.Interfaces.IData;
using Transacciones.Core.Interfaces.IRepositories.Transacciones;
using Transacciones.Core.Interfaces.IServices.Transacciones;
using Transacciones.Core.Mappings;
using Transacciones.Core.Services.Transacciones;
using Xunit;

namespace Transacciones.Tests.Services;

public class TransaccionServiceTransaccionTests {
	private readonly Mock<ITransaccionRepository> _transaccionRepositoryMock;
	private readonly Mock<ICuentaRepository> _cuentaRepositoryMock;
	private readonly Mock<ITransaccionesDbContext> _contextMock;
	private readonly Mock<IDbContextTransaction> _transactionMock;
	private readonly ITransaccionService _transaccionService;

	public TransaccionServiceTransaccionTests() {
		_transaccionRepositoryMock = new Mock<ITransaccionRepository>();
		_cuentaRepositoryMock = new Mock<ICuentaRepository>();
		_contextMock = new Mock<ITransaccionesDbContext>();
		_transactionMock = new Mock<IDbContextTransaction>();

		_contextMock
			.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(_transactionMock.Object);

		var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<TransaccionesMappingProfile>());
		var mapper = mapperConfig.CreateMapper();

		_transaccionService = new TransaccionService(
			_transaccionRepositoryMock.Object,
			_cuentaRepositoryMock.Object,
			mapper,
			_contextMock.Object);
	}

	[Fact]
	public async Task ProcesarTransaccionAsync_TipoAbono_DeberiaCommitarYRetornarDto() {
		// Arrange
		var cuentaId = 1;
		var cuenta = new Cuenta {
			Id = cuentaId,
			Saldo = 100m,
			Activa = true,
			NumeroCuenta = "123456",
			Titular = "Test User"
		};

		var dtoInput = new CrearTransaccionDto {
			CuentaId = cuentaId,
			TipoTransaccion = TipoTransaccion.Abono,
			Monto = 25m,
			Descripcion = "Abono transaccion"
		};

		_transaccionRepositoryMock
			.Setup(x => x.CreateAsync(It.IsAny<Transaccion>()))
			.ReturnsAsync((Transaccion t) => t);

		_cuentaRepositoryMock
			.Setup(x => x.GetByIdAsync(cuentaId))
			.ReturnsAsync(cuenta);

		decimal? saldoActualizado = null;
		_cuentaRepositoryMock
			.Setup(x => x.UpdateAsync(It.IsAny<Cuenta>()))
			.Callback<Cuenta>(c => saldoActualizado = c.Saldo)
			.Returns(Task.CompletedTask);

		// Act
		var result = await _transaccionService.ProcesarTransaccionAsync(dtoInput);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(cuentaId, result.CuentaId);
		Assert.Equal(TipoTransaccion.Abono, result.TipoTransaccion);
		Assert.Equal(25m, result.Monto);
		Assert.Equal(100m, result.SaldoAnterior);
		Assert.Equal(125m, result.SaldoNuevo);
		Assert.Equal(dtoInput.Descripcion, result.Descripcion);

		Assert.Equal(125m, saldoActualizado);
		_transactionMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
		_transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task ProcesarTransaccionAsync_TipoRetiro_DeberiaCommitarYRetornarDto() {
		// Arrange
		var cuentaId = 1;
		var cuenta = new Cuenta {
			Id = cuentaId,
			Saldo = 100m,
			Activa = true,
			NumeroCuenta = "123456",
			Titular = "Test User"
		};

		var dtoInput = new CrearTransaccionDto {
			CuentaId = cuentaId,
			TipoTransaccion = TipoTransaccion.Retiro,
			Monto = 40m,
			Descripcion = "Retiro transaccion"
		};

		_transaccionRepositoryMock
			.Setup(x => x.CreateAsync(It.IsAny<Transaccion>()))
			.ReturnsAsync((Transaccion t) => t);

		_cuentaRepositoryMock
			.Setup(x => x.GetByIdAsync(cuentaId))
			.ReturnsAsync(cuenta);

		decimal? saldoActualizado = null;
		_cuentaRepositoryMock
			.Setup(x => x.UpdateAsync(It.IsAny<Cuenta>()))
			.Callback<Cuenta>(c => saldoActualizado = c.Saldo)
			.Returns(Task.CompletedTask);

		// Act
		var result = await _transaccionService.ProcesarTransaccionAsync(dtoInput);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(cuentaId, result.CuentaId);
		Assert.Equal(TipoTransaccion.Retiro, result.TipoTransaccion);
		Assert.Equal(40m, result.Monto);
		Assert.Equal(100m, result.SaldoAnterior);
		Assert.Equal(60m, result.SaldoNuevo);
		Assert.Equal(dtoInput.Descripcion, result.Descripcion);

		Assert.Equal(60m, saldoActualizado);
		_transactionMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
		_transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}

