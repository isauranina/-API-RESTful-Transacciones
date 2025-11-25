using System.Net;
using Application.Exceptions;

namespace Transacciones.Core.Exceptions.Transacciones;

public class SaldoNegativoException : CustomException
{
    public SaldoNegativoException(string operacion) 
        : base($"No se puede realizar {operacion}. El saldo resultante no puede ser negativo.", HttpStatusCode.BadRequest)
    {
    }
}


