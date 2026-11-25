using System.Text;

namespace BffApiClassLibrary.Common;

/// <summary>
/// Os microsserviços gravam os enums de formas diferentes ("Open", "NotTested", "Problem", "validated"...).
/// O BFF entrega tudo padronizado em snake_case minusculo para o Angular.
/// </summary>
public static class Vocabulary
{
    public static class LinkTypes
    {
        public const string Problem = "problem";
        public const string Solution = "solution";
        public const string Version = "version";
        public static readonly string[] All = [Problem, Solution, Version];
    }

    public static class LinkStatuses
    {
        public const string Open = "open";
        public const string Selected = "selected";
        public const string Discarded = "discarded";
        public const string Final = "final";
        public static readonly string[] All = [Open, Selected, Discarded, Final];
    }

    public static class Validations
    {
        public const string NotTested = "not_tested";
        public const string Validated = "validated";
        public const string Refuted = "refuted";
        public static readonly string[] All = [NotTested, Validated, Refuted];
    }

    public static class HypothesisStatuses
    {
        public const string Pending = "pending";
        public const string Validated = "validated";
        public const string Refuted = "refuted";
    }

    public static class TeamRoles
    {
        public const string Admin = "Admin";
        public const string Owner = "Owner";
        public const string Collaborator = "Collaborator";
        public const string Evaluator = "Evaluator";
        public static readonly string[] All = [Admin, Owner, Collaborator, Evaluator];

        /// <summary>Aceita "admin", "ADMIN", "Admin" ou o numero do enum e devolve o nome esperado pelo serviço de Equipes.</summary>
        public static string? Parse(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (int.TryParse(value, out int index) && index >= 0 && index < All.Length) return All[index];
            return All.FirstOrDefault(r => r.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>"NotTested" → "not_tested", "Open" → "open", "not_tested" → "not_tested".</summary>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        string trimmed = value.Trim();
        if (trimmed.Contains('_') || trimmed.All(c => !char.IsUpper(c)))
        {
            return trimmed.ToLowerInvariant();
        }

        StringBuilder builder = new(trimmed.Length + 4);
        for (int i = 0; i < trimmed.Length; i++)
        {
            char c = trimmed[i];
            if (char.IsUpper(c) && i > 0 && !char.IsUpper(trimmed[i - 1]))
            {
                builder.Append('_');
            }
            builder.Append(char.ToLowerInvariant(c));
        }
        return builder.ToString();
    }

    /// <summary>Resultado de hipótese (texto livre no serviço) → pending | validated | refuted.</summary>
    public static string HypothesisStatus(string? result)
    {
        string normalized = Normalize(result);
        return normalized switch
        {
            "" or "not_tested" or "pending" => HypothesisStatuses.Pending,
            "validated" or "validate" or "valid" => HypothesisStatuses.Validated,
            "refuted" or "refute" => HypothesisStatuses.Refuted,
            _ => normalized
        };
    }
}
