using System.ComponentModel.DataAnnotations;
using BffApiClassLibrary.Clients.Team;
using BffApiClassLibrary.Clients.User;
using BffApiClassLibrary.Common;
using BffApiClassLibrary.Services.Common;
using BffApiClassLibrary.Views;
using Microsoft.Extensions.DependencyInjection;
using static BffApiClassLibrary.Common.Vocabulary;

namespace BffApiClassLibrary.Services.Teams;

public record TeamMemberInput
{
    [Required(ErrorMessage = "UserCode é obrigatório")]
    public string UserCode { get; init; } = string.Empty;

    /// <summary>Admin | Owner | Collaborator | Evaluator (sem diferenciar maiusculas).</summary>
    [Required(ErrorMessage = "Role é obrigatório")]
    public string Role { get; init; } = string.Empty;
}

public record CreateTeamRequest
{
    [Required(ErrorMessage = "Name é obrigatório")]
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public IReadOnlyList<TeamMemberInput> Members { get; init; } = [];
}

public record ReplaceMembersRequest
{
    [Required(ErrorMessage = "Members é obrigatório")]
    public IReadOnlyList<TeamMemberInput> Members { get; init; } = [];
}

public interface ITeamViewService
{
    Task<TeamView> GetAsync(string code, CancellationToken ct = default);
    Task<TeamView> CreateAsync(CreateTeamRequest request, CancellationToken ct = default);
    Task<TeamView> ReplaceMembersAsync(string code, ReplaceMembersRequest request, CancellationToken ct = default);
    /// <summary>Adiciona um membro ou troca o papel dele, se ja estiver na equipe.</summary>
    Task<TeamView> UpsertMemberAsync(string code, TeamMemberInput member, CancellationToken ct = default);
    Task<TeamView> RemoveMemberAsync(string code, string userCode, CancellationToken ct = default);
}

/// <summary>
/// O serviço de Equipes so guarda o código dos membros e o PATCH dele substitui a lista inteira
/// exigindo nome e descrição. O BFF junta os dados dos usuários e oferece operações de um membro so.
/// </summary>
public class TeamViewService(ITeamApiClient teams, IUserApiClient users) : ITeamViewService
{
    public async Task<TeamView> GetAsync(string code, CancellationToken ct = default)
    {
        TeamResult team = await FindRequiredAsync(code, ct);
        return await ToViewAsync(team, ct);
    }

    public async Task<TeamView> CreateAsync(CreateTeamRequest request, CancellationToken ct = default)
    {
        List<TeamMemberRequest> members = NormalizeMembers(request.Members);
        await UserGuard.EnsureExistAsync(users, members.Select(m => m.UserCode), ct);

        TeamResult created = await teams.CreateAsync(new TeamUpsertRequest(
            request.Name.Trim(), request.Description?.Trim() ?? string.Empty, members), ct);

        return await ToViewAsync(created, ct);
    }

    public async Task<TeamView> ReplaceMembersAsync(string code, ReplaceMembersRequest request, CancellationToken ct = default)
    {
        TeamResult team = await FindRequiredAsync(code, ct);
        List<TeamMemberRequest> members = NormalizeMembers(request.Members);
        await UserGuard.EnsureExistAsync(users, members.Select(m => m.UserCode), ct);

        return await SaveAsync(team, members, ct);
    }

    public async Task<TeamView> UpsertMemberAsync(string code, TeamMemberInput member, CancellationToken ct = default)
    {
        TeamResult team = await FindRequiredAsync(code, ct);
        TeamMemberRequest normalized = NormalizeMembers([member])[0];
        await UserGuard.EnsureExistAsync(users, [normalized.UserCode], ct);

        List<TeamMemberRequest> members = Current(team)
            .Where(m => m.UserCode != normalized.UserCode)
            .Append(normalized)
            .ToList();

        return await SaveAsync(team, members, ct);
    }

