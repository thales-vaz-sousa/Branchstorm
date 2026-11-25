using BffApiClassLibrary.Services.Export;
using Microsoft.AspNetCore.Mvc;

namespace BffServiceApi.Controllers;

/// <summary>Exportação da árvore (RF12). Somente leitura: exportar nunca altera dados.</summary>
[ApiController]
[Route("bff/export")]
public class ExportController(IExportService exportService) : ControllerBase
{
    private readonly IExportService exportService = exportService;

    /// <summary>
    /// format=json devolve os dados agregados (para o front gerar o que quiser);
    /// markdown | csv | mermaid devolvem o arquivo pronto. download=true forca o "Salvar como".
    /// </summary>
    [HttpGet("{code}")]
    public async Task<IActionResult> Export(
        [FromRoute] string code,
        [FromQuery] string scope = ExportScopes.Full,
        [FromQuery] string format = ExportFormats.Json,
        [FromQuery] bool includeDiscarded = true,
        [FromQuery] bool includeHistory = false,
        [FromQuery] bool includeHypotheses = true,
        [FromQuery] bool includeFeedback = true,
        [FromQuery] string profile = "software",
        [FromQuery] bool download = false,
        CancellationToken ct = default)
    {
        ExportOptions options = new()
        {
            Scope = scope,
            Format = format,
            IncludeDiscarded = includeDiscarded,
            IncludeHistory = includeHistory,
            IncludeHypotheses = includeHypotheses,
            IncludeFeedback = includeFeedback,
            Profile = profile
        };

        if (format.Trim().Equals(ExportFormats.Json, StringComparison.OrdinalIgnoreCase))
        {
            return Ok(await exportService.BuildAsync(code, options, ct));
        }

        ExportFile file = await exportService.RenderAsync(code, options, ct);
        return download
            ? File(file.Content, file.ContentType, file.FileName)
            : File(file.Content, file.ContentType);
    }

    /// <summary>Perfis de vocabulario disponiveis (RF13), para o seletor do front.</summary>
    [HttpGet("profiles")]
    public ActionResult<IReadOnlyList<VocabularyProfile>> FindProfiles() => Ok(Profiles.All);
}
