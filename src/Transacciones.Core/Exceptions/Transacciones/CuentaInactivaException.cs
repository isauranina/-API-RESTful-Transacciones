using System.Net;
using Application.Exceptions;

namespace Transacciones.Core.Exceptions.Transacciones;

public class CuentaInactivaException : CustomException
{
    public CuentaInactivaException(string operacion) 
        : base($"La cuenta no está activa. No se puede realizar {operacion}.", HttpStatusCode.BadRequest)
    {
    }
}