    public async Task<TeamView> RemoveMemberAsync(string code, string userCode, CancellationToken ct = default)
    {
        TeamResult team = await FindRequiredAsync(code, ct);
        List<TeamMemberRequest> current = Current(team);

        if (current.All(m => m.UserCode != userCode))
        {
            throw BffException.NotFound($"Membro na equipe '{team.Name}'", userCode);
        }

        List<TeamMemberRequest> remaining = current.Where(m => m.UserCode != userCode).ToList();
        bool hadOwnership = current.Any(m => m.Role is TeamRoles.Owner or TeamRoles.Admin);
        if (hadOwnership && remaining.Count > 0 && remaining.All(m => m.Role is not (TeamRoles.Owner or TeamRoles.Admin)))
        {
            throw BffException.Conflict("A equipe precisa manter pelo menos um Owner ou Admin.");
        }

        return await SaveAsync(team, remaining, ct);
    }

    private async Task<TeamView> SaveAsync(TeamResult team, List<TeamMemberRequest> members, CancellationToken ct)
    {
        TeamResult updated = await teams.ReplaceMembersAsync(team.Code,
            new TeamUpsertRequest(team.Name, team.Description ?? string.Empty, members), ct)
            ?? throw BffException.NotFound("Equipe", team.Code);

        return await ToViewAsync(updated, ct);
    }

    private async Task<TeamResult> FindRequiredAsync(string code, CancellationToken ct) =>
        await teams.FindOneAsync(code, ct) ?? throw BffException.NotFound("Equipe", code);

    private static List<TeamMemberRequest> Current(TeamResult team) =>
        team.Members
            .Select(m => new TeamMemberRequest(m.UserCode, TeamRoles.Parse(m.Role) ?? TeamRoles.Collaborator))
            .ToList();

    private static List<TeamMemberRequest> NormalizeMembers(IEnumerable<TeamMemberInput> members)
    {
        List<TeamMemberRequest> result = [];
        foreach (TeamMemberInput member in members)
        {
            string role = TeamRoles.Parse(member.Role)
                ?? throw BffException.BadRequest(
                    $"Papel '{member.Role}' inválido. Use: {string.Join(", ", TeamRoles.All)}.");

            if (string.IsNullOrWhiteSpace(member.UserCode))
                throw BffException.BadRequest("UserCode é obrigatório em todos os membros.");

            result.Add(new TeamMemberRequest(member.UserCode.Trim(), role));
        }

        List<string> duplicated = result.GroupBy(m => m.UserCode).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicated.Count > 0)
            throw BffException.BadRequest($"Usuário(s) repetido(s) na equipe: {string.Join(", ", duplicated)}.");

        return result;
    }

    private static int RoleOrder(string role)
    {
        int index = Array.IndexOf(TeamRoles.All, role);
        return index < 0 ? int.MaxValue : index;
    }

    private async Task<TeamView> ToViewAsync(TeamResult team, CancellationToken ct)
    {
        WarningCollector warnings = new();
        IReadOnlyDictionary<string, UserResult> userMap = await UserLookup.ResolveAsync(
            users, team.Members.Select(m => (string?)m.UserCode), warnings, ct);

        List<TeamMemberView> members = team.Members
            .Select(m =>
            {
                string role = TeamRoles.Parse(m.Role) ?? m.Role;
                bool found = userMap.TryGetValue(m.UserCode, out UserResult? user);
                return new TeamMemberView(m.UserCode, user?.Name, user?.Email, role, found);
            })
            .OrderBy(m => RoleOrder(m.Role))
            .ThenBy(m => m.Name ?? m.UserCode, StringComparer.CurrentCulture)
            .ToList();

        // So acusa usuario sumido se o serviço de Usuários respondeu.
        if (warnings.ToList().Count == 0)
        {
            foreach (TeamMemberView missing in members.Where(m => !m.UserFound))
                warnings.Add($"Usuário '{missing.UserCode}' da equipe não existe mais no serviço de Usuários.");
        }

        return new TeamView
        {
            Code = team.Code,
            Name = team.Name,
            Description = team.Description,
            Members = members,
            CountByRole = TeamRoles.All.ToDictionary(r => r, r => members.Count(m => m.Role == r)),
            Warnings = warnings.ToList()
        };
    }
}

public static class TeamViewServiceUtil
{
    public static IServiceCollection UseTeamViewService(this IServiceCollection services)
    {
        services.AddTransient<ITeamViewService, TeamViewService>();
        return services;
    }
}
