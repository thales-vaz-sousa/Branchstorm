using BffApiClassLibrary.Clients.Feedback;
using BffApiClassLibrary.Clients.Hypothesis;
using BffApiClassLibrary.Clients.Idea;
using BffApiClassLibrary.Clients.User;
using BffApiClassLibrary.Common;
using BffApiClassLibrary.Services.Common;
using BffApiClassLibrary.Views;
using Microsoft.Extensions.DependencyInjection;
using static BffApiClassLibrary.Common.Vocabulary;

namespace BffApiClassLibrary.Services.Ideas;

/// <summary>
/// Comandos da tela de ideias. Cada comando chama o microsserviço dono do dado e devolve
/// a view ja atualizada, poupando o Angular de uma segunda ida ao servidor.
/// </summary>
public interface IIdeaCommandService
{
    Task<IdeaTreeView> CreateIdeaAsync(CreateIdeaRequest request, CancellationToken ct = default);
    Task<IdeaTreeView> AddChildAsync(string fatherCode, AddChildRequest request, CancellationToken ct = default);
    Task<IdeaTreeView> SelectAsync(string code, SelectLinkRequest request, CancellationToken ct = default);
    Task<LinkDetailView> UpdateValidationAsync(string code, UpdateValidationRequest request, CancellationToken ct = default);
    Task<LinkDetailView> AddHypothesisAsync(string linkCode, AddHypothesisRequest request, CancellationToken ct = default);
    Task<LinkDetailView> RegisterHypothesisResultAsync(string hypothesisCode, RegisterHypothesisResultRequest request, CancellationToken ct = default);
    Task<LinkDetailView> AddFeedbackAsync(string linkCode, AddFeedbackRequest request, CancellationToken ct = default);
}

public class IdeaCommandService(
    IIdeaApiClient ideas,
    IHypothesisApiClient hypotheses,
    IFeedbackApiClient feedbacks,
    IUserApiClient users,
    IIdeaTreeService treeService,
    ILinkDetailService detailService) : IIdeaCommandService
{
    private static readonly TreeBuildOptions DefaultTree = new();

    public async Task<IdeaTreeView> CreateIdeaAsync(CreateIdeaRequest request, CancellationToken ct = default)
    {
        await UserGuard.EnsureExistAsync(users, [request.AutorCode], ct);

        LinkResult created = await ideas.CreateAsync(new LinkCreateRequest(
            FatherCode: null,
            Type: LinkTypes.Problem,
            Title: request.Title.Trim(),
            Description: request.Description,
            AutorCode: request.AutorCode,
            TeamCode: request.TeamCode,
            Reason: request.Reason), ct);

        return await treeService.GetTreeAsync(created.Code, DefaultTree, ct);
    }

    public async Task<IdeaTreeView> AddChildAsync(string fatherCode, AddChildRequest request, CancellationToken ct = default)
    {
        LinkResult father = await LinkPathResolver.FindRequiredAsync(ideas, fatherCode, ct);

        string type = string.IsNullOrWhiteSpace(request.Type)
            ? (ViewMapper.TypeOf(father) == LinkTypes.Problem ? LinkTypes.Solution : LinkTypes.Version)
            : Normalize(request.Type);

        if (type is not (LinkTypes.Solution or LinkTypes.Version))
        {
            throw BffException.BadRequest("Type deve ser 'solution' ou 'version' (o problema é sempre a raiz).");
        }

        await UserGuard.EnsureExistAsync(users, [request.AutorCode], ct);

        LinkResult created = await ideas.CreateAsync(new LinkCreateRequest(
            FatherCode: father.Code,
            Type: type,
            Title: request.Title.Trim(),
            Description: request.Description,
            AutorCode: request.AutorCode,
            TeamCode: string.IsNullOrWhiteSpace(request.TeamCode) ? father.TeamCode : request.TeamCode,
            Reason: request.Reason), ct);

        return await treeService.GetTreeAsync(created.Code, DefaultTree, ct);
    }

    public async Task<IdeaTreeView> SelectAsync(string code, SelectLinkRequest request, CancellationToken ct = default)
    {
        // O serviço de Ideias seleciona o nó e descarta os irmãos (sem apagar) numa transacao.
        LinkResult selected = await ideas.SelectAsync(code, request.Reason, ct)
            ?? throw BffException.NotFound("Nó (link)", code);

        return await treeService.GetTreeAsync(selected.Code, DefaultTree, ct);
    }

    public async Task<LinkDetailView> UpdateValidationAsync(string code, UpdateValidationRequest request, CancellationToken ct = default)
    {
        string validation = Normalize(request.Validation);
        if (!Validations.All.Contains(validation))
        {
            throw BffException.BadRequest($"Validation deve ser um de: {string.Join(", ", Validations.All)}.");
        }

        _ = await ideas.UpdateValidationAsync(code, validation, request.Reason, ct)
            ?? throw BffException.NotFound("Nó (link)", code);

        return await detailService.GetAsync(code, ct);
    }

    public async Task<LinkDetailView> AddHypothesisAsync(string linkCode, AddHypothesisRequest request, CancellationToken ct = default)
    {
        // Confere o nó antes, para devolver 404 claro em vez do erro generico do serviço de Hipóteses.
        _ = await LinkPathResolver.FindRequiredAsync(ideas, linkCode, ct);

        await hypotheses.CreateAsync(new HypothesisCreateRequest(
            linkCode, request.Description.Trim(), request.SuccessCriteria.Trim()), ct);

        return await detailService.GetAsync(linkCode, ct);
    }

    public async Task<LinkDetailView> RegisterHypothesisResultAsync(
        string hypothesisCode, RegisterHypothesisResultRequest request, CancellationToken ct = default)
    {
        string result = HypothesisStatus(request.Result);
        if (result is not (HypothesisStatuses.Validated or HypothesisStatuses.Refuted))
        {
            throw BffException.BadRequest("Result deve ser 'validated' ou 'refuted'.");
        }

        // RF08/RF09: o serviço de Hipóteses grava o resultado e ele mesmo atualiza a validação do nó em Ideias.
        HypothesisResult updated = await hypotheses.RegisterResultAsync(hypothesisCode, result, ct);

        return await detailService.GetAsync(updated.LinkCode, ct);
    }

    public async Task<LinkDetailView> AddFeedbackAsync(string linkCode, AddFeedbackRequest request, CancellationToken ct = default)
    {
        LinkResult link = await LinkPathResolver.FindRequiredAsync(ideas, linkCode, ct);

        // RF10 / regra de negocio: feedback so em nos com status "aberto".
        if (ViewMapper.StatusOf(link) != LinkStatuses.Open)
        {
            throw BffException.Conflict(
                $"O nó '{link.Title}' está '{ViewMapper.StatusOf(link)}' e só aceita feedback quando está 'open'.");
        }

        await UserGuard.EnsureExistAsync(users, [request.AutorCode], ct);

        await feedbacks.CreateAsync(new FeedbackCreateRequest(
            linkCode, request.AutorCode, request.Text.Trim(), request.Rating), ct);

        return await detailService.GetAsync(linkCode, ct);
    }
}

public static class IdeaCommandServiceUtil
{
    public static IServiceCollection UseIdeaCommandService(this IServiceCollection services)
    {
        services.AddTransient<IIdeaCommandService, IdeaCommandService>();
        return services;
    }
}
