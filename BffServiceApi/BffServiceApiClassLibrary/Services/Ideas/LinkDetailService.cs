using BffApiClassLibrary.Clients.Feedback;
using BffApiClassLibrary.Clients.Hypothesis;
using BffApiClassLibrary.Clients.Idea;
using BffApiClassLibrary.Clients.Team;
using BffApiClassLibrary.Clients.User;
using BffApiClassLibrary.Services.Common;
using BffApiClassLibrary.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BffApiClassLibrary.Services.Ideas;

public interface ILinkDetailService
{
    /// <summary>
    /// Painel de um nó: dados, autor, equipe, trilha (raiz → no), filhos diretos, histórico,
    /// hipóteses, feedbacks com nome do autor, resumo das notas e ações permitidas.
    /// </summary>
    Task<LinkDetailView> GetAsync(string code, CancellationToken ct = default);
}

public class LinkDetailService(
    IIdeaApiClient ideas,
    IHypothesisApiClient hypotheses,
    IFeedbackApiClient feedbacks,
    IUserApiClient users,
    ITeamApiClient teams) : ILinkDetailService
{
    public async Task<LinkDetailView> GetAsync(string code, CancellationToken ct = default)
    {
        LinkResult link = await LinkPathResolver.FindRequiredAsync(ideas, code, ct);
        WarningCollector warnings = new();

        Task<IReadOnlyList<LinkResult>> ancestorsTask = warnings.TryAsync(
            () => LinkPathResolver.GetAncestorsAsync(ideas, link, ct), [], "Trilha até a raiz");
        Task<IReadOnlyList<LinkResult>> subtreeTask = warnings.TryAsync(
            () => ideas.FindTreeAsync(link.Code, ct), [], "Filhos do nó");
        Task<IReadOnlyList<HypothesisResult>> hypothesesTask = warnings.TryAsync(
            () => hypotheses.FindByLinkAsync(link.Code, ct), [], "Hipóteses");
        Task<IReadOnlyList<FeedbackResult>> feedbacksTask = warnings.TryAsync(
            () => feedbacks.FindByLinksAsync([link.Code], ct), [], "Feedbacks");
        Task<TeamRef?> teamTask = TeamRefResolver.ResolveAsync(teams, link.TeamCode, warnings, ct);

        await Task.WhenAll(ancestorsTask, subtreeTask, hypothesesTask, feedbacksTask, teamTask);

        IReadOnlyList<LinkResult> ancestors = await ancestorsTask;
        IReadOnlyList<FeedbackResult> feedbackList = await feedbacksTask;
        IReadOnlyList<HypothesisResult> hypothesisList = await hypothesesTask;

        IReadOnlyDictionary<string, UserResult> userMap = await UserLookup.ResolveAsync(
            users,
            feedbackList.Select(f => (string?)f.AutorCode).Append(link.AutorCode),
            warnings, ct);

        List<ChildSummary> children = (await subtreeTask)
            .Where(c => c.FatherCode == link.Code)
            .OrderBy(c => c.CreationDate)
            .Select(c => new ChildSummary(c.Code, ViewMapper.TypeOf(c), c.Title,
                ViewMapper.StatusOf(c), ViewMapper.ValidationOf(c), c.CreationDate))
            .ToList();

        List<BreadcrumbItem> breadcrumb = ancestors.Append(link)
            .Select(l => new BreadcrumbItem(l.Code, ViewMapper.TypeOf(l), l.Title, ViewMapper.StatusOf(l)))
            .ToList();

        return new LinkDetailView
        {
            Link = new LinkView
            {
                Code = link.Code,
                FatherCode = link.FatherCode,
                RootCode = ancestors.FirstOrDefault()?.Code ?? link.Code,
                Type = ViewMapper.TypeOf(link),
                Title = link.Title,
                Description = link.Description,
                Status = ViewMapper.StatusOf(link),
                Validation = ViewMapper.ValidationOf(link),
                Reason = link.Reason,
                Author = ViewMapper.Person(link.AutorCode, userMap),
                Team = await teamTask,
                CreationDate = link.CreationDate
            },
            Breadcrumb = breadcrumb,
            Children = children,
            History = ViewMapper.History(link.History),
            Hypotheses = hypothesisList.OrderBy(h => h.CreationDate).Select(ViewMapper.Hypothesis).ToList(),
            HypothesisStats = ViewMapper.HypothesisStats(hypothesisList),
            Feedbacks = feedbackList.OrderByDescending(f => f.CreationDate).Select(f => ViewMapper.Feedback(f, userMap)).ToList(),
            Rating = ViewMapper.Rating(feedbackList),
            Actions = ViewMapper.ActionsFor(link),
            Warnings = warnings.ToList()
        };
    }
}

public static class LinkDetailServiceUtil
{
    public static IServiceCollection UseLinkDetailService(this IServiceCollection services)
    {
        services.AddTransient<ILinkDetailService, LinkDetailService>();
        return services;
    }
}
