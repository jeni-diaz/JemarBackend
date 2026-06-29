using Jemar.Aplication.Exceptions;
using System.Text.Json;
using AppValidationException = Jemar.Aplication.Exceptions.ValidationException;

namespace Jemar.Presentation.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                AppValidationException ex => (StatusCodes.Status400BadRequest, ex.Message),
                ArgumentException ex => (StatusCodes.Status400BadRequest, ex.Message),
                NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
                ConflictException ex => (StatusCodes.Status409Conflict, ex.Message),
                UnauthorizedException ex => (StatusCodes.Status401Unauthorized, ex.Message),
                UnauthorizedAccessException ex => (StatusCodes.Status403Forbidden, ex.Message),
                DatabaseException ex => (StatusCodes.Status500InternalServerError, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado.")
            };

            if (exception is not (AppValidationException or ArgumentException or NotFoundException or ConflictException or UnauthorizedException or UnauthorizedAccessException or DatabaseException))
                _logger.LogError(exception, "Excepción no controlada: {Message}", exception.Message);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new { error = message });
            await context.Response.WriteAsync(payload);
        }
    }
}