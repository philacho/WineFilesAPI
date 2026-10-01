using WineFilesApi.Domain.Entities;

namespace WineFilesApi.Application.Interfaces;

public interface IBatchRepository
{
    Task<IReadOnlyList<Batch>> GetAsync(
        string? search, IReadOnlyList<string> columns, CancellationToken ct);

    Task<Batch?> GetByIdAsync(
        string batch, IReadOnlyList<string> columns, CancellationToken ct);

    Task<bool> InsertAsync(Batch batch, CancellationToken ct);
    Task<bool> UpdateAsync(Batch batch, CancellationToken ct);
    Task<bool> DeleteAsync(string batch, CancellationToken ct);
}