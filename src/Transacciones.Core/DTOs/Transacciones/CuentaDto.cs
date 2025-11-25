namespace Transacciones.Core.DTOs.Transacciones;

public class CuentaDto
{
    public int Id { get; set; }
    public string NumeroCuenta { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public string Titular { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool Activa { get; set; }
}

