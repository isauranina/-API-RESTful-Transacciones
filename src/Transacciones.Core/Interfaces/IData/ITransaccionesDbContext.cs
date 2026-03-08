using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Transacciones.Core.Entities;

namespace Transacciones.Core.Interfaces.IData;

public interface ITransaccionesDbContext {
	DbSet<Cuenta> Cuentas { get; }
	DbSet<Transaccion> Transacciones { get; }
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}



