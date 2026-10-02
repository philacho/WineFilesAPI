using WineFilesApi.Application.DTOs;

namespace WineFilesApi.Application.Interfaces;

public interface IVesselQueryRepository
{
    Task<IReadOnlyList<VesselCompositionRowDto>> GetVesselCompositionAsync(
        string vessel, int lastOp, bool fromCn, string appLevel, CancellationToken ct);
}