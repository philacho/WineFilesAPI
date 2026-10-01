using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Domain.Entities;

namespace WineFilesApi.Application.Services;

public sealed class BatchService(IBatchRepository repo) : IBatchService
{
    private static readonly FieldSelector Fields = new(new Dictionary<string, string>
    {
        ["batch"] = "FCBATCH",
        ["description"] = "FCDESCRIPT",
        ["active"] = "FCACTIVE",
        ["userLock"] = "FCUSERLOCK"
    });

    public async Task<IReadOnlyList<BatchDto>> GetAsync(string? search, string? fields, CancellationToken ct)
    {
        var columns = Fields.Resolve(fields);
        var rows = await repo.GetAsync(search, columns, ct);
        return rows.Select(Map).ToList();
    }

    public async Task<BatchDto?> GetByIdAsync(string batch, string? fields, CancellationToken ct)
    {
        var columns = Fields.Resolve(fields);
        var item = await repo.GetByIdAsync(batch, columns, ct);
        return item is null ? null : Map(item);
    }

    public Task<bool> InsertAsync(UpsertBatchDto r, CancellationToken ct)
        => repo.InsertAsync(ToEntity(r), ct);

    public Task<bool> UpdateAsync(string batch, UpsertBatchDto r, CancellationToken ct)
    {
        var item = ToEntity(r);
        item.BatchCode = batch.Trim();
        return repo.UpdateAsync(item, ct);
    }

    public Task<bool> DeleteAsync(string batch, CancellationToken ct)
        => repo.DeleteAsync(batch.Trim(), ct);

    private static Batch ToEntity(UpsertBatchDto r) => new()
    {
        BatchCode = r.Batch.Trim(),
        Description = r.Description.Trim(),
        Active = r.Active.Trim(),
        UserLock = r.UserLock.Trim()
    };

    private static BatchDto Map(Batch x) => new()
    {
        Batch = x.BatchCode,
        Description = x.Description,
        Active = x.Active.Equals("Y", StringComparison.OrdinalIgnoreCase),
        UserLock = x.UserLock
    };
}