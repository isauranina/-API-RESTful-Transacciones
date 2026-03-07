using System.Net;
using Application.Exceptions;

namespace Transacciones.Core.Exceptions.Transacciones;

public class MontoInvalidoException : CustomException
{
    public MontoInvalidoException(string operacion) 
        : base($"El monto a {operacion} debe ser mayor a cero", HttpStatusCode.BadRequest)
    {
    }
}




