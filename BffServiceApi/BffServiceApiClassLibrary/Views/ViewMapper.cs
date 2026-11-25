using BffApiClassLibrary.Clients.Feedback;
using BffApiClassLibrary.Clients.Hypothesis;
using BffApiClassLibrary.Clients.Idea;
using BffApiClassLibrary.Clients.User;
using BffApiClassLibrary.Common;
using static BffApiClassLibrary.Common.Vocabulary;

namespace BffApiClassLibrary.Views;

/// <summary>Conversoes dos contratos dos microsserviços para os view models do front.</summary>
public static class ViewMapper
{
    public static string TypeOf(LinkResult link) => Normalize(link.Type);
    public static string StatusOf(LinkResult link) => Normalize(link.Status);
    public static string ValidationOf(LinkResult link) => Normalize(link.Validation);

    public static bool IsSelectedOrFinal(LinkResult link) =>
        StatusOf(link) is LinkStatuses.Selected or LinkStatuses.Final;

    public static PersonRef? Person(string? code, IReadOnlyDictionary<string, UserResult> users) =>
        string.IsNullOrWhiteSpace(code)
            ? null
            : new PersonRef(code, users.TryGetValue(code, out UserResult? user) ? user.Name : null);

    public static NodeActions ActionsFor(LinkResult link)
    {
        string status = StatusOf(link);
        string type = TypeOf(link);
        bool discarded = status == LinkStatuses.Discarded;

        return new NodeActions(
            CanAddChild: !discarded,
            SuggestedChildType: discarded ? null : type == LinkTypes.Problem ? LinkTypes.Solution : LinkTypes.Version,
            // Selecionar so faz sentido para filhos (o problema raiz não tem irmaos) que ainda não estao nó caminho.
            CanSelect: !string.IsNullOrWhiteSpace(link.FatherCode)
                       && status is not (LinkStatuses.Selected or LinkStatuses.Final),
            // Regra de negocio: feedback so e aceito em nos com status "aberto".
            CanReceiveFeedback: status == LinkStatuses.Open,
            CanAddHypothesis: !discarded);
    }

    public static HistoryItem History(LinkHistoryResult h) => new(h.Code, h.Event, h.CreationDate);

    public static IReadOnlyList<HistoryItem> History(IEnumerable<LinkHistoryResult> history) =>
        history.OrderBy(h => h.CreationDate).Select(History).ToList();

    public static HypothesisView Hypothesis(HypothesisResult h) => new(
        h.Code, h.LinkCode, h.Description, h.SuccessCriteria,
        HypothesisStatus(h.Result), h.Result, h.CreationDate);

    public static FeedbackView Feedback(FeedbackResult f, IReadOnlyDictionary<string, UserResult> users) => new(
        f.Code, f.LinkCode,
        Person(f.AutorCode, users) ?? new PersonRef(f.AutorCode, null),
        f.Text, f.Rating, f.CreationDate);

    public static HypothesisStats HypothesisStats(IEnumerable<HypothesisResult> hypotheses)
    {
        List<string> statuses = hypotheses.Select(h => HypothesisStatus(h.Result)).ToList();
        return new HypothesisStats(
            statuses.Count,
            statuses.Count(s => s == HypothesisStatuses.Validated),
            statuses.Count(s => s == HypothesisStatuses.Refuted),
            statuses.Count(s => s == HypothesisStatuses.Pending));
    }

    public static FeedbackStats FeedbackStats(IEnumerable<FeedbackResult> feedbacks)
    {
        List<decimal> ratings = feedbacks.Select(f => f.Rating).ToList();
        return new FeedbackStats(ratings.Count, ratings.Count == 0 ? null : Math.Round(ratings.Average(), 2));
    }

    public static RatingSummary Rating(IEnumerable<FeedbackResult> feedbacks)
    {
        List<decimal> ratings = feedbacks.Select(f => f.Rating).ToList();
        Dictionary<int, int> distribution = Enumerable.Range(1, 5).ToDictionary(n => n, _ => 0);
        foreach (decimal rating in ratings)
        {
            int bucket = (int)Math.Clamp(Math.Round(rating, MidpointRounding.AwayFromZero), 1, 5);
            distribution[bucket]++;
        }

        return new RatingSummary(
            ratings.Count,
            ratings.Count == 0 ? null : Math.Round(ratings.Average(), 2),
            distribution);
    }
}
