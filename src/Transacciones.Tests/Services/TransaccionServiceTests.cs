using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Transacciones.Core.DTOs.Transacciones;
using Transacciones.Core.Entities;
using Transacciones.Core.Exceptions.Transacciones;
using Transacciones.Core.Interfaces.IData;
using Transacciones.Core.Interfaces.IRepositories.Transacciones;
using Transacciones.Core.Mappings;
using Transacciones.Core.Services.Transacciones;
using Xunit;

namespace Transacciones.Tests.Services;

public class TransaccionServiceTests
{
    private readonly Mock<ITransaccionRepository> _transaccionRepositoryMock;
    private readonly Mock<ICuentaRepository> _cuentaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ITransaccionesDbContext> _contextMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;
    private readonly TransaccionService _transaccionService;

    public TransaccionServiceTests()
    {
        _transaccionRepositoryMock = new Mock<ITransaccionRepository>();
        _cuentaRepositoryMock = new Mock<ICuentaRepository>();
        _mapperMock = new Mock<IMapper>();
        _contextMock = new Mock<ITransaccionesDbContext>();
        _transactionMock = new Mock<IDbContextTransaction>();

        // AutoMapper real para  tests
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<TransaccionesMappingProfile>());
        var mapper = mapperConfig.CreateMapper();

        _transaccionService = new TransaccionService(
            _transaccionRepositoryMock.Object,
            _cuentaRepositoryMock.Object,
            mapper,
            _contextMock.Object);
    }

    [Fact]
    public async Task ProcesarRetiroAsync_ConSaldoInsuficiente_DeberiaLanzarSaldoInsuficienteException()
    {
        // Arrange
        var cuentaId = 6;
        var saldoDisponible = 100m;
        var montoRetiro = 200m;

        var cuenta = new Cuenta
        {
            Id = cuentaId,
            Saldo = saldoDisponible,
            Activa = true,
            NumeroCuenta = "123456",
            Titular = "Test User"
        };

        var crearRetiroDto = new CrearRetiroDto
        {
            CuentaId = cuentaId,
            Monto = montoRetiro
        };

        _contextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _cuentaRepositoryMock.Setup(x => x.GetByIdAsync(cuentaId))
            .ReturnsAsync(cuenta);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<SaldoInsuficienteException>(
            () => _transaccionService.ProcesarRetiroAsync(crearRetiroDto));

        Assert.NotNull(exception);
        Assert.Contains("Saldo insuficiente", exception.Message);
        Assert.Contains(saldoDisponible.ToString("C"), exception.Message);
        Assert.Contains(montoRetiro.ToString("C"), exception.Message);

        // Verificar que se intentó hacer rollback
        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcesarRetiroAsync_ConMontoInvalido_DeberiaLanzarMontoInvalidoException()
    {
        // Arrange
        var cuentaId = 1;
        var cuenta = new Cuenta
        {
            Id = cuentaId,
            Saldo = 1000m,
            Activa = false,
            NumeroCuenta = "123456",
            Titular = "Test User"
        };

        var crearRetiroDto = new CrearRetiroDto
        {
            CuentaId = cuentaId,
            Monto = 0m // Monto inválido
        };

        _contextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _cuentaRepositoryMock.Setup(x => x.GetByIdAsync(cuentaId))
            .ReturnsAsync(cuenta);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<MontoInvalidoException>(
            () => _transaccionService.ProcesarRetiroAsync(crearRetiroDto));

        Assert.NotNull(exception);
        Assert.Contains("El monto a retirar debe ser mayor a cero", exception.Message);

        // Verificar que se intentó hacer rollback
        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

