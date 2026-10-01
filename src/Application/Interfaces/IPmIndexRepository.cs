using WineFilesApi.Domain.Entities;

namespace WineFilesApi.Application.Interfaces;

public interface IPmIndexRepository
{
    Task<IReadOnlyList<PmIndex>> GetAsync(string? search, CancellationToken ct);
    Task<IReadOnlyList<PmIndex>> GetByTableAsync(string table, CancellationToken ct);
}