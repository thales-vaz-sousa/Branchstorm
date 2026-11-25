using BffApiClassLibrary.Services.Ideas;
using BffApiClassLibrary.Views;
using Microsoft.AspNetCore.Mvc;

namespace BffServiceApi.Controllers;

/// <summary>Tela da árvore de uma ideia (RF03, RF06).</summary>
[ApiController]
[Route("bff/ideas")]
public class IdeasController(
    IIdeaTreeService treeService,
    IIdeaCommandService commandService
    ) : ControllerBase
{
    private readonly IIdeaTreeService treeService = treeService;
    private readonly IIdeaCommandService commandService = commandService;

    /// <summary>Cria um problema/ideia raiz e ja devolve a árvore (com um no so).</summary>
    [HttpPost]
    public async Task<ActionResult<IdeaTreeView>> Create([FromBody] CreateIdeaRequest request, CancellationToken ct)
    {
        IdeaTreeView tree = await commandService.CreateIdeaAsync(request, ct);
        return Created($"/bff/ideas/{tree.RootCode}/tree", tree);
    }

    /// <summary>
    /// Árvore completa, ja aninhada, com autores, contadores de hipóteses/feedback,
    /// caminho selecionado e ações por nó. Aceita o código de qualquer no: sobe ate a raiz e marca o foco.
    /// </summary>
    [HttpGet("{code}/tree")]
    public async Task<ActionResult<IdeaTreeView>> FindTree(
        [FromRoute] string code,
        [FromQuery] bool includeDiscarded = true,
        [FromQuery] bool includeStats = true,
        CancellationToken ct = default)
    {
        TreeBuildOptions options = new() { IncludeDiscarded = includeDiscarded, IncludeStats = includeStats };
        return Ok(await treeService.GetTreeAsync(code, options, ct));
    }
}
