using BffApiClassLibrary.Clients.Feedback;
using BffApiClassLibrary.Clients.Hypothesis;
using BffApiClassLibrary.Clients.Idea;
using BffApiClassLibrary.Clients.Team;
using BffApiClassLibrary.Clients.User;
using BffApiClassLibrary.Services.Common;
using BffApiClassLibrary.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BffApiClassLibrary.Services.Ideas;

/// <summary>Tudo que foi carregado dos serviços para montar uma árvore (reaproveitado pela exportação).</summary>
public record TreeData(
    LinkResult Focus,
    LinkResult Root,
    IReadOnlyList<LinkResult> Ancestors,
    IReadOnlyList<LinkResult> Flat,
    TreeLookups Lookups,
    TeamRef? Team,
    TreeNodeView RootView,
    TreeSummary Summary,
    IReadOnlyList<string> Warnings);

public interface IIdeaTreeService
{
    /// <summary>
    /// Árvore completa da ideia a que o nó pertence (sobe ate a raiz), com autores, contadores de
    /// hipóteses/feedback, caminho selecionado e ações. Junta Ideias + Hipóteses + Feedback + Usuários + Equipes.
    /// </summary>
    Task<IdeaTreeView> GetTreeAsync(string code, TreeBuildOptions options, CancellationToken ct = default);

    Task<TreeData> LoadAsync(string code, bool resolveRoot, TreeBuildOptions options, CancellationToken ct = default);

    /// <summary>Restringe a carga ao caminho selecionado (escopo "selected" da exportação).</summary>
    Task<TreeData> LoadSelectedPathAsync(string code, TreeBuildOptions options, CancellationToken ct = default);
}

public class IdeaTreeService(
    IIdeaApiClient ideas,
    IHypothesisApiClient hypotheses,
    IFeedbackApiClient feedbacks,
    IUserApiClient users,
    ITeamApiClient teams) : IIdeaTreeService
{
    public async Task<IdeaTreeView> GetTreeAsync(string code, TreeBuildOptions options, CancellationToken ct = default)
    {
        TreeData data = await LoadAsync(code, resolveRoot: true, options, ct);

        return new IdeaTreeView
        {
            RootCode = data.Root.Code,
            FocusCode = data.Focus.Code,
            Team = data.Team,
            Summary = data.Summary,
            Root = data.RootView,
            Warnings = data.Warnings
        };
    }

    public async Task<TreeData> LoadSelectedPathAsync(string code, TreeBuildOptions options, CancellationToken ct = default)
    {
        LinkResult focus = await LinkPathResolver.FindRequiredAsync(ideas, code, ct);
        IReadOnlyList<LinkResult> ancestors = await LinkPathResolver.GetAncestorsAsync(ideas, focus, ct);
        LinkResult root = ancestors.FirstOrDefault() ?? focus;

        IReadOnlyList<LinkResult> flat = await ideas.FindTreeAsync(root.Code, ct);
        HashSet<string> path = [.. TreeBuilder.SelectedPath(root.Code, flat.Count == 0 ? [root] : flat)];

        return await BuildAsync(focus, root, ancestors, flat,
            options with { IncludeDiscarded = true, OnlyCodes = path }, ct);
    }

    public async Task<TreeData> LoadAsync(string code, bool resolveRoot, TreeBuildOptions options, CancellationToken ct = default)
    {
        LinkResult focus = await LinkPathResolver.FindRequiredAsync(ideas, code, ct);
        IReadOnlyList<LinkResult> ancestors = resolveRoot
            ? await LinkPathResolver.GetAncestorsAsync(ideas, focus, ct)
            : [];
        LinkResult root = ancestors.FirstOrDefault() ?? focus;

        IReadOnlyList<LinkResult> flat = await ideas.FindTreeAsync(root.Code, ct);
        return await BuildAsync(focus, root, ancestors, flat, options, ct);
    }

    private async Task<TreeData> BuildAsync(
        LinkResult focus,
        LinkResult root,
        IReadOnlyList<LinkResult> ancestors,
        IReadOnlyList<LinkResult> flat,
        TreeBuildOptions options,
        CancellationToken ct)
    {
        WarningCollector warnings = new();
        if (flat.Count == 0) flat = [root];

        List<string> codes = flat
            .Where(l => options.OnlyCodes is null || options.OnlyCodes.Contains(l.Code))
            .Select(l => l.Code)
            .ToList();

        // Chamadas independentes em paralelo (cada uma em lotes de 100 códigos).
        Task<IReadOnlyList<HypothesisResult>?> hypothesesTask = options.NeedsHypotheses
            ? warnings.TryAsync<IReadOnlyList<HypothesisResult>?>(
                async () => await hypotheses.FindByLinksAsync(codes, ct), null, "Hipóteses")
            : Task.FromResult<IReadOnlyList<HypothesisResult>?>(null);

        Task<IReadOnlyList<FeedbackResult>?> feedbacksTask = options.NeedsFeedbacks
            ? warnings.TryAsync<IReadOnlyList<FeedbackResult>?>(
                async () => await feedbacks.FindByLinksAsync(codes, ct), null, "Feedbacks")
            : Task.FromResult<IReadOnlyList<FeedbackResult>?>(null);

        Task<TeamRef?> teamTask = TeamRefResolver.ResolveAsync(teams, root.TeamCode, warnings, ct);

        await Task.WhenAll(hypothesesTask, feedbacksTask, teamTask);

        IReadOnlyList<HypothesisResult>? hypothesisList = await hypothesesTask;
        IReadOnlyList<FeedbackResult>? feedbackList = await feedbacksTask;

        IReadOnlyDictionary<string, UserResult> userMap = await UserLookup.ResolveAsync(
            users,
            flat.Select(l => l.AutorCode).Concat(feedbackList?.Select(f => (string?)f.AutorCode) ?? []),
            warnings, ct);

        TreeLookups lookups = new(
            userMap,
            hypothesisList?.ToLookup(h => h.LinkCode, StringComparer.Ordinal),
            feedbackList?.ToLookup(f => f.LinkCode, StringComparer.Ordinal));

        (TreeNodeView rootView, TreeSummary summary) = TreeBuilder.Build(flat, root.Code, lookups, options);

        return new TreeData(focus, root, ancestors, flat, lookups, await teamTask, rootView, summary, warnings.ToList());
    }
}

public static class IdeaTreeServiceUtil
{
    public static IServiceCollection UseIdeaTreeService(this IServiceCollection services)
    {
        services.AddTransient<IIdeaTreeService, IdeaTreeService>();
        return services;
    }
}
