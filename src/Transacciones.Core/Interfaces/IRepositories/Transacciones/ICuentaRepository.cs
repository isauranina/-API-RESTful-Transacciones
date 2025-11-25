using Transacciones.Core.Entities;

namespace Transacciones.Core.Interfaces.IRepositories.Transacciones;

public interface ICuentaRepository
{
    Task<Cuenta?> GetByIdAsync(int id);
    Task<Cuenta?> GetByNumeroCuentaAsync(string numeroCuenta);
    Task<Cuenta> CreateAsync(Cuenta cuenta);
    Task UpdateAsync(Cuenta cuenta);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNumeroCuentaAsync(string numeroCuenta);
}





