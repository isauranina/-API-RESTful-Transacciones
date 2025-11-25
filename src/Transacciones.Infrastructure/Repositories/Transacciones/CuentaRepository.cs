using Microsoft.EntityFrameworkCore;
using Transacciones.Core.Entities;
using Transacciones.Core.Interfaces.IData;
using Transacciones.Core.Interfaces.IRepositories.Transacciones;

namespace Transacciones.Infrastructure.Repositories.Transacciones;

public class CuentaRepository : ICuentaRepository
{
    private readonly ITransaccionesDbContext _context;

    public CuentaRepository(ITransaccionesDbContext context)
    {
        _context = context;
    }

    public async Task<Cuenta?> GetByIdAsync(int id)
    {
        return await _context.Cuentas
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cuenta?> GetByNumeroCuentaAsync(string numeroCuenta)
    {
        return await _context.Cuentas
            .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta);
    }

    public async Task<Cuenta> CreateAsync(Cuenta cuenta)
    {
        await _context.Cuentas.AddAsync(cuenta);
        await _context.SaveChangesAsync();
        return cuenta;
    }

    public async Task UpdateAsync(Cuenta cuenta)
    {
        _context.Cuentas.Update(cuenta);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Cuentas
            .AnyAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByNumeroCuentaAsync(string numeroCuenta)
    {
        return await _context.Cuentas
            .AnyAsync(c => c.NumeroCuenta == numeroCuenta);
    }
}





