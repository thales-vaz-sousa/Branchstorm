using BffApiClassLibrary.Clients.Idea;
using BffApiClassLibrary.Common;

namespace BffApiClassLibrary.Services.Ideas;

public static class LinkPathResolver
{
    private const int MaxDepth = 64;

    /// <summary>
    /// Sobe pelo FatherCode ate a raiz. Devolve os ancestrais na ordem raiz → pai (sem o proprio no).
    /// O serviço de Ideias não tem consulta "para cima", entao sao chamadas GET /api/idea/{code} em sequencia.
    /// </summary>
    public static async Task<IReadOnlyList<LinkResult>> GetAncestorsAsync(
        IIdeaApiClient ideas, LinkResult link, CancellationToken ct)
    {
        List<LinkResult> ancestors = [];
        HashSet<string> visited = [link.Code];
        string? fatherCode = link.FatherCode;

        while (!string.IsNullOrWhiteSpace(fatherCode) && ancestors.Count < MaxDepth)
        {
            if (!visited.Add(fatherCode))
            {
                throw new BffException(502, "Árvore inconsistente",
                    $"Ciclo detectado ao subir a árvore a partir de '{link.Code}'.");
            }

            LinkResult? father = await ideas.FindOneAsync(fatherCode, ct);
            if (father is null) break; // pai removido/inconsistente: para no ultimo encontrado

            ancestors.Add(father);
            fatherCode = father.FatherCode;
        }

        ancestors.Reverse();
        return ancestors;
    }

    public static async Task<LinkResult> FindRequiredAsync(IIdeaApiClient ideas, string code, CancellationToken ct) =>
        await ideas.FindOneAsync(code, ct) ?? throw BffException.NotFound("Nó (link)", code);
}
