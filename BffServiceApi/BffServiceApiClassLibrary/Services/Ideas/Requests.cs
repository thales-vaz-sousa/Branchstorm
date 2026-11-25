using System.ComponentModel.DataAnnotations;

namespace BffApiClassLibrary.Services.Ideas;

// Corpos de requisicao do BFF (o que o Angular envia). Validados antes de chamar os microsserviços.

public record CreateIdeaRequest
{
    [Required(ErrorMessage = "Title é obrigatório")]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; init; } = string.Empty;

    /// <summary>Markdown livre ou gerado a partir de um modelo de problema (front).</summary>
    public string? Description { get; init; }

    public string? AutorCode { get; init; }
    public string? TeamCode { get; init; }
    public string? Reason { get; init; }
}

public record AddChildRequest
{
    [Required(ErrorMessage = "Title é obrigatório")]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    /// <summary>"solution" ou "version". Se omitido: filho de problema = solution, demais = version.</summary>
    public string? Type { get; init; }

    public string? AutorCode { get; init; }

    /// <summary>Se omitido, herda a equipe do nó pai.</summary>
    public string? TeamCode { get; init; }

    /// <summary>RF04: o motivo da nova solução/versão.</summary>
    [Required(ErrorMessage = "Reason é obrigatório (RF04: informe o motivo)")]
    public string Reason { get; init; } = string.Empty;
}

public record SelectLinkRequest
{
    public string? Reason { get; init; }
}

public record UpdateValidationRequest
{
    /// <summary>"not_tested" | "validated" | "refuted"</summary>
    [Required(ErrorMessage = "Validation é obrigatório")]
    public string Validation { get; init; } = string.Empty;

    public string? Reason { get; init; }
}

public record AddHypothesisRequest
{
    [Required(ErrorMessage = "Description é obrigatório")]
    public string Description { get; init; } = string.Empty;

    [Required(ErrorMessage = "SuccessCriteria é obrigatório")]
    public string SuccessCriteria { get; init; } = string.Empty;
}

public record RegisterHypothesisResultRequest
{
    /// <summary>"validated" | "refuted"</summary>
    [Required(ErrorMessage = "Result é obrigatório")]
    public string Result { get; init; } = string.Empty;
}

public record AddFeedbackRequest
{
    [Required(ErrorMessage = "AutorCode é obrigatório")]
    public string AutorCode { get; init; } = string.Empty;

    [Required(ErrorMessage = "Text é obrigatório")]
    public string Text { get; init; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating deve ser entre 1 e 5")]
    public decimal Rating { get; init; }
}
