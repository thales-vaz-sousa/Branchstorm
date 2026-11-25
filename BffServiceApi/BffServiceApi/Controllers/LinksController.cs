using BffApiClassLibrary.Services.Ideas;
using BffApiClassLibrary.Views;
using Microsoft.AspNetCore.Mvc;

namespace BffServiceApi.Controllers;

/// <summary>Painel de um nó (link) e as ações sobre ele (RF04, RF05, RF07, RF09, RF10, RF11).</summary>
[ApiController]
[Route("bff/links")]
public class LinksController(
    ILinkDetailService detailService,
    IIdeaCommandService commandService
    ) : ControllerBase
{
    private readonly ILinkDetailService detailService = detailService;
    private readonly IIdeaCommandService commandService = commandService;

    /// <summary>Dados do nó + trilha + filhos + histórico + hipóteses + feedbacks (com autores) + notas.</summary>
    [HttpGet("{code}")]
    public async Task<ActionResult<LinkDetailView>> FindOne([FromRoute] string code, CancellationToken ct)
    {
        return Ok(await detailService.GetAsync(code, ct));
    }

    /// <summary>Propoe uma solução/versão filha do nó. Devolve a árvore atualizada com foco nó novo no.</summary>
    [HttpPost("{code}/children")]
    public async Task<ActionResult<IdeaTreeView>> AddChild(
        [FromRoute] string code, [FromBody] AddChildRequest request, CancellationToken ct)
    {
        IdeaTreeView tree = await commandService.AddChildAsync(code, request, ct);
        return Created($"/bff/links/{tree.FocusCode}", tree);
    }

    /// <summary>Seleciona o nó (os irmãos viram descartados). Devolve a árvore atualizada.</summary>
    [HttpPut("{code}/select")]
    public async Task<ActionResult<IdeaTreeView>> Select(
        [FromRoute] string code, [FromBody] SelectLinkRequest? request, CancellationToken ct)
    {
        return Ok(await commandService.SelectAsync(code, request ?? new SelectLinkRequest(), ct));
    }

    /// <summary>Atualiza a validação do nó manualmente (not_tested | validated | refuted).</summary>
    [HttpPut("{code}/validation")]
    public async Task<ActionResult<LinkDetailView>> UpdateValidation(
        [FromRoute] string code, [FromBody] UpdateValidationRequest request, CancellationToken ct)
    {
        return Ok(await commandService.UpdateValidationAsync(code, request, ct));
    }

    /// <summary>Cadastra uma hipótese no nó. Devolve o painel do nó atualizado.</summary>
    [HttpPost("{code}/hypotheses")]
    public async Task<ActionResult<LinkDetailView>> AddHypothesis(
        [FromRoute] string code, [FromBody] AddHypothesisRequest request, CancellationToken ct)
    {
        LinkDetailView detail = await commandService.AddHypothesisAsync(code, request, ct);
        return Created($"/bff/links/{code}", detail);
    }

    /// <summary>Comenta e da nota (1 a 5). Retorna 409 se o nó não estiver aberto.</summary>
    [HttpPost("{code}/feedbacks")]
    public async Task<ActionResult<LinkDetailView>> AddFeedback(
        [FromRoute] string code, [FromBody] AddFeedbackRequest request, CancellationToken ct)
    {
        LinkDetailView detail = await commandService.AddFeedbackAsync(code, request, ct);
        return Created($"/bff/links/{code}", detail);
    }
}

/// <summary>Resultado de hipótese (RF08 → RF09 automatico pelo serviço de Hipóteses).</summary>
[ApiController]
[Route("bff/hypotheses")]
public class HypothesesController(IIdeaCommandService commandService) : ControllerBase
{
    private readonly IIdeaCommandService commandService = commandService;

    /// <summary>Registra validated/refuted e devolve o painel do nó, ja com a validação atualizada.</summary>
    [HttpPut("{code}/result")]
    public async Task<ActionResult<LinkDetailView>> RegisterResult(
        [FromRoute] string code, [FromBody] RegisterHypothesisResultRequest request, CancellationToken ct)
    {
        return Ok(await commandService.RegisterHypothesisResultAsync(code, request, ct));
    }
}
