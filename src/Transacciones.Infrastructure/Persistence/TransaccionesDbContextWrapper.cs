using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Transacciones.Core.Entities;
using Transacciones.Core.Interfaces.IData;

namespace Transacciones.Infrastructure.Persistence;

public class TransaccionesDbContextWrapper : ITransaccionesDbContext
{
    private readonly TransaccionesDbContext _context;

    public TransaccionesDbContextWrapper(TransaccionesDbContext context)
    {
        _context = context;
    }

    public DbSet<Cuenta> Cuentas => _context.Cuentas;
    public DbSet<Transaccion> Transacciones => _context.Transacciones;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _context.Database.BeginTransactionAsync(cancellationToken);
    }
}





