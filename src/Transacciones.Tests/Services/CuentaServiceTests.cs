using AutoMapper;
using Moq;
using Transacciones.Core.DTOs.Transacciones;
using Transacciones.Core.Entities;
using Transacciones.Core.Exceptions.Cuentas;
using Transacciones.Core.Interfaces.IData;
using Transacciones.Core.Interfaces.IRepositories.Transacciones;
using Transacciones.Core.Interfaces.IServices.Transacciones;
using Transacciones.Core.Services.Transacciones;
using Xunit;

namespace Transacciones.Tests.Services;

public class CuentaServiceTests {
	private readonly Mock<ICuentaRepository> _cuentaRepositoryMock;
	private readonly Mock<ITransaccionesDbContext> _contextMock;
	private readonly ICuentaService _cuentaService;

	public CuentaServiceTests() {
		_cuentaRepositoryMock = new Mock<ICuentaRepository>();
		_contextMock = new Mock<ITransaccionesDbContext>();

		var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<Transacciones.Core.Mappings.TransaccionesMappingProfile>());
		var mapper = mapperConfig.CreateMapper();

		_cuentaService = new CuentaService(
			_cuentaRepositoryMock.Object,
			mapper,
			_contextMock.Object);
	}

	[Fact]
	public async Task CreateAsync_CuandoSaldoInicialEsNegativo_DeberiaLanzarSaldoInicialNegativoException() {
		var crearCuentaDto = new CrearCuentaDto {
			NumeroCuenta = "123",
			SaldoInicial = -1m,
			Titular = "Test"
		};

		await Assert.ThrowsAsync<SaldoInicialNegativoException>(() => _cuentaService.CreateAsync(crearCuentaDto));
	}

	[Fact]
	public async Task CreateAsync_CuandoNumeroCuentaYaExiste_DeberiaLanzarNumeroCuentaDuplicadoException() {
		var crearCuentaDto = new CrearCuentaDto {
			NumeroCuenta = "123",
			SaldoInicial = 100m,
			Titular = "Test"
		};

		_cuentaRepositoryMock
			.Setup(r => r.ExistsByNumeroCuentaAsync(crearCuentaDto.NumeroCuenta))
			.ReturnsAsync(true);

		await Assert.ThrowsAsync<NumeroCuentaDuplicadoException>(() => _cuentaService.CreateAsync(crearCuentaDto));
	}

	[Fact]
	public async Task CreateAsync_CuandoDatosSonValidos_DeberiaCrearCuentaYRetornarCuentaDto() {
		var crearCuentaDto = new CrearCuentaDto {
			NumeroCuenta = "123",
			SaldoInicial = 100m,
			Titular = "Test"
		};

		_cuentaRepositoryMock
			.Setup(r => r.ExistsByNumeroCuentaAsync(crearCuentaDto.NumeroCuenta))
			.ReturnsAsync(false);

		_cuentaRepositoryMock
			.Setup(r => r.CreateAsync(It.IsAny<Cuenta>()))
			.ReturnsAsync((Cuenta cuenta) => {
				cuenta.Id = 1;
				return cuenta;
			});

		var result = await _cuentaService.CreateAsync(crearCuentaDto);

		Assert.NotNull(result);
		Assert.Equal(1, result.Id);
		Assert.Equal(crearCuentaDto.NumeroCuenta, result.NumeroCuenta);
		Assert.Equal(crearCuentaDto.SaldoInicial, result.Saldo);
		Assert.Equal(crearCuentaDto.Titular, result.Titular);
		Assert.True(result.Activa);
	}
}

