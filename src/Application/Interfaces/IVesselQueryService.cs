using WineFilesApi.Application.DTOs;

namespace WineFilesApi.Application.Interfaces;

public interface IVesselQueryService
{
    Task<IReadOnlyList<VesselCompositionRowDto>> GetVesselCompositionAsync(
        string vessel, int lastOp, bool fromCn, string appLevel, CancellationToken ct);
}