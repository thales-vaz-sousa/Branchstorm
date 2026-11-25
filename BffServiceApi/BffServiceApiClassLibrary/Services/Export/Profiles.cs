using static BffApiClassLibrary.Common.Vocabulary;

namespace BffApiClassLibrary.Services.Export;

/// <summary>
/// Perfis de uso (README 2.4.1 / RF13). So mudam os rotulos; dados e regras continuam iguais.
/// </summary>
public record VocabularyProfile(
    string Key,
    string Name,
    string Problem,
    string Solution,
    string Version,
    string Hypothesis,
    string Feedback)
{
    public string TypeLabel(string type) => type switch
    {
        LinkTypes.Problem => Problem,
        LinkTypes.Solution => Solution,
        LinkTypes.Version => Version,
        _ => type
    };
}

public static class Profiles
{
    public static readonly IReadOnlyList<VocabularyProfile> All =
    [
        new("software", "Software (padrão)", "Problema", "Solução", "Versão", "Hipótese", "Feedback"),
        new("tickets", "Tickets e incidentes", "Ticket", "Solução proposta", "Ajuste", "Verificação", "Resultado"),
        new("decisions", "Decisões", "Decisão", "Opção", "Variante", "Teste", "Comentário"),
        new("research", "Pesquisa e estudo", "Pergunta", "Resposta possível", "Refinamento", "Hipótese", "Revisão"),
        new("teaching", "Ensino", "Desafio", "Estratégia", "Adaptação", "Teste em sala", "Observação"),
        new("process", "Melhoria de processos", "Problema", "Ação", "Padrão", "Piloto", "Resultado")
    ];

    public static VocabularyProfile Find(string? key) =>
        All.FirstOrDefault(p => p.Key.Equals(key?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? All[0];

    public static string StatusLabel(string status) => status switch
    {
        LinkStatuses.Open => "aberto",
        LinkStatuses.Selected => "selecionado",
        LinkStatuses.Discarded => "descartado",
        LinkStatuses.Final => "final",
        _ => status
    };

    public static string ValidationLabel(string validation) => validation switch
    {
        Validations.NotTested => "não testado",
        Validations.Validated => "validado",
        Validations.Refuted => "refutado",
        _ => validation
    };

    public static string HypothesisLabel(string status) => status switch
    {
        HypothesisStatuses.Pending => "pendente",
        HypothesisStatuses.Validated => "validada",
        HypothesisStatuses.Refuted => "refutada",
        _ => status
    };
}
