namespace Transacciones.Core.DTOs.Transacciones;

public class CrearTransaccionDto {
	public int CuentaId { get; set; }
	public string TipoTransaccion { get; set; } = string.Empty;
	public decimal Monto { get; set; }
	public string Descripcion { get; set; } = string.Empty;
}

