using WineFilesApi.Application.DTOs;

namespace WineFilesApi.Application.Interfaces;

public interface IPmFieldService
{
    Task<IReadOnlyList<PmFieldDto>> GetAsync(string? table, CancellationToken ct);
    Task<IReadOnlyList<PmFieldDto>> GetByTableAsync(string table, CancellationToken ct);
}