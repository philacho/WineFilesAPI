using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Domain.Entities;

namespace WineFilesApi.Application.Services;

public sealed class PmDbfService : IPmDbfService
{
    private readonly IPmDbfRepository _repo;

    public PmDbfService(IPmDbfRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<PmDbfDto>> GetAsync(string? search, CancellationToken ct)
        => (await _repo.GetAsync(search, ct)).Select(Map).ToList();

    public async Task<PmDbfDto?> GetByIdAsync(string tableName, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(tableName, ct);
        return item is null ? null : Map(item);
    }

    private static PmDbfDto Map(PmDbf x) => new()
    {
        Table = x.TableName,
        Cdx = x.CdxName,
        Type = x.TableType,
        ParentTable = x.ParentTable,
        LinkField = x.LinkField,
        Description = x.Description
    };
}