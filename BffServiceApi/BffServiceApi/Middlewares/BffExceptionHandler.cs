using BffApiClassLibrary.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BffServiceApi.Middlewares;

/// <summary>
/// Todo erro sai como ProblemDetails (RFC 9457), num formato unico para o Angular tratar:
/// { status, title, detail, service?, upstreamStatus?, correlationId }.
/// </summary>
public sealed class BffExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<BffExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        ProblemDetails problem;

        switch (exception)
        {
            case UpstreamException upstream:
                logger.LogWarning("Falha no serviço {Service}: {Status} {Detail}",
                    upstream.Service, upstream.UpstreamStatusCode ?? upstream.StatusCode, upstream.Message);
                problem = new ProblemDetails
                {
                    Status = upstream.StatusCode,
                    Title = upstream.Title,
                    Detail = upstream.Message
                };
                problem.Extensions["service"] = upstream.Service;
                if (upstream.UpstreamStatusCode is int upstreamStatus)
                    problem.Extensions["upstreamStatus"] = upstreamStatus;
                break;

            case BffException bff:
                problem = new ProblemDetails
                {
                    Status = bff.StatusCode,
                    Title = bff.Title,
                    Detail = bff.Message
                };
                break;

            case BadHttpRequestException bad:
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Requisição inválida",
                    Detail = bad.Message
                };
                break;

            default:
                logger.LogError(exception, "Erro inesperado no BFF");
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Erro interno no BFF",
                    Detail = "Ocorreu um erro inesperado. Informe o correlationId ao suporte."
                };
                break;
        }

        if (context.Items[CorrelationIdHandler.HeaderName] is string correlationId)
            problem.Extensions["correlationId"] = correlationId;

        context.Response.StatusCode = problem.Status ?? 500;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}

public static class CorrelationIdMiddleware
{
    /// <summary>Le ou gera o X-Correlation-Id, devolve no response e deixa disponivel para os clientes HTTP.</summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            string correlationId = context.Request.Headers.TryGetValue(CorrelationIdHandler.HeaderName, out var value)
                                   && !string.IsNullOrWhiteSpace(value)
                ? value.ToString()
                : Guid.NewGuid().ToString("N");

            context.Items[CorrelationIdHandler.HeaderName] = correlationId;
            context.Response.Headers[CorrelationIdHandler.HeaderName] = correlationId;

            using (context.RequestServices.GetRequiredService<ILoggerFactory>()
                       .CreateLogger("Bff").BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            {
                await next();
            }
        });
}
