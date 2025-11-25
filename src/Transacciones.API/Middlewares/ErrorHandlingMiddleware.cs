using Application.Exceptions;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Transacciones.API.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception,
            ILogger<ErrorHandlingMiddleware> logger)
        {
            logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);
            
            context.Response.ContentType = "application/json";
            var statusCode = HttpStatusCode.InternalServerError;
            var message = exception.Message;

            // Manejo de excepciones personalizadas segun requerimiento de la prueba tecnica trnsacciones
            // Las CustomException siempre retornan su mensaje personalizado y código HTTP
            if (exception is CustomException customException)
            {
                statusCode = customException.StatusCode;
                message = customException.Message; 
            }
            // para Conflicto de concurrencia o idempotencia
            else if (exception is DbUpdateConcurrencyException)
            {
                statusCode = HttpStatusCode.Conflict;
                message = "Conflicto de concurrencia. El recurso ha sido modificado por otro usuario.";
            }
            // Conflicto - Para duplicados u otros conflictos (verificar después de CustomException)
            else if (exception.Message.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) || 
                     exception.Message.Contains("duplicado", StringComparison.OrdinalIgnoreCase) ||
                     exception.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            {
                statusCode = HttpStatusCode.Conflict;
            }
            // Not Found - Recurso no encontrado
            else if (exception is KeyNotFoundException || 
                     exception is FileNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;                
                if (string.IsNullOrWhiteSpace(exception.Message) || exception.Message == "An item with the same key has already been added.")
                {
                    message = "Recurso no encontrado";
                }
            }
            // Bad Request - Validaciones fallidas
            else if (exception is ArgumentException || 
                     exception is ArgumentNullException ||
                     exception is InvalidOperationException)
            {
                statusCode = HttpStatusCode.BadRequest;              
            }
            // Internal Server Error - Errores del sistema
            else
            {
                statusCode = HttpStatusCode.InternalServerError;               
                if (context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() != true)
                {
                    message = "Ha ocurrido un error interno del servidor.";
                }
            }

            context.Response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new
            {
                statusCode = (int)statusCode,
                message = message,
                timestamp = DateTime.UtcNow
            });

            await context.Response.WriteAsync(result);
        }
    }
}
