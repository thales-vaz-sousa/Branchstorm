using BffApiClassLibrary.Clients.Feedback;
using BffApiClassLibrary.Clients.Hypothesis;
using BffApiClassLibrary.Clients.Idea;
using BffApiClassLibrary.Clients.User;
using BffApiClassLibrary.Views;
using static BffApiClassLibrary.Common.Vocabulary;

namespace BffApiClassLibrary.Services.Ideas;

public record TreeBuildOptions
{
    public bool IncludeDiscarded { get; init; } = true;
    public bool IncludeStats { get; init; } = true;
    public bool IncludeHistory { get; init; }
    public bool IncludeHypotheses { get; init; }
    public bool IncludeFeedbacks { get; init; }
    /// <summary>Se informado, so estes nos entram (usado no escopo "caminho selecionado" da exportação).</summary>
    public IReadOnlySet<string>? OnlyCodes { get; init; }

    public bool NeedsHypotheses => IncludeStats || IncludeHypotheses;
    public bool NeedsFeedbacks => IncludeStats || IncludeFeedbacks;
}

public record TreeLookups(
    IReadOnlyDictionary<string, UserResult> Users,
    ILookup<string, HypothesisResult>? Hypotheses,
    ILookup<string, FeedbackResult>? Feedbacks);

/// <summary>
/// Transforma a lista plana do GET /api/idea/tree/{code} na árvore aninhada que o front desenha,
/// ja com caminho selecionado, contadores e ações por nó.
/// </summary>
public static class TreeBuilder
{
    /// <summary>Raiz → filho selecionado/final → ... (regra: so um filho selecionado por nivel).</summary>
    public static IReadOnlyList<string> SelectedPath(string rootCode, IReadOnlyList<LinkResult> flat)
    {
        ILookup<string, LinkResult> children = ChildrenLookup(flat);
        List<string> path = [rootCode];
        HashSet<string> visited = [rootCode];
        string current = rootCode;

        while (true)
        {
            LinkResult? next = children[current]
                .Where(ViewMapper.IsSelectedOrFinal)
                .OrderBy(l => ViewMapper.StatusOf(l) == LinkStatuses.Final ? 0 : 1)
                .ThenBy(l => l.CreationDate)
                .FirstOrDefault();

            if (next is null || !visited.Add(next.Code)) break;
            path.Add(next.Code);
            current = next.Code;
        }

        return path;
    }

    public static (TreeNodeView Root, TreeSummary Summary) Build(
        IReadOnlyList<LinkResult> flat,
        string rootCode,
        TreeLookups lookups,
        TreeBuildOptions options)
    {
        ILookup<string, LinkResult> children = ChildrenLookup(flat);
        LinkResult root = flat.First(l => l.Code == rootCode);
        HashSet<string> selectedPath = [.. SelectedPath(rootCode, flat)];
        List<LinkResult> included = [];
        HashSet<string> visited = [];

        TreeNodeView BuildNode(LinkResult link, int depth)
        {
            visited.Add(link.Code);
            included.Add(link);

            List<TreeNodeView> childViews = children[link.Code]
                .Where(c => !visited.Contains(c.Code))
                .Where(c => options.IncludeDiscarded || ViewMapper.StatusOf(c) != LinkStatuses.Discarded)
                .Where(c => options.OnlyCodes is null || options.OnlyCodes.Contains(c.Code))
                .OrderBy(c => c.CreationDate)
                .ThenBy(c => c.Title, StringComparer.CurrentCulture)
                .Select(c => BuildNode(c, depth + 1))
                .ToList();

            List<HypothesisResult> hypotheses = lookups.Hypotheses?[link.Code].ToList() ?? [];
            List<FeedbackResult> feedbacks = lookups.Feedbacks?[link.Code].ToList() ?? [];

            NodeDetails? details = options.IncludeHistory || options.IncludeHypotheses || options.IncludeFeedbacks
                ? new NodeDetails(
                    options.IncludeHistory ? ViewMapper.History(link.History) : null,
                    options.IncludeHypotheses && lookups.Hypotheses is not null
                        ? hypotheses.OrderBy(h => h.CreationDate).Select(ViewMapper.Hypothesis).ToList()
                        : null,
                    options.IncludeFeedbacks && lookups.Feedbacks is not null
                        ? feedbacks.OrderBy(f => f.CreationDate).Select(f => ViewMapper.Feedback(f, lookups.Users)).ToList()
                        : null)
                : null;

            return new TreeNodeView
            {
                Code = link.Code,
                FatherCode = link.FatherCode,
                Type = ViewMapper.TypeOf(link),
                Title = link.Title,
                Description = link.Description,
                Status = ViewMapper.StatusOf(link),
                Validation = ViewMapper.ValidationOf(link),
                Reason = link.Reason,
                TeamCode = link.TeamCode,
                Author = ViewMapper.Person(link.AutorCode, lookups.Users),
                CreationDate = link.CreationDate,
                Depth = depth,
                IsOnSelectedPath = selectedPath.Contains(link.Code),
                Hypotheses = options.IncludeStats && lookups.Hypotheses is not null ? ViewMapper.HypothesisStats(hypotheses) : null,
                Feedback = options.IncludeStats && lookups.Feedbacks is not null ? ViewMapper.FeedbackStats(feedbacks) : null,
                Actions = ViewMapper.ActionsFor(link),
                Details = details,
                Children = childViews
            };
        }

        TreeNodeView rootView = BuildNode(root, 0);
        int maxDepth = MaxDepth(rootView);

        List<HypothesisResult> allHypotheses = included.SelectMany(l => lookups.Hypotheses?[l.Code] ?? []).ToList();
        List<FeedbackResult> allFeedbacks = included.SelectMany(l => lookups.Feedbacks?[l.Code] ?? []).ToList();

        TreeSummary summary = new()
        {
            TotalNodes = included.Count,
            ByStatus = LinkStatuses.All.ToDictionary(s => s, s => included.Count(l => ViewMapper.StatusOf(l) == s)),
            ByType = LinkTypes.All.ToDictionary(t => t, t => included.Count(l => ViewMapper.TypeOf(l) == t)),
            MaxDepth = maxDepth,
            SelectedPath = SelectedPath(rootCode, flat),
            FinalCode = flat.FirstOrDefault(l => ViewMapper.StatusOf(l) == LinkStatuses.Final)?.Code,
            Hypotheses = lookups.Hypotheses is null ? null : ViewMapper.HypothesisStats(allHypotheses),
            Feedback = lookups.Feedbacks is null ? null : ViewMapper.FeedbackStats(allFeedbacks)
        };

        return (rootView, summary);
    }

    private static ILookup<string, LinkResult> ChildrenLookup(IEnumerable<LinkResult> flat) =>
        flat.Where(l => !string.IsNullOrWhiteSpace(l.FatherCode))
            .ToLookup(l => l.FatherCode!, StringComparer.Ordinal);

    private static int MaxDepth(TreeNodeView node) =>
        node.Children.Count == 0 ? node.Depth : node.Children.Max(MaxDepth);

    /// <summary>Percorre a árvore em pre-ordem (util para exportar).</summary>
    public static IEnumerable<TreeNodeView> Flatten(TreeNodeView node)
    {
        yield return node;
        foreach (TreeNodeView child in node.Children)
        {
            foreach (TreeNodeView descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }
}
