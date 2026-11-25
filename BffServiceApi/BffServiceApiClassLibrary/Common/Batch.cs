namespace BffApiClassLibrary.Common;

public static class Batch
{
    /// <summary>Limite das consultas em lote dos microsserviços (users?codes, hypothesis?linkCodes, feedbacks?linkCodes).</summary>
    public const int MaxCodesPerCall = 100;

    /// <summary>
    /// Divide os códigos em lotes de ate 100, chama todos em paralelo e junta o resultado.
    /// </summary>
    public static async Task<IReadOnlyList<T>> FetchInChunksAsync<T>(
        IEnumerable<string> codes,
        Func<IReadOnlyList<string>, Task<IReadOnlyList<T>>> fetch)
    {
        List<string> distinct = codes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (distinct.Count == 0) return [];

        IReadOnlyList<T>[] parts = await Task.WhenAll(
            distinct.Chunk(MaxCodesPerCall).Select(chunk => fetch(chunk)));

        return parts.SelectMany(p => p).ToList();
    }

    public static string Query(string name, IEnumerable<string> values) =>
        string.Join("&", values.Select(v => $"{name}={Uri.EscapeDataString(v)}"));
}
