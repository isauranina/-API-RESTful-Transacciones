namespace Transacciones.Core.Entities;

public class Cuenta
{
    public int Id { get; set; }
    public string NumeroCuenta { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public string Titular { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool Activa { get; set; }
    
    // Para control de concurrencia optimista
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    
    // Navegación
    public virtual ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
}

