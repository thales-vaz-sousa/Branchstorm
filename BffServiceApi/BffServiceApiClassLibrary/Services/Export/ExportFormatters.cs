using System.Globalization;
using System.Text;
using BffApiClassLibrary.Services.Ideas;
using BffApiClassLibrary.Views;
using static BffApiClassLibrary.Common.Vocabulary;

namespace BffApiClassLibrary.Services.Export;

public static class MarkdownExporter
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Render(ExportView export)
    {
        VocabularyProfile p = export.Profile;
        TreeNodeView root = export.Tree;
        StringBuilder md = new();

        md.AppendLine($"# {root.Title}");
        md.AppendLine();
        md.AppendLine($"> Exportado do Branchstorm em {export.GeneratedAt.ToString("dd/MM/yyyy HH:mm", PtBr)} (UTC) · " +
                      $"escopo: {ScopeLabel(export.Options.Scope)} · perfil: {p.Name}" +
                      (export.Team is null ? "" : $" · equipe: {export.Team.Name ?? export.Team.Code}"));

        if (export.Context.Count > 0)
        {
            md.AppendLine(">");
            md.AppendLine($"> Parte de: {string.Join(" › ", export.Context.Select(c => c.Title))}");
        }
        md.AppendLine();

        // A descrição do problema ja pode vir com as secoes do modelo em Markdown (README 2.4.2).
        if (!string.IsNullOrWhiteSpace(root.Description))
        {
            md.AppendLine(ShiftHeadings(root.Description.Trim()));
            md.AppendLine();
        }

        md.AppendLine("## Caminho selecionado");
        md.AppendLine();
        List<TreeNodeView> path = TreeBuilder.Flatten(root).Where(n => n.IsOnSelectedPath).ToList();
        if (path.Count <= 1)
        {
            md.AppendLine("_Nenhum ramo selecionado ainda._");
        }
        else
        {
            int step = 1;
            foreach (TreeNodeView node in path)
            {
                md.AppendLine($"{step++}. **{p.TypeLabel(node.Type)}:** {node.Title} — {StateText(node)}");
            }
        }
        md.AppendLine();

        md.AppendLine(export.Options.Scope == ExportScopes.Selected ? "## Detalhes do caminho" : "## Árvore");
        md.AppendLine();
        AppendNode(md, root, 0, export);
        md.AppendLine();

        md.AppendLine($"## {p.Feedback}");
        md.AppendLine();
        if (!export.Options.IncludeFeedback)
        {
            md.AppendLine("(preencher)");
        }
        else
        {
            List<(TreeNodeView Node, FeedbackView Feedback)> all = TreeBuilder.Flatten(root)
                .SelectMany(n => (n.Details?.Feedbacks ?? []).Select(f => (n, f)))
                .ToList();

            if (all.Count == 0)
            {
                md.AppendLine("_Nenhum feedback registrado._");
            }
            else
            {
                if (export.Summary.Feedback?.AverageRating is decimal avg)
                {
                    md.AppendLine($"Nota média: **{avg.ToString("0.0", PtBr)}/5** em {all.Count} avaliação(ões).");
                    md.AppendLine();
                }
                foreach ((TreeNodeView node, FeedbackView feedback) in all)
                {
                    md.AppendLine($"- **{node.Title}** — {feedback.Author.Name ?? feedback.Author.Code} " +
                                  $"({feedback.Rating.ToString("0.#", PtBr)}/5): {OneLine(feedback.Text)}");
                }
            }
        }

        if (export.Warnings.Count > 0)
        {
            md.AppendLine();
            md.AppendLine("---");
            md.AppendLine("_Avisos da exportação:_");
            foreach (string warning in export.Warnings) md.AppendLine($"- {warning}");
        }

        return md.ToString();
    }

    private static void AppendNode(StringBuilder md, TreeNodeView node, int level, ExportView export)
    {
        VocabularyProfile p = export.Profile;
        string indent = new(' ', level * 2);
        string inner = indent + "  ";

        md.AppendLine($"{indent}- **[{p.TypeLabel(node.Type)}] {node.Title}** · {StateText(node)}" +
                      (node.Author is null ? "" : $" · por {node.Author.Name ?? node.Author.Code}"));

        if (level > 0 && !string.IsNullOrWhiteSpace(node.Description))
        {
            foreach (string line in node.Description.Trim().Split('\n'))
                md.AppendLine($"{inner}{line.TrimEnd()}");
        }

        if (!string.IsNullOrWhiteSpace(node.Reason))
            md.AppendLine($"{inner}_Motivo: {OneLine(node.Reason)}_");

        foreach (HypothesisView h in node.Details?.Hypotheses ?? [])
        {
            md.AppendLine($"{inner}- {p.Hypothesis}: {OneLine(h.Description)} · critério: {OneLine(h.SuccessCriteria)} · " +
                          $"**{Profiles.HypothesisLabel(h.Status)}**");
        }

        foreach (HistoryItem item in node.Details?.History ?? [])
        {
            md.AppendLine($"{inner}- Histórico {item.CreationDate.ToString("dd/MM/yyyy HH:mm", PtBr)}: {OneLine(item.Event)}");
        }

        foreach (TreeNodeView child in node.Children)
        {
            AppendNode(md, child, level + 1, export);
        }
    }

    private static string StateText(TreeNodeView node)
    {
        string text = Profiles.StatusLabel(node.Status);
        if (node.Validation != Validations.NotTested) text += $", {Profiles.ValidationLabel(node.Validation)}";
        return text;
    }

    private static string ScopeLabel(string scope) => scope switch
    {
        ExportScopes.Selected => "caminho selecionado",
        ExportScopes.Subtree => "nó e descendentes",
        _ => "árvore inteira"
    };

    /// <summary>Rebaixa "# x" para "## x" na descrição, para não competir com o titulo do documento.</summary>
    private static string ShiftHeadings(string markdown) =>
        string.Join('\n', markdown.Split('\n').Select(l => l.StartsWith('#') ? "#" + l : l));

    private static string OneLine(string value) =>
        value.Replace("\r", " ").Replace("\n", " ").Trim();
}

