using System.Net;
using Application.Exceptions;

namespace Transacciones.Core.Exceptions.Transacciones;

public class CuentaNoEncontradaException : CustomException {
	public CuentaNoEncontradaException(int cuentaId)
		: base($"No se encontró la cuenta con ID {cuentaId}", HttpStatusCode.NotFound) {
	}
}

