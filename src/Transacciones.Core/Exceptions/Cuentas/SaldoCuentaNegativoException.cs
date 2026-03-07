using System.Net;
using Application.Exceptions;

namespace Transacciones.Core.Exceptions.Cuentas;

public class SaldoCuentaNegativoException : CustomException {
    public SaldoCuentaNegativoException() 
        : base("El saldo no puede ser negativo en ningún momento.", HttpStatusCode.BadRequest) {
    }
}