public static class CsvExporter
{
    private static readonly string[] Header =
    [
        "code", "father_code", "depth", "type", "type_label", "title", "status", "validation",
        "on_selected_path", "author_code", "author_name", "reason", "creation_date",
        "hypotheses_total", "hypotheses_validated", "hypotheses_refuted", "hypotheses_pending",
        "feedback_count", "average_rating"
    ];

    public static string Render(ExportView export)
    {
        StringBuilder csv = new();
        csv.AppendLine(string.Join(',', Header));

        foreach (TreeNodeView n in TreeBuilder.Flatten(export.Tree))
        {
            string?[] row =
            [
                n.Code, n.FatherCode, n.Depth.ToString(CultureInfo.InvariantCulture), n.Type,
                export.Profile.TypeLabel(n.Type), n.Title, n.Status, n.Validation,
                n.IsOnSelectedPath ? "true" : "false", n.Author?.Code, n.Author?.Name, n.Reason,
                n.CreationDate.ToString("o", CultureInfo.InvariantCulture),
                n.Hypotheses?.Total.ToString(CultureInfo.InvariantCulture),
                n.Hypotheses?.Validated.ToString(CultureInfo.InvariantCulture),
                n.Hypotheses?.Refuted.ToString(CultureInfo.InvariantCulture),
                n.Hypotheses?.Pending.ToString(CultureInfo.InvariantCulture),
                n.Feedback?.Count.ToString(CultureInfo.InvariantCulture),
                n.Feedback?.AverageRating?.ToString(CultureInfo.InvariantCulture)
            ];
            csv.AppendLine(string.Join(',', row.Select(Escape)));
        }

        return csv.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        // Evita injecao de formula ao abrir no Excel/Sheets.
        if (value[0] is '=' or '+' or '-' or '@') value = "'" + value;

        return value.IndexOfAny([',', '"', '\n', '\r']) >= 0
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }
}

public static class MermaidExporter
{
    public static string Render(ExportView export)
    {
        StringBuilder mmd = new();
        mmd.AppendLine("flowchart TD");

        Dictionary<string, string> ids = [];
        int next = 0;
        string Id(string code) => ids.TryGetValue(code, out string? id) ? id : ids[code] = $"n{next++}";

        List<TreeNodeView> nodes = TreeBuilder.Flatten(export.Tree).ToList();

        foreach (TreeNodeView n in nodes)
        {
            string label = $"{export.Profile.TypeLabel(n.Type)}: {Truncate(n.Title, 60)}";
            if (n.Validation != Validations.NotTested) label += $" ({Profiles.ValidationLabel(n.Validation)})";
            mmd.AppendLine($"  {Id(n.Code)}[\"{Escape(label)}\"]");
        }

        foreach (TreeNodeView n in nodes)
        {
            foreach (TreeNodeView child in n.Children)
            {
                string edge = child.IsOnSelectedPath ? "==>" : child.Status == LinkStatuses.Discarded ? "-.->" : "-->";
                mmd.AppendLine($"  {Id(n.Code)} {edge} {Id(child.Code)}");
            }
        }

        mmd.AppendLine("  classDef selected fill:#E4F1F3,stroke:#1A8796,stroke-width:2px,color:#0E2A30");
        mmd.AppendLine("  classDef final fill:#1A8796,stroke:#126B77,stroke-width:2px,color:#FFFFFF");
        mmd.AppendLine("  classDef discarded fill:#F4F6F7,stroke:#9DB3B7,stroke-dasharray:4 3,color:#4B6168");

        foreach (string status in new[] { LinkStatuses.Selected, LinkStatuses.Final, LinkStatuses.Discarded })
        {
            List<string> group = nodes.Where(n => n.Status == status).Select(n => Id(n.Code)).ToList();
            if (group.Count > 0) mmd.AppendLine($"  class {string.Join(',', group)} {status}");
        }

        return mmd.ToString();
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..(max - 1)] + "…";

    private static string Escape(string value) =>
        value.Replace("\"", "#quot;").Replace("\r", " ").Replace("\n", " ")
             .Replace("<", "#lt;").Replace(">", "#gt;");
}
