using WineFilesApi.Application.DTOs;
using WineFilesApi.Application.Interfaces;
using WineFilesApi.Domain.Entities;

namespace WineFilesApi.Application.Services;

public sealed class PmIndexService : IPmIndexService
{
    private readonly IPmIndexRepository _repo;

    public PmIndexService(IPmIndexRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<PmIndexDto>> GetAsync(string? search, CancellationToken ct)
        => (await _repo.GetAsync(search, ct)).Select(Map).ToList();

    public async Task<IReadOnlyList<PmIndexDto>> GetByTableAsync(string table, CancellationToken ct)
        => (await _repo.GetByTableAsync(table, ct)).Select(Map).ToList();

    private static PmIndexDto Map(PmIndex x) => new()
    {
        CdxName = x.CdxName,
        TagName = x.TagName,
        TagKey = x.TagKey,
        Unique = x.Unique,
        Descending = x.Descending
    };
}