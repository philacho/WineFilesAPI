using WineFilesApi.Application.DTOs;

namespace WineFilesApi.Application.Interfaces;

public interface IPmIndexService
{
    Task<IReadOnlyList<PmIndexDto>> GetAsync(string? search, CancellationToken ct);
    Task<IReadOnlyList<PmIndexDto>> GetByTableAsync(string table, CancellationToken ct);
}