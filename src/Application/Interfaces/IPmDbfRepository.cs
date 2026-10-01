using WineFilesApi.Domain.Entities;

namespace WineFilesApi.Application.Interfaces;

public interface IPmDbfRepository
{
    Task<IReadOnlyList<PmDbf>> GetAsync(string? search, CancellationToken ct);
    Task<PmDbf?> GetByIdAsync(string tableName, CancellationToken ct);
}