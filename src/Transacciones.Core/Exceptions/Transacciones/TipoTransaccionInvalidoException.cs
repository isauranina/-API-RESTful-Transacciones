using System.Net;
using Application.Exceptions;

namespace Transacciones.Core.Exceptions.Transacciones;

public class TipoTransaccionInvalidoException : CustomException
{
    public TipoTransaccionInvalidoException(string tipoTransaccion) 
        : base($"Tipo de transacción inválido: {tipoTransaccion}. Los tipos válidos son: ABONO, RETIRO", HttpStatusCode.BadRequest)
    {
    }
}




