using WineFilesApi.Application.DTOs;

namespace WineFilesApi.Application.Interfaces;

public interface IQueryService
{
    Task<IReadOnlyList<IDictionary<string, object?>>> GetBlendBatchSummaryAsync(
        BlendBatchSummaryQuery request, CancellationToken ct);

    Task<IReadOnlyList<IDictionary<string, object?>>> GetVesselOpMasterAsync(
        VesselOpMasterQuery request, CancellationToken ct);
}