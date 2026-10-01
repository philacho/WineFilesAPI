using WineFilesApi.Application.DTOs;

namespace WineFilesApi.Application.Interfaces;

public interface IPmDbfService
{
    Task<IReadOnlyList<PmDbfDto>> GetAsync(string? search, CancellationToken ct);
    Task<PmDbfDto?> GetByIdAsync(string tableName, CancellationToken ct);
}