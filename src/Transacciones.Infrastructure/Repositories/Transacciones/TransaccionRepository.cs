using Microsoft.EntityFrameworkCore;
using Transacciones.Core.Entities;
using Transacciones.Core.Interfaces.IData;
using Transacciones.Core.Interfaces.IRepositories.Transacciones;

namespace Transacciones.Infrastructure.Repositories.Transacciones;

public class TransaccionRepository : ITransaccionRepository
{
    private readonly ITransaccionesDbContext _context;

    public TransaccionRepository(ITransaccionesDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaccion>> GetByCuentaIdAsync(int cuentaId)
    {
        return await _context.Transacciones
            .Where(t => t.CuentaId == cuentaId)
            .OrderByDescending(t => t.FechaTransaccion)
            .ToListAsync();
    }

    public async Task<Transaccion> CreateAsync(Transaccion transaccion)
    {
        await _context.Transacciones.AddAsync(transaccion);
        await _context.SaveChangesAsync();
        return transaccion;
    }
}





