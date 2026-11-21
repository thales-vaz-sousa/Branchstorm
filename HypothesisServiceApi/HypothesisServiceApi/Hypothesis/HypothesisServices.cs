using System.Collections.Concurrent;

namespace HypothesisServiceApi.Hypothesis;

public interface IHypothesisRepository
{
    Task<Hypothesis> CreateAsync(Hypothesis hypothesis);
    Task<Hypothesis?> FindByCodeAsync(Guid code);
    Task<IReadOnlyList<Hypothesis>> FindByLinkCodeAsync(Guid linkCode);
    Task UpdateAsync(Hypothesis hypothesis);
}

public sealed class InMemoryHypothesisRepository : IHypothesisRepository
{
    private readonly ConcurrentDictionary<Guid, Hypothesis> hypotheses = new();

    public Task<Hypothesis> CreateAsync(Hypothesis hypothesis)
    {
        hypotheses[hypothesis.Code] = hypothesis;
        return Task.FromResult(hypothesis);
    }

    public Task<Hypothesis?> FindByCodeAsync(Guid code)
    {
        hypotheses.TryGetValue(code, out var hypothesis);
        return Task.FromResult(hypothesis);
    }

    public Task<IReadOnlyList<Hypothesis>> FindByLinkCodeAsync(Guid linkCode)
    {
        IReadOnlyList<Hypothesis> result = hypotheses.Values
            .Where(item => item.LinkCode == linkCode)
            .OrderBy(item => item.CreatedAtUtc)
            .ToArray();

        return Task.FromResult(result);
    }

    public Task UpdateAsync(Hypothesis hypothesis)
    {
        hypotheses[hypothesis.Code] = hypothesis;
        return Task.CompletedTask;
    }
}

public interface IHypothesisService
{
    Task<Hypothesis> CreateAsync(CreateHypothesisRequest request);
    Task<IReadOnlyList<Hypothesis>> FindByLinkCodeAsync(Guid linkCode);
    Task<Hypothesis?> RegisterResultAsync(
        Guid code,
        RegisterHypothesisResultRequest request);
}

public sealed class HypothesisService(
    IHypothesisRepository repository) : IHypothesisService
{
    public Task<Hypothesis> CreateAsync(CreateHypothesisRequest request)
    {
        var hypothesis = new Hypothesis
        {
            LinkCode = request.LinkCode,
            Description = request.Description.Trim(),
            SuccessCriteria = request.SuccessCriteria.Trim()
        };

        return repository.CreateAsync(hypothesis);
    }

    public Task<IReadOnlyList<Hypothesis>> FindByLinkCodeAsync(Guid linkCode)
    {
        return repository.FindByLinkCodeAsync(linkCode);
    }

    public async Task<Hypothesis?> RegisterResultAsync(
        Guid code,
        RegisterHypothesisResultRequest request)
    {
        var hypothesis = await repository.FindByCodeAsync(code);

        if (hypothesis is null)
        {
            return null;
        }

        hypothesis.Status = request.Result == HypothesisResult.Validated
            ? HypothesisStatus.Validated
            : HypothesisStatus.Refuted;
        hypothesis.ResultDescription = request.ResultDescription.Trim();
        hypothesis.EvaluatedAtUtc = DateTime.UtcNow;

        await repository.UpdateAsync(hypothesis);
        return hypothesis;
    }
}