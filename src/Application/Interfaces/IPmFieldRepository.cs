using WineFilesApi.Domain.Entities;

namespace WineFilesApi.Application.Interfaces;

public interface IPmFieldRepository
{
    Task<IReadOnlyList<PmField>> GetAsync(string? table, CancellationToken ct);
    Task<IReadOnlyList<PmField>> GetByTableAsync(string table, CancellationToken ct);
}