namespace Transacciones.Core.DTOs.Transacciones;

public class CrearAbonoDto
	{
	public int CuentaId { get; set; }
	public decimal Monto { get; set; }
	public string Descripcion { get; set; } = string.Empty;
}




