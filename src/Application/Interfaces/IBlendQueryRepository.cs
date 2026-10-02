using WineFilesApi.Application.DTOs;

namespace WineFilesApi.Application.Interfaces;

public interface IBlendQueryRepository
{
    Task<IReadOnlyList<BlendVesselDto>> GetBlendVesselsAsync(
        string blend, string? batch, bool complete, CancellationToken ct);

    Task<IReadOnlyList<BlendCompositionRowDto>> GetBlendCompositionAsync(
        string blend, string? batch, string appLevel, CancellationToken ct);

    Task<IReadOnlyList<BlendAdditiveRowDto>> GetBlendAdditivesAsync(
        string blend, string? batch, string appLevel, CancellationToken ct);
}