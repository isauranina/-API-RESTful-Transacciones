namespace Transacciones.Core.DTOs.Transacciones;

public class TransaccionDto
{
    public int Id { get; set; }
    public int CuentaId { get; set; }
    public string TipoTransaccion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaTransaccion { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal SaldoAnterior { get; set; }
    public decimal SaldoNuevo { get; set; }
}

