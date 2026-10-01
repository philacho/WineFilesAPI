namespace WineFilesApi.Application.Interfaces;

public interface IQueryRepository
{
    Task<IReadOnlyList<IDictionary<string, object?>>> GetBlendBatchSummaryAsync(
        string? blend, string? batch, decimal? minVolume, int limit, CancellationToken ct);

    Task<IReadOnlyList<IDictionary<string, object?>>> GetVesselOpMasterAsync(
        decimal? capacityLow, decimal? capacityHigh, string? vessel,
        string? blend, int limit, CancellationToken ct);
}