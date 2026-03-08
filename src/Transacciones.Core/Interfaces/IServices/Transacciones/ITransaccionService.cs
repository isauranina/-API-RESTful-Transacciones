using Transacciones.Core.DTOs.Transacciones;

namespace Transacciones.Core.Interfaces.IServices.Transacciones;

public interface ITransaccionService {
	Task<TransaccionDto> ProcesarTransaccionAsync(CrearTransaccionDto crearTransaccionDto);
	Task<TransaccionDto> ProcesarAbonoAsync(CrearAbonoDto crearAbonoDto);
	Task<TransaccionDto> ProcesarRetiroAsync(CrearRetiroDto crearRetiroDto);
	Task<IEnumerable<TransaccionDto>> GetByCuentaIdAsync(int cuentaId);
}





