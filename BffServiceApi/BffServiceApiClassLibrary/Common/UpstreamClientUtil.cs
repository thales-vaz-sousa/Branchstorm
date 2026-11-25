using Microsoft.AspNetCore.Http;

namespace BffApiClassLibrary.Common;

public static class UpstreamClientUtil
{
    /// <summary>Garante a barra final, senão "http://host/prefixo" + "api/x" perde o prefixo.</summary>
    public static Uri? ToBaseUri(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return null;
        return new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
    }
}

/// <summary>
/// Propaga o cabecalho X-Correlation-Id para todos os microsserviços chamados,
/// permitindo seguir uma requisicao do Angular pelos logs de cada serviço.
/// </summary>
public sealed class CorrelationIdHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    public const string HeaderName = "X-Correlation-Id";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? correlationId = accessor.HttpContext?.Items[HeaderName] as string;
        if (!string.IsNullOrWhiteSpace(correlationId) && !request.Headers.Contains(HeaderName))
        {
            request.Headers.TryAddWithoutValidation(HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
