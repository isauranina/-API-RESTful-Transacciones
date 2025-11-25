using Transacciones.Core.DTOs.Transacciones;

namespace Transacciones.Core.Interfaces.IServices.Transacciones;

public interface ICuentaService
{
    Task<CuentaDto?> GetByIdAsync(int id);
    Task<CuentaDto?> GetByNumeroCuentaAsync(string numeroCuenta);
    Task<CuentaDto> CreateAsync(CrearCuentaDto crearCuentaDto);
}





