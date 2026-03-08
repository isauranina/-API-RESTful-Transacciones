using System.Net;
using Application.Exceptions;

namespace Transacciones.Core.Exceptions.Transacciones;

public class SaldoInsuficienteException : CustomException {
	public SaldoInsuficienteException(decimal saldoDisponible, decimal montoSolicitado)
		: base($"Saldo insuficiente. Saldo disponible: {saldoDisponible:C}, Monto solicitado: {montoSolicitado:C}", HttpStatusCode.BadRequest) {
	}
}




