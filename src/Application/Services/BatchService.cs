using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Domain.Entities;

namespace WineFilesApi.Application.Services;

public sealed class BatchService : IBatchService
{
    private readonly IBatchRepository _repo;

    public BatchService(IBatchRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<BatchDto>> GetAsync(string? search, CancellationToken ct)
        => (await _repo.GetAsync(search, ct)).Select(Map).ToList();

    public async Task<BatchDto?> GetByIdAsync(string batch, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(batch, ct);
        return item is null ? null : Map(item);
    }

    public Task<bool> InsertAsync(UpsertBatchDto request, CancellationToken ct)
        => _repo.InsertAsync(new Batch {
            BatchCode = request.Batch.Trim(),
            Description = request.Description.Trim(),
            Active = request.Active.Trim(),
            UserLock = request.UserLock.Trim()
        }, ct);

    public Task<bool> UpdateAsync(string batch, UpsertBatchDto request, CancellationToken ct)
        => _repo.UpdateAsync(new Batch {
            BatchCode = batch.Trim(),
            Description = request.Description.Trim(),
            Active = request.Active.Trim(),
            UserLock = request.UserLock.Trim()
        }, ct);

    public Task<bool> DeleteAsync(string batch, CancellationToken ct)
        => _repo.DeleteAsync(batch.Trim(), ct);

    private static BatchDto Map(Batch x) => new() {
        Batch = x.BatchCode,
        Description = x.Description,
        Active = x.Active.Equals("Y", StringComparison.OrdinalIgnoreCase),
        UserLock = x.UserLock
    };
}
