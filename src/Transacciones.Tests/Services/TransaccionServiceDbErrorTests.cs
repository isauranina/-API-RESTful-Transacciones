using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;
using Transacciones.Core.DTOs.Transacciones;
using Transacciones.Core.Entities;
using Transacciones.Core.Interfaces.IData;
using Transacciones.Core.Interfaces.IRepositories.Transacciones;
using Transacciones.Core.Services.Transacciones;

namespace Transacciones.Tests.Services
{
    public class TransaccionServiceDbErrorTests
    {
        private readonly Mock<ICuentaRepository> _cuentaRepositoryMock;
        private readonly Mock<ITransaccionRepository> _transaccionRepositoryMock;
        private readonly Mock<ITransaccionesDbContext> _contextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly TransaccionService _transaccionService;

        public TransaccionServiceDbErrorTests()
        {
            _cuentaRepositoryMock = new Mock<ICuentaRepository>();
            _transaccionRepositoryMock = new Mock<ITransaccionRepository>();
            _contextMock = new Mock<ITransaccionesDbContext>();
            _mapperMock = new Mock<IMapper>(); // Inicializa el mock de IMapper

            _transaccionService = new TransaccionService(
                _transaccionRepositoryMock.Object, // Correcto: ITransaccionRepository primero
                _cuentaRepositoryMock.Object,      // Correcto: ICuentaRepository segundo
                _mapperMock.Object, // Pasa el mock de IMapper
                _contextMock.Object);
        }

        [Fact]
        public async Task ProcesarAbonoAsync_AlOcurrirErrorEnBaseDeDatos_DebeHacerRollbackYLanzarExcepcion()
        {
            // Arrange
            var dto = new CrearAbonoDto { CuentaId = 1, Monto = 100 };
            var cuenta = new Cuenta { Id = 1, Activa = true, Saldo = 500 };

            var transactionMock = new Mock<IDbContextTransaction>();
            _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(transactionMock.Object);

            _cuentaRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cuenta);

            // Simulamos que ocurre un error crítico al intentar guardar la transacción en BD
            _transaccionRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Transaccion>()))
                                      .ThrowsAsync(new Exception("Error de conexión a la base de datos"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _transaccionService.ProcesarAbonoAsync(dto));
            Assert.Equal("Error de conexión a la base de datos", exception.Message);

            // Verificamos que se ejecutó RollbackAsync y NO CommitAsync
            transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcesarRetiroAsync_AlOcurrirErrorEnBaseDeDatos_DebeHacerRollbackYLanzarExcepcion()
        {
            // Arrange
            var dto = new CrearRetiroDto { CuentaId = 1, Monto = 100 };
            var cuenta = new Cuenta { Id = 1, Activa = true, Saldo = 500 };

            var transactionMock = new Mock<IDbContextTransaction>();
            _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(transactionMock.Object);

            _cuentaRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cuenta);

            _transaccionRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Transaccion>()))
                                      .ThrowsAsync(new Exception("Error de conexión a la base de datos"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _transaccionService.ProcesarRetiroAsync(dto));

            // Verificamos protección de atomicidad
            transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
