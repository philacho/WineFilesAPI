using WineFilesApi.Domain.Entities;

namespace WineFilesApi.Application.Interfaces;

public interface IBlendRepository
{
    Task<IReadOnlyList<Blend>> GetAsync(string? search, CancellationToken ct);
    Task<Blend?> GetByIdAsync(string blend, CancellationToken ct);
    Task<bool> InsertAsync(Blend blend, CancellationToken ct);
    Task<bool> UpdateAsync(Blend blend, CancellationToken ct);
    Task<bool> DeleteAsync(string blend, CancellationToken ct);
}
