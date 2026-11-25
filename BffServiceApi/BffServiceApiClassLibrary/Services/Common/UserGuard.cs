using BffApiClassLibrary.Clients.User;
using BffApiClassLibrary.Common;

namespace BffApiClassLibrary.Services.Common;

public static class UserGuard
{
    /// <summary>
    /// Antes de gravar, confere se os usuários informados existem (consulta em lote).
    /// Se o serviço de Usuários estiver fora do ar, não bloqueia a gravacao.
    /// </summary>
    public static async Task EnsureExistAsync(IUserApiClient users, IEnumerable<string?> codes, CancellationToken ct)
    {
        List<string> distinct = codes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (distinct.Count == 0) return;

        IReadOnlyList<UserResult> found;
        try
        {
            found = await users.FindByCodesAsync(distinct, ct);
        }
        catch (UpstreamException ex) when (ex.StatusCode >= 500)
        {
            return;
        }

        HashSet<string> foundCodes = found.Select(u => u.Code).ToHashSet(StringComparer.Ordinal);
        List<string> missing = distinct.Where(c => !foundCodes.Contains(c)).ToList();

        if (missing.Count > 0)
        {
            throw BffException.BadRequest($"Usuário(s) inexistente(s): {string.Join(", ", missing)}.");
        }
    }
}
