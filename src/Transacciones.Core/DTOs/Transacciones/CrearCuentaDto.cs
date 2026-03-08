namespace Transacciones.Core.DTOs.Transacciones;

public class CrearCuentaDto {
	public string NumeroCuenta { get; set; } = string.Empty;
	public decimal SaldoInicial { get; set; }
	public string Titular { get; set; } = string.Empty;
}

