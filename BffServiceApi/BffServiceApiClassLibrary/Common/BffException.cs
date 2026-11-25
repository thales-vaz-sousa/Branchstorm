namespace BffApiClassLibrary.Common;

/// <summary>
/// Erro de negocio ou de orquestracao detectado pelo proprio BFF
/// (ex.: no inexistente, feedback em no fechado). Vira ProblemDetails com o mesmo status.
/// </summary>
public class BffException(int statusCode, string title, string detail) : Exception(detail)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;

    public static BffException NotFound(string what, string code) =>
        new(404, "Recurso não encontrado", $"{what} '{code}' não existe.");

    public static BffException Conflict(string detail) =>
        new(409, "Operação não permitida", detail);

    public static BffException BadRequest(string detail) =>
        new(400, "Requisição inválida", detail);
}

/// <summary>
/// Falha vinda de um microsserviço (status de erro, indisponibilidade ou timeout).
/// </summary>
public sealed class UpstreamException(
    string service,
    int statusCode,
    string title,
    string detail,
    int? upstreamStatusCode = null) : BffException(statusCode, title, detail)
{
    public string Service { get; } = service;
    public int? UpstreamStatusCode { get; } = upstreamStatusCode;
}
