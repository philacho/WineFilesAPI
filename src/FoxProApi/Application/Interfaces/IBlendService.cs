using WineFilesApi.Application.DTOs;

namespace WineFilesApi.Application.Interfaces;

public interface IBlendService
{
    Task<IReadOnlyList<BlendDto>> GetAsync(string? search, CancellationToken ct);
    Task<BlendDto?> GetByIdAsync(string blend, CancellationToken ct);
    Task<bool> InsertAsync(UpsertBlendDto request, CancellationToken ct);
    Task<bool> UpdateAsync(string blend, UpsertBlendDto request, CancellationToken ct);
    Task<bool> DeleteAsync(string blend, CancellationToken ct);
}
