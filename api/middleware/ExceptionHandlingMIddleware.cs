using Microsoft.AspNetCore.Mvc;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            // Se a resposta já começou, não podemos substituí-la.
            if (context.Response.HasStarted)
                throw;

            var (statusCode, title, detail) = exception switch
            {
                ResourceNotFoundException => (
                    StatusCodes.Status404NotFound,
                    "Recurso não encontrado",
                    exception.Message),

                InvalidUploadException => (
                    StatusCodes.Status400BadRequest,
                    "Imagem inválida",
                    exception.Message),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Erro interno",
                    "Ocorreu um erro inesperado ao processar a solicitação.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Erro ao processar {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path.Value
            };

            await context.Response.WriteAsJsonAsync(
                problem,
                options: null,
                contentType: "application/problem+json",
                cancellationToken: context.RequestAborted);
        }
    }
}
