using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;

namespace WineFilesApi.Application.Services;

public sealed class QueryService : IQueryService
{
    private readonly IQueryRepository _repo;

    public QueryService(IQueryRepository repo) => _repo = repo;

    public Task<IReadOnlyList<IDictionary<string, object?>>> GetBlendBatchSummaryAsync(
        BlendBatchSummaryQuery request, CancellationToken ct)
        => _repo.GetBlendBatchSummaryAsync(
            request.Blend, request.Batch, request.MinVolume, request.Limit, ct);

    public Task<IReadOnlyList<IDictionary<string, object?>>> GetVesselOpMasterAsync(
        VesselOpMasterQuery request, CancellationToken ct)
        => _repo.GetVesselOpMasterAsync(
            request.CapacityLow, request.CapacityHigh,
            request.Vessel, request.Blend, request.Limit, ct);
}