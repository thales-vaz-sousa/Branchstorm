using BffApiClassLibrary.Views;

namespace BffApiClassLibrary.Services.Export;

public static class ExportScopes
{
    /// <summary>Árvore inteira (a partir da raiz do nó informado).</summary>
    public const string Full = "full";
    /// <summary>So o caminho selecionado (raiz → selecionados → final).</summary>
    public const string Selected = "selected";
    /// <summary>O nó informado e seus descendentes.</summary>
    public const string Subtree = "subtree";

    public static readonly string[] All = [Full, Selected, Subtree];
}

public static class ExportFormats
{
    public const string Json = "json";
    public const string Markdown = "markdown";
    public const string Csv = "csv";
    public const string Mermaid = "mermaid";

    public static readonly string[] All = [Json, Markdown, Csv, Mermaid];
}

/// <summary>Parametros de GET /bff/export/{code} (RF12).</summary>
public record ExportOptions
{
    public string Scope { get; init; } = ExportScopes.Full;
    public string Format { get; init; } = ExportFormats.Json;
    public bool IncludeDiscarded { get; init; } = true;
    public bool IncludeHistory { get; init; }
    public bool IncludeHypotheses { get; init; } = true;
    public bool IncludeFeedback { get; init; } = true;
    public string Profile { get; init; } = "software";
}

public record ExportView
{
    public DateTime GeneratedAt { get; init; }
    public string RequestedCode { get; init; } = string.Empty;
    public string RootCode { get; init; } = string.Empty;
    public ExportOptions Options { get; init; } = new();
    public VocabularyProfile Profile { get; init; } = Profiles.All[0];
    public IReadOnlyList<BreadcrumbItem> Context { get; init; } = [];
    public TeamRef? Team { get; init; }
    public TreeSummary Summary { get; init; } = new();
    public TreeNodeView Tree { get; init; } = new();
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

/// <summary>Arquivo pronto para download (formatos de texto).</summary>
public record ExportFile(string FileName, string ContentType, byte[] Content);
