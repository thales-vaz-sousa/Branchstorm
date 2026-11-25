namespace BffApiClassLibrary.Views;

// ---------------------------------------------------------------------------
// Blocos reutilizados pelas telas
// ---------------------------------------------------------------------------

/// <summary>Referencia a uma pessoa ja com o nome resolvido (Name fica null se o serviço de Usuários falhar).</summary>
public record PersonRef(string Code, string? Name);

public record TeamRef(string Code, string? Name);

public record HypothesisStats(int Total, int Validated, int Refuted, int Pending);

public record FeedbackStats(int Count, decimal? AverageRating);

/// <summary>
/// Dicas de interface calculadas a partir das regras de negocio do README
/// (ex.: feedback so em no aberto). O front so mostra/esconde botoes.
/// </summary>
public record NodeActions(
    bool CanAddChild,
    string? SuggestedChildType,
    bool CanSelect,
    bool CanReceiveFeedback,
    bool CanAddHypothesis);

public record HistoryItem(string Code, string Event, DateTime CreationDate);

public record HypothesisView(
    string Code,
    string LinkCode,
    string Description,
    string SuccessCriteria,
    string Status,
    string? Result,
    DateTime CreationDate);

public record FeedbackView(
    string Code,
    string LinkCode,
    PersonRef Author,
    string Text,
    decimal Rating,
    DateTime CreationDate);

public record RatingSummary(int Count, decimal? Average, IReadOnlyDictionary<int, int> Distribution);

// ---------------------------------------------------------------------------
// Tela da árvore (RF06): GET /bff/ideas/{code}/tree
// ---------------------------------------------------------------------------

public record TreeNodeView
{
    public string Code { get; init; } = string.Empty;
    public string? FatherCode { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Validation { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public string? TeamCode { get; init; }
    public PersonRef? Author { get; init; }
    public DateTime CreationDate { get; init; }
    public int Depth { get; init; }
    public bool IsOnSelectedPath { get; init; }
    public HypothesisStats? Hypotheses { get; init; }
    public FeedbackStats? Feedback { get; init; }
    public NodeActions Actions { get; init; } = new(false, null, false, false, false);

    /// <summary>Preenchido apenas na exportação, conforme as opcoes pedidas.</summary>
    public NodeDetails? Details { get; init; }

    public IReadOnlyList<TreeNodeView> Children { get; init; } = [];
}

public record NodeDetails(
    IReadOnlyList<HistoryItem>? History,
    IReadOnlyList<HypothesisView>? Hypotheses,
    IReadOnlyList<FeedbackView>? Feedbacks);

public record TreeSummary
{
    public int TotalNodes { get; init; }
    public IReadOnlyDictionary<string, int> ByStatus { get; init; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> ByType { get; init; } = new Dictionary<string, int>();
    public int MaxDepth { get; init; }
    public IReadOnlyList<string> SelectedPath { get; init; } = [];
    public string? FinalCode { get; init; }
    public HypothesisStats? Hypotheses { get; init; }
    public FeedbackStats? Feedback { get; init; }
}

public record IdeaTreeView
{
    public string RootCode { get; init; } = string.Empty;
    /// <summary>No que foi pedido na URL (pode ser um no interno; a árvore volta sempre a partir da raiz).</summary>
    public string FocusCode { get; init; } = string.Empty;
    public TeamRef? Team { get; init; }
    public TreeSummary Summary { get; init; } = new();
    public TreeNodeView Root { get; init; } = new();
    /// <summary>Dados complementares que não puderam ser carregados (a tela continua funcionando sem eles).</summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

// ---------------------------------------------------------------------------
// Tela de detalhe do nó: GET /bff/links/{code}
// ---------------------------------------------------------------------------

public record BreadcrumbItem(string Code, string Type, string Title, string Status);

public record ChildSummary(string Code, string Type, string Title, string Status, string Validation, DateTime CreationDate);

public record LinkView
{
    public string Code { get; init; } = string.Empty;
    public string? FatherCode { get; init; }
    public string RootCode { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Validation { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public PersonRef? Author { get; init; }
    public TeamRef? Team { get; init; }
    public DateTime CreationDate { get; init; }
}

public record LinkDetailView
{
    public LinkView Link { get; init; } = new();
    public IReadOnlyList<BreadcrumbItem> Breadcrumb { get; init; } = [];
    public IReadOnlyList<ChildSummary> Children { get; init; } = [];
    public IReadOnlyList<HistoryItem> History { get; init; } = [];
    public IReadOnlyList<HypothesisView> Hypotheses { get; init; } = [];
    public HypothesisStats HypothesisStats { get; init; } = new(0, 0, 0, 0);
    public IReadOnlyList<FeedbackView> Feedbacks { get; init; } = [];
    public RatingSummary Rating { get; init; } = new(0, null, new Dictionary<int, int>());
    public NodeActions Actions { get; init; } = new(false, null, false, false, false);
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

// ---------------------------------------------------------------------------
// Tela de equipe: GET /bff/teams/{code}
// ---------------------------------------------------------------------------

public record TeamMemberView(string UserCode, string? Name, string? Email, string Role, bool UserFound);

public record TeamView
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IReadOnlyList<TeamMemberView> Members { get; init; } = [];
    public IReadOnlyDictionary<string, int> CountByRole { get; init; } = new Dictionary<string, int>();
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

// ---------------------------------------------------------------------------
// Saude: GET /bff/health
// ---------------------------------------------------------------------------

public record ServiceHealth(string Service, string Status, long LatencyMs, string? Error);

public record HealthView(string Status, DateTime CheckedAt, IReadOnlyList<ServiceHealth> Services);
