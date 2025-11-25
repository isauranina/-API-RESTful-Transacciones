using Transacciones.Core.Entities;

namespace Transacciones.Core.Interfaces.IRepositories.Transacciones;

public interface ITransaccionRepository
{
    Task<IEnumerable<Transaccion>> GetByCuentaIdAsync(int cuentaId);
    Task<Transaccion> CreateAsync(Transaccion transaccion);
}





