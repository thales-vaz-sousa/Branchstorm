using System.Text;
using BffApiClassLibrary.Common;
using BffApiClassLibrary.Services.Ideas;
using BffApiClassLibrary.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BffApiClassLibrary.Services.Export;

public interface IExportService
{
    /// <summary>Agregacao somente leitura (README 2.4.3): árvore + hipóteses + feedbacks + autores.</summary>
    Task<ExportView> BuildAsync(string code, ExportOptions options, CancellationToken ct = default);

    /// <summary>Mesmo conteudo renderizado em Markdown, CSV ou Mermaid. Exportar nunca altera dados.</summary>
    Task<ExportFile> RenderAsync(string code, ExportOptions options, CancellationToken ct = default);
}

public class ExportService(IIdeaTreeService treeService, Clients.Idea.IIdeaApiClient ideas) : IExportService
{
    public async Task<ExportView> BuildAsync(string code, ExportOptions options, CancellationToken ct = default)
    {
        options = Validate(options);

        TreeBuildOptions build = new()
        {
            IncludeDiscarded = options.IncludeDiscarded,
            IncludeStats = true,
            IncludeHistory = options.IncludeHistory,
            IncludeHypotheses = options.IncludeHypotheses,
            IncludeFeedbacks = options.IncludeFeedback
        };

        TreeData data = options.Scope switch
        {
            ExportScopes.Selected => await treeService.LoadSelectedPathAsync(code, build, ct),
            ExportScopes.Subtree => await treeService.LoadAsync(code, resolveRoot: false, build, ct),
            _ => await treeService.LoadAsync(code, resolveRoot: true, build, ct)
        };

        // No escopo "subtree" a raiz exportada e o proprio no: os ancestrais viram contexto ("Parte de").
        IReadOnlyList<BreadcrumbItem> context = [];
        if (options.Scope == ExportScopes.Subtree && !string.IsNullOrWhiteSpace(data.Focus.FatherCode))
        {
            context = (await LinkPathResolver.GetAncestorsAsync(ideas, data.Focus, ct))
                .Select(a => new BreadcrumbItem(a.Code, ViewMapper.TypeOf(a), a.Title, ViewMapper.StatusOf(a)))
                .ToList();
        }

        return new ExportView
        {
            GeneratedAt = DateTime.UtcNow,
            RequestedCode = code,
            RootCode = data.Root.Code,
            Options = options,
            Profile = Profiles.Find(options.Profile),
            Context = context,
            Team = data.Team,
            Summary = data.Summary,
            Tree = data.RootView,
            Warnings = data.Warnings
        };
    }

    public async Task<ExportFile> RenderAsync(string code, ExportOptions options, CancellationToken ct = default)
    {
        ExportView export = await BuildAsync(code, options, ct);
        string baseName = $"branchstorm-{Slug(export.Tree.Title)}-{export.Options.Scope}";

        return export.Options.Format switch
        {
            ExportFormats.Markdown => Text(MarkdownExporter.Render(export), $"{baseName}.md", "text/markdown; charset=utf-8"),
            // BOM para o Excel reconhecer UTF-8 (acentos).
            ExportFormats.Csv => Text(CsvExporter.Render(export), $"{baseName}.csv", "text/csv; charset=utf-8", bom: true),
            ExportFormats.Mermaid => Text(MermaidExporter.Render(export), $"{baseName}.mmd", "text/plain; charset=utf-8"),
            _ => throw BffException.BadRequest("Use BuildAsync para o formato json.")
        };
    }

    private static ExportOptions Validate(ExportOptions options)
    {
        string scope = options.Scope.Trim().ToLowerInvariant();
        string format = options.Format.Trim().ToLowerInvariant() switch
        {
            "md" => ExportFormats.Markdown,
            "mmd" => ExportFormats.Mermaid,
            var f => f
        };

        if (!ExportScopes.All.Contains(scope))
            throw BffException.BadRequest($"scope deve ser um de: {string.Join(", ", ExportScopes.All)}.");
        if (!ExportFormats.All.Contains(format))
            throw BffException.BadRequest($"format deve ser um de: {string.Join(", ", ExportFormats.All)}.");

        return options with { Scope = scope, Format = format, Profile = Profiles.Find(options.Profile).Key };
    }

    private static ExportFile Text(string content, string fileName, string contentType, bool bom = false)
    {
        byte[] body = Encoding.UTF8.GetBytes(content);
        if (bom) body = [.. Encoding.UTF8.GetPreamble(), .. body];
        return new ExportFile(fileName, contentType, body);
    }

    private static string Slug(string value)
    {
        string normalized = value.Normalize(NormalizationForm.FormD);
        StringBuilder slug = new();
        foreach (char c in normalized)
        {
            if (char.IsAsciiLetterOrDigit(c)) slug.Append(char.ToLowerInvariant(c));
            else if ((c == ' ' || c == '-' || c == '_') && slug.Length > 0 && slug[^1] != '-') slug.Append('-');
        }

        string result = slug.ToString().Trim('-');
        if (result.Length > 40) result = result[..40].Trim('-');
        return result.Length == 0 ? "ideia" : result;
    }
}

public static class ExportServiceUtil
{
    public static IServiceCollection UseExportService(this IServiceCollection services)
    {
        services.AddTransient<IExportService, ExportService>();
        return services;
    }
}
