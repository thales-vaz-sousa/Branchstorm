using BffApiClassLibrary.Clients.User;
using BffApiClassLibrary.Common;

namespace BffApiClassLibrary.Services.Common;

/// <summary>
/// Degradacao elegante: dados complementares (nomes, notas, hipóteses) que falham
/// não derrubam a tela; viram um aviso em "warnings" e o campo fica vazio/null.
/// </summary>
public sealed class WarningCollector
{
    private readonly List<string> warnings = [];
    private readonly Lock gate = new();

    public IReadOnlyList<string> ToList()
    {
        lock (gate) return warnings.ToList();
    }

    public void Add(string warning)
    {
        lock (gate) warnings.Add(warning);
    }

    public async Task<T> TryAsync<T>(Func<Task<T>> action, T fallback, string what)
    {
        try
        {
            return await action();
        }
        catch (UpstreamException ex)
        {
            Add($"{what} indisponível ({ex.Service}): {ex.Message}");
            return fallback;
        }
    }
}

public static class UserLookup
{
    /// <summary>Resolve nomes de varios usuários numa chamada em lote (blocos de 100), tolerando falha.</summary>
    public static async Task<IReadOnlyDictionary<string, UserResult>> ResolveAsync(
        IUserApiClient users,
        IEnumerable<string?> codes,
        WarningCollector warnings,
        CancellationToken ct)
    {
        List<string> distinct = codes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (distinct.Count == 0) return new Dictionary<string, UserResult>();

        IReadOnlyList<UserResult> found = await warnings.TryAsync(
            () => users.FindByCodesAsync(distinct, ct), [], "Nomes dos usuários");

        return found
            .GroupBy(u => u.Code, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
    }
}

public static class TeamRefResolver
{
    /// <summary>Nome da equipe para exibir junto do nó; tolerante a falha do serviço de Equipes.</summary>
    public static async Task<Views.TeamRef?> ResolveAsync(
        Clients.Team.ITeamApiClient teams,
        string? teamCode,
        WarningCollector warnings,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(teamCode)) return null;

        Clients.Team.TeamResult? team = await warnings.TryAsync<Clients.Team.TeamResult?>(
            () => teams.FindOneAsync(teamCode, ct), null, "Equipe");

        return new Views.TeamRef(teamCode, team?.Name);
    }
}
