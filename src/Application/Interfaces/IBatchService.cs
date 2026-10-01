using WineFilesApi.Application.DTOs;

namespace WineFilesApi.Application.Interfaces;

public interface IBatchService
{
    Task<IReadOnlyList<BatchDto>> GetAsync(
        string? search, string? fields, CancellationToken ct);

    Task<BatchDto?> GetByIdAsync(
        string batch, string? fields, CancellationToken ct);

    Task<bool> InsertAsync(UpsertBatchDto request, CancellationToken ct);
    Task<bool> UpdateAsync(string batch, UpsertBatchDto request, CancellationToken ct);
    Task<bool> DeleteAsync(string batch, CancellationToken ct);
}